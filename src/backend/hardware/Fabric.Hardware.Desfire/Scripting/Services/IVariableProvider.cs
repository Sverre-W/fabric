namespace Fabric.Hardware.Desfire.Scripting.Services;

public interface IVariableProvider
{
    Task<byte[]> GetNextVariable(CancellationToken cancelToken = default);
}

public interface ICsvRowAwareVariableProvider
{
    Task<byte[]> GetNextVariable(IDictionary<string, string> dataSourceRow, CancellationToken cancelToken = default);
}

/// <summary>
///     A fixed value provider
/// </summary>
/// <param name="value"></param>
public class FixedVariableProvider(byte[] value) : IVariableProvider
{
    public Task<byte[]> GetNextVariable(CancellationToken cancelToken = default)
    {
        return Task.FromResult(value);
    }
}

/// <summary>
///     The provider caches the result of the underlying provider so that the underlying provider only
///     provides its variable once
/// </summary>
/// <param name="provider"></param>
public class CachedVariableProvider(IVariableProvider provider) : IVariableProvider
{
    private byte[]? _value;

    public async Task<byte[]> GetNextVariable(CancellationToken cancelToken = default)
    {
        _value ??= await provider.GetNextVariable(cancelToken);
        return _value;
    }
}
