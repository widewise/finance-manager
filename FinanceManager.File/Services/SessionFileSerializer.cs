using System.Text;
using Newtonsoft.Json;
using FinanceManager.File.Exceptions;
using FinanceManager.File.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.File.Services;

public interface ISessionFileSerializer
{
    Task<(IEnumerable<FileContentItem> fileItems, string fileName)> ParseFileAsync(long fileId);
    Task<long> SaveFileAsync(DateTime createDate, FileContentItem[] fileItems);
}

public class SessionFileSerializer : ISessionFileSerializer
{
    private readonly ILogger<SessionFileSerializer> _logger;
    private readonly FileAppDbContext _fileAppDbContext;

    public SessionFileSerializer(
        ILogger<SessionFileSerializer> logger,
        FileAppDbContext fileAppDbContext)
    {
        _logger = logger;
        _fileAppDbContext = fileAppDbContext;
    }

    public async Task<(IEnumerable<FileContentItem> fileItems, string fileName)> ParseFileAsync(long fileId)
    {
        var file = await _fileAppDbContext.Files.FirstOrDefaultAsync(x => x.Id == fileId);
        if (file == null)
        {
            _logger.LogError("File with id {FileId} is not found", fileId);
            throw new ImportDataException();
        }

        var result = new List<FileContentItem>();
        var memoryStream = new MemoryStream(file.FileContent);
        using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
        await using var reader = new JsonTextReader(streamReader);
        reader.SupportMultipleContent = true;
        var serializer = new JsonSerializer();
        while (await reader.ReadAsync())
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var item = serializer.Deserialize<FileContentItem>(reader);
                if (item == null) continue;
                result.Add(item);
            }
        }

        return (result.ToArray(), file.FileName);
    }

    public async Task<long> SaveFileAsync(DateTime createDate, FileContentItem[] fileItems)
    {
        var createdEntity = await _fileAppDbContext.Files.AddAsync(new Models.File
        {
            FileName = $"ExportData_{createDate.ToShortDateString()}",
            FileContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fileItems))
        });

        await _fileAppDbContext.SaveChangesAsync();

        return createdEntity.Entity.Id;
    }
}