namespace FinanceManager.File.Models;

public class File
{
    public long Id { get; set; }
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
}