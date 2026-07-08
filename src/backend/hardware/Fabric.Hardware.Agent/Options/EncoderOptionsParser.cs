namespace Fabric.Hardware.Agent.Options;

public static class EncoderOptionsParser
{
    public static IReadOnlyList<EncoderOptions> Parse(IConfiguration configuration)
    {
        IConfigurationSection encodersSection = configuration.GetSection($"{HardwareAgentOptions.SectionName}:Encoders");
        if (!encodersSection.Exists())
            return [];

        List<EncoderOptions> encoders = [];

        foreach (IConfigurationSection encoderSection in encodersSection.GetChildren())
        {
            string? type = encoderSection["$type"];
            if (string.Equals(type, "HumanAssistedEncoder", StringComparison.OrdinalIgnoreCase))
            {
                encoders.Add(new HumanAssistedEncoderOptions
                {
                    DeviceId = encoderSection["deviceId"] ?? string.Empty,
                    Reader = encoderSection["reader"] ?? string.Empty,
                    Implementation = ParseImplementation(encoderSection["implementation"])
                });
                continue;
            }

            if (string.Equals(type, "DispenserEncoder", StringComparison.OrdinalIgnoreCase))
            {
                encoders.Add(new DispenserEncoderOptions
                {
                    DeviceId = encoderSection["deviceId"] ?? string.Empty,
                    ComPort = encoderSection["comPort"] ?? string.Empty,
                    Reader = encoderSection["reader"] ?? string.Empty,
                    Implementation = ParseImplementation(encoderSection["implementation"]),
                    ResponseTimeout = ParseTimeSpan(encoderSection["responseTimeout"], TimeSpan.FromSeconds(5))
                });
                continue;
            }

            throw new InvalidOperationException($"Unsupported encoder type '{type ?? "<null>"}'.");
        }

        return encoders;
    }

    private static PcscEncoderImplementation ParseImplementation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PcscEncoderImplementation.Iso;

        return Enum.TryParse<PcscEncoderImplementation>(value, ignoreCase: true, out PcscEncoderImplementation implementation)
            ? implementation
            : throw new InvalidOperationException($"Unsupported encoder implementation '{value}'. Expected Iso or Native.");
    }

    private static TimeSpan ParseTimeSpan(string? value, TimeSpan defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return TimeSpan.TryParse(value, out TimeSpan parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid time span '{value}'.");
    }
}
