namespace Fabric.Hardware.Desfire.Scripting.Entities;

public class ScriptError
{
    public string Message { get; set; } = default!;

    public override string ToString()
    {
        return Message;
    }
}
