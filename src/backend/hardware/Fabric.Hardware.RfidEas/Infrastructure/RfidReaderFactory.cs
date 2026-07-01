using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.RfidEas.Infrastructure;

public sealed class RfidReaderFactory
{
    public RfidEasReader Create(RfidReaderSettings settings, ILoggerFactory loggerFactory)
    {
        ICardDataReader cardFormat = settings.CardFormat switch
        {
            CardDataFormat.Hexadecimal => new HexadecimalCardReader(),
            CardDataFormat.BCD => new BcdCardReader(),
            _ => throw new ArgumentException("Must specify a supported format"),
        };

        ICardDataTransformer transformer = settings.Transformer switch
        {
            CardDataTransformer.None => new NoTransformation(),
            CardDataTransformer.InvertBits => new InvertBitsTransformation(),
            CardDataTransformer.InvertBytes => new InvertBytesTransformation(),
            _ => throw new ArgumentException("Must specify a supported transformer"),
        };

        return new RfidEasReader(
            transformer,
            cardFormat,
            loggerFactory.CreateLogger<RfidEasReader>(),
            settings.PollingDelay,
            settings.DelayAfterRead
        );
    }
}
