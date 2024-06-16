namespace FinanceManager.TransportLibrary.Models;

/// <summary>
/// Message transport settings
/// </summary>
public class MessageTransportSettings
{
    public static string SectionName => "MessageTransport";

    /// <summary>
    /// Host name
    /// </summary>
    public string Hostname { get; set; } = null!;

    /// <summary>
    /// User name
    /// </summary>
    public string User { get; set; } = null!;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = null!;
}