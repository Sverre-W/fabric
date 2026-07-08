namespace Fabric.Hardware.Desfire.Encoding.Models;

/// <summary>
/// Represent a data point to be read on a Desfire card.
/// </summary>
public class ReaderProfile
{
    /// <summary>
    /// The unique identifier of the profile
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The application from which the reader will read the data
    /// </summary>
    public string ApplicationId { get; set; } = null!;

    /// <summary>
    /// The file within the application where the data is stored
    /// </summary>
    public int FileId { get; set; }

    /// <summary>
    /// The chip design to use to read the badge
    /// </summary>
    public EntityLink ChipDesign { get; set; } = null!;
}
