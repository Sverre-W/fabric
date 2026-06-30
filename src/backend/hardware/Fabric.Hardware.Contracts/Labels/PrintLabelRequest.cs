using System.Text.Json.Nodes;

namespace Fabric.Hardware.Contracts.Labels;

public sealed record PrintLabelRequest(
    string Template,
    int Copies,
    JsonObject Data);
