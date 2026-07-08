using System.Globalization;
using System.Text;
using System.Text.Json;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireVariableResolver(DesfireDbContext db, TimeProvider timeProvider)
{
    public async Task<ResolvedDesfireVariables> ResolveAsync(IReadOnlyList<string> requiredVariables, IReadOnlyList<TransformationVariableConfigRequest> variables, string inputJson, CancellationToken ct)
    {
        List<EncodingVariableRequest> internalConfigs = [];
        foreach (TransformationVariableConfigRequest variable in variables)
        {
            internalConfigs.Add(await ToInternalConfigAsync(variable, ct));
        }

        return await ResolveAsync(requiredVariables, internalConfigs, inputJson, ct);
    }

    public async Task<ResolvedDesfireVariables> ResolveAsync(IReadOnlyList<string> requiredVariables, IReadOnlyList<EncodingVariableRequest> variables, string inputJson, CancellationToken ct)
    {
        using JsonDocument document = JsonDocument.Parse(inputJson);
        JsonElement input = document.RootElement.Clone();
        Dictionary<string, EncodingVariableRequest> configs = variables.ToDictionary(variable => variable.Name, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, byte[]> resolved = [];
        List<ResolvedVariableAudit> audit = [];

        foreach (string required in requiredVariables)
        {
            if (!configs.TryGetValue(required, out EncodingVariableRequest? config))
                throw new InvalidOperationException($"Variable '{required}' is required but has no provider config.");

            byte[] value = await ResolveVariableAsync(config, input, ct);
            resolved[required] = value;
            audit.Add(new ResolvedVariableAudit(required, config.Provider.Type.ToString(), config.Format.Type.ToString(), Convert.ToHexString(value)));
        }

        return new ResolvedDesfireVariables(resolved, JsonSerializer.Serialize(audit, DesfireJson.Options));
    }

    private async Task<byte[]> ResolveVariableAsync(EncodingVariableRequest config, JsonElement input, CancellationToken ct)
    {
        string raw = await ResolveProviderValueAsync(config.Provider, input, ct);
        return await FormatAsync(raw, config.Format, input, ct);
    }

    private async Task<string> ResolveProviderValueAsync(VariableProviderRequest provider, JsonElement input, CancellationToken ct) => provider.Type switch
    {
        DesfireVariableProviderKind.Provided => ReadInputString(input, provider.Field ?? throw new InvalidOperationException("Provided variable requires field.")),
        DesfireVariableProviderKind.Fixed => provider.Value ?? throw new InvalidOperationException("Fixed variable requires value."),
        DesfireVariableProviderKind.Sequence when provider.SystemProviderId is not null => (await TakeSystemProviderSequenceAsync(provider.SystemProviderId.Value, ct)).ToString(CultureInfo.InvariantCulture),
        DesfireVariableProviderKind.Sequence => (await TakeSequenceAsync(provider.SequenceName, provider.InitialValue ?? 1, ct)).ToString(CultureInfo.InvariantCulture),
        _ => throw new InvalidOperationException($"Unsupported variable provider '{provider.Type}'.")
    };

    private async Task<EncodingVariableRequest> ToInternalConfigAsync(TransformationVariableConfigRequest config, CancellationToken ct)
    {
        if (config.Kind == TransformationVariableKind.UserProvided)
            return new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Provided, Field: string.IsNullOrWhiteSpace(config.Field) ? config.Name : config.Field), config.Format);

        if (config.SystemProviderId is not null)
        {
            DesfireSystemProvider provider = await db.SystemProviders.AsNoTracking().SingleOrDefaultAsync(provider => provider.Id == config.SystemProviderId.Value, ct)
                ?? throw new InvalidOperationException($"System provider '{config.SystemProviderId}' was not found.");

            return provider.ProviderType switch
            {
                SystemVariableProviderKind.Fixed => new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Fixed, Value: provider.FixedValue ?? string.Empty), config.Format),
                SystemVariableProviderKind.Sequence => new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Sequence, SystemProviderId: provider.Id), config.Format),
                _ => throw new InvalidOperationException($"Unsupported system provider '{provider.ProviderType}'.")
            };
        }

        return config.SystemProvider switch
        {
            SystemVariableProviderKind.Fixed => new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Fixed, Value: config.Value ?? string.Empty), config.Format),
            SystemVariableProviderKind.Sequence => new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Sequence, SequenceName: string.IsNullOrWhiteSpace(config.SequenceName) ? config.Name : config.SequenceName, InitialValue: config.InitialValue ?? 1), config.Format),
            _ => new EncodingVariableRequest(config.Name, new VariableProviderRequest(DesfireVariableProviderKind.Provided, Field: string.IsNullOrWhiteSpace(config.Field) ? config.Name : config.Field), config.Format)
        };
    }

    private async Task<byte[]> FormatAsync(string raw, VariableFormatRequest format, JsonElement input, CancellationToken ct) => format.Type switch
    {
        DesfireVariableFormatKind.Hex => Convert.FromHexString(RemoveHexPrefix(raw)),
        DesfireVariableFormatKind.Text => Encoding.UTF8.GetBytes(raw),
        DesfireVariableFormatKind.UInt => FormatUInt(raw, format.Length),
        DesfireVariableFormatKind.PaddedDecimal => Encoding.ASCII.GetBytes(ParseUInt(raw).ToString(CultureInfo.InvariantCulture).PadLeft(format.Length ?? 0, '0')),
        DesfireVariableFormatKind.PaddedHex => Convert.FromHexString(ParseUInt(raw).ToString("X", CultureInfo.InvariantCulture).PadLeft(format.Length ?? 0, '0')),
        DesfireVariableFormatKind.GenericWiegand => await FormatWiegandAsync(format.Wiegand ?? throw new InvalidOperationException("Generic Wiegand format requires config."), input, ct),
        _ => throw new InvalidOperationException($"Unsupported variable format '{format.Type}'.")
    };

    private async Task<byte[]> FormatWiegandAsync(GenericWiegandFormatRequest config, JsonElement input, CancellationToken ct)
    {
        if (config.BitLength < 1)
            throw new InvalidOperationException("Wiegand bit length must be greater than zero.");

        bool[] bits = new bool[config.BitLength];
        bool[] occupied = new bool[config.BitLength];

        foreach (WiegandFieldRequest field in config.Fields)
        {
            ValidateRange(field.Offset, field.Length, config.BitLength, field.Name);
            for (int i = field.Offset; i < field.Offset + field.Length; i++)
            {
                if (occupied[i])
                    throw new InvalidOperationException($"Wiegand field '{field.Name}' overlaps another field.");
                occupied[i] = true;
            }

            ulong value = await ResolveWiegandFieldValueAsync(field, input, ct);
            if (field.Length < 64 && value >= (1UL << field.Length))
                throw new InvalidOperationException($"Wiegand field '{field.Name}' value does not fit in {field.Length} bits.");

            for (int bit = 0; bit < field.Length; bit++)
            {
                int target = field.Offset + field.Length - 1 - bit;
                bits[target] = ((value >> bit) & 1UL) == 1UL;
            }
        }

        foreach (WiegandParityRequest parity in config.Parity)
        {
            ValidateRange(parity.Offset, 1, config.BitLength, "parity");
            ValidateRange(parity.CoversOffset, parity.CoversLength, config.BitLength, "parity coverage");
            int count = 0;
            for (int i = parity.CoversOffset; i < parity.CoversOffset + parity.CoversLength; i++)
            {
                if (bits[i])
                    count++;
            }

            bool set = parity.Kind == WiegandParityKind.Even ? count % 2 != 0 : count % 2 == 0;
            bits[parity.Offset] = set;
        }

        return PackBits(bits);
    }

    private async Task<ulong> ResolveWiegandFieldValueAsync(WiegandFieldRequest field, JsonElement input, CancellationToken ct)
    {
        string raw = field.Source switch
        {
            WiegandFieldSourceKind.Provided => ReadInputString(input, field.Field ?? throw new InvalidOperationException($"Wiegand field '{field.Name}' requires input field.")),
            WiegandFieldSourceKind.Fixed => field.Value ?? throw new InvalidOperationException($"Wiegand field '{field.Name}' requires fixed value."),
            WiegandFieldSourceKind.Sequence => (await TakeSequenceAsync(field.SequenceName, field.InitialValue ?? 1, ct)).ToString(CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"Unsupported Wiegand field source '{field.Source}'.")
        };

        return ParseUInt(raw);
    }

    private async Task<long> TakeSequenceAsync(string? sequenceName, long initialValue, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(sequenceName))
            throw new InvalidOperationException("Sequence provider requires sequenceName.");

        string normalized = sequenceName.Trim();
        string lockKey = $"desfire:variable-sequence:{db.TenantId}:{normalized}";

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({lockKey}))", ct);

        DesfireVariableSequence? sequence = await db.VariableSequences.SingleOrDefaultAsync(candidate => candidate.Name == normalized, ct);
        if (sequence is null)
        {
            sequence = DesfireVariableSequence.Create(normalized, initialValue, timeProvider.GetUtcNow());
            db.VariableSequences.Add(sequence);
        }

        long value = sequence.TakeNext(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return value;
    }

    private async Task<long> TakeSystemProviderSequenceAsync(Guid systemProviderId, CancellationToken ct)
    {
        string lockKey = $"desfire:system-provider-sequence:{db.TenantId}:{systemProviderId}";

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({lockKey}))", ct);

        DesfireSystemProvider provider = await db.SystemProviders.SingleOrDefaultAsync(provider => provider.Id == systemProviderId, ct)
            ?? throw new InvalidOperationException($"System provider '{systemProviderId}' was not found.");
        long value = provider.TakeNextValue();
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return value;
    }

    private static string ReadInputString(JsonElement input, string field)
    {
        if (!input.TryGetProperty(field, out JsonElement value))
            throw new InvalidOperationException($"Input field '{field}' is missing.");

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => throw new InvalidOperationException($"Input field '{field}' must be scalar.")
        };
    }

    private static byte[] FormatUInt(string raw, int? length)
    {
        ulong value = ParseUInt(raw);
        int bytes = length ?? GetMinimumByteCount(value);
        byte[] result = new byte[bytes];
        for (int i = bytes - 1; i >= 0; i--)
        {
            result[i] = (byte)(value & 0xFF);
            value >>= 8;
        }
        return result;
    }

    private static int GetMinimumByteCount(ulong value)
    {
        int bytes = 1;
        while ((value >>= 8) > 0)
            bytes++;
        return bytes;
    }

    private static ulong ParseUInt(string raw)
    {
        string value = RemoveHexPrefix(raw);
        return raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            ? ulong.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
            : ulong.Parse(value, CultureInfo.InvariantCulture);
    }

    private static string RemoveHexPrefix(string raw) => raw.Trim().StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? raw.Trim()[2..] : raw.Trim();

    private static void ValidateRange(int offset, int length, int totalLength, string name)
    {
        if (offset < 0 || length < 1 || offset + length > totalLength)
            throw new InvalidOperationException($"Invalid {name} bit range.");
    }

    private static byte[] PackBits(bool[] bits)
    {
        byte[] bytes = new byte[(bits.Length + 7) / 8];
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
                bytes[i / 8] |= (byte)(1 << (7 - (i % 8)));
        }
        return bytes;
    }
}

public sealed record ResolvedDesfireVariables(Dictionary<string, byte[]> Variables, string AuditJson);

public sealed record ResolvedVariableAudit(string Name, string Provider, string Format, string ValueHex);
