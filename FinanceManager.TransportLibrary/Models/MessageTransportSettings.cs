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
    public string Hostname { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }
}