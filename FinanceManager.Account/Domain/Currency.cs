using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Account.Domain;

/// <summary>
/// Currency model 
/// </summary>
public class Currency
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Request identifier
    /// </summary>
    [Required]
    public string RequestId { get; set; } = null!;
    /// <summary>
    /// User identifier
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// Short name
    /// </summary>
    [Required]
    public string ShortName { get; set; } = null!;
    /// <summary>
    /// Name
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;
    /// <summary>
    /// Icon link
    /// </summary>
    public string? Icon { get; set; }
}