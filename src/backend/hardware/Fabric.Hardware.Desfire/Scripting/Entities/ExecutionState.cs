namespace Fabric.Hardware.Desfire.Scripting.Entities;

public class ExecutionState
{
    public string CardUid { get; set; } = "Unknown";
    public string SelectedApplication = "000000";
    public Dictionary<string, byte[]> Variables { get; set; } = [];
}
