namespace FinanceManager.File.Models;

public class File
{
    public long Id { get; set; }
    public string FileName { get; set; } = null!;
    public byte[] FileContent { get; set; } = null!;
}