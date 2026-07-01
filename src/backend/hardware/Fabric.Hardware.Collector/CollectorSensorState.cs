namespace Fabric.Hardware.Collector;

public sealed class CollectorSensorState
{
    public static readonly CollectorSensorState ClearState = new([SensorStates.Normal, SensorStates.Normal]);


    private static class SensorStates
    {
            public const byte Normal = 0x80;
            public const byte Input1CollectorJam = 0x81;
            public const byte Input1FeedJam = 0x82;
            public const byte Input1TwoCards = 0x84;
            public const byte Input1Busy = 0xc0;


            public const byte Input2Sensor2 = 0x81;
            public const byte Input2Sensor1 = 0x82;
            public const byte Input2FeedSensor1 = 0x84;
            public const byte Input2FeedSensor2 = 0x88;
            public const byte Input2FeedSensor3 = 0x90;

            /// <summary>
            /// Card being collected
            /// </summary>
            public const byte Feed3AndInput1Input2 = 0x93;

            public const byte Input2SensorFull = 0xa0;
            public const byte Input2Cartridge = 0xc0;
    }


    private readonly byte _input1;
    private readonly byte _input2;


    public bool Clear => AssertExact(SensorStates.Normal, SensorStates.Normal);

    public bool CollectorJammed => Assert(SensorStates.Input1CollectorJam, SensorStates.Normal);

    public bool FeedJammed => Assert(SensorStates.Input1FeedJam, SensorStates.Normal);

    public bool Jammed => CollectorJammed || FeedJammed;


    public bool Busy => Assert(SensorStates.Input1Busy, SensorStates.Normal);

    public bool HasTwoCards => Assert(SensorStates.Input1TwoCards, SensorStates.Normal);

    public bool CollectStackFull => Assert(SensorStates.Input2SensorFull, SensorStates.Normal);

    public bool AnyFeedSensor => 0 != (_input2 & 0x1C);

    public bool AnyCollectSensor => 0 != (_input2 & 0x03);

    public bool CardBeingCollected => AssertExact(SensorStates.Normal, SensorStates.Feed3AndInput1Input2);


    public CollectorSensorState(byte[] data)
    {
        if (data.Length == 0)
            throw new ArgumentException("Must contain data", nameof(data));

        _input1 = data[0];
        _input2 = data.Length > 1 ? data[1] : (byte)0x00;
    }


    private bool AssertExact(byte inputState1, byte inputState2) =>
        _input1 == inputState1 && _input2 == inputState2;


    private bool Assert(byte inputState1, byte inputState2) =>
        inputState1 == (_input1 & inputState1)
        && inputState2 == (_input2 & inputState2);


    public bool IsFeedSensor1(bool exact) =>
        exact
            ? AssertExact(SensorStates.Normal, SensorStates.Input2FeedSensor1)
            : Assert(SensorStates.Normal, SensorStates.Input2FeedSensor1);

    public bool IsFeedSensor2(bool exact) =>
        exact
            ? AssertExact(SensorStates.Normal, SensorStates.Input2FeedSensor2)
            : Assert(SensorStates.Normal, SensorStates.Input2FeedSensor2);

    public bool IsFeedSensor3(bool exact) =>
        exact
            ? AssertExact(SensorStates.Normal, SensorStates.Input2FeedSensor3)
            : Assert(SensorStates.Normal, SensorStates.Input2FeedSensor3);


    public bool IsSensor1(bool exact) =>
        exact
            ? AssertExact(SensorStates.Normal, SensorStates.Input2Sensor1)
            : Assert(SensorStates.Normal, 0x0);

    public bool IsSensor2(bool exact) =>
        exact
            ? AssertExact(SensorStates.Normal, 0x0)
            : Assert(SensorStates.Normal, 0x0);


    public override int GetHashCode() => HashCode.Combine(_input1, _input2);

    public override string ToString() =>
        $"Input1: {_input1:X2} Input2: {_input2:X2} \n" +
        $"State: Clear: {Clear} Jammed: {Jammed} Busy: {Busy}, HasTwoCards: {HasTwoCards}, CollectStackFull: {CollectStackFull}, CardBeingCollected: {CardBeingCollected}, AnyFeedSensor: {AnyFeedSensor}, AnyCollectSensor: {AnyCollectSensor}, ";

    public override bool Equals(object? obj) => obj is CollectorSensorState state && state._input1 == _input1 && state._input2 == _input2;
    }
