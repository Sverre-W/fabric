namespace AccessControl.Unipass.Contracts;

public class UnipassOperationResponse
{
    public string Id { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}
