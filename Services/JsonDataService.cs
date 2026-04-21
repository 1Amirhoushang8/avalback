using System.Text.Json;
using Microsoft.AspNetCore.Hosting;        
using System.IO;                           
using AvalWebBack.Models;

namespace AvalWebBack.Services;

public class JsonDataService
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions _writeOptions = new() { WriteIndented = true };

    public JsonDataService(IWebHostEnvironment env)
    {
        var dataFolder = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        _filePath = Path.Combine(dataFolder, "db.json");
    }

    public async Task<Database> ReadAsync()
    {
        if (!File.Exists(_filePath))
            return new Database();

        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<Database>(json) ?? new Database();
    }

    public async Task WriteAsync(Database data)
    {
        var json = JsonSerializer.Serialize(data, _writeOptions);
        await File.WriteAllTextAsync(_filePath, json);
    }
}