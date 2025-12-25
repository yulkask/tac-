using System.Text.Json;

namespace TacTickle.Data;

/// <summary>
/// Сервис для работы с таблицей рекордов (Data слой).
/// </summary>
public sealed class RecordsService
{
    private const string RecordsFile = "records.json";
    private const int TopRecordsCount = 10;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Добавляет запись о завершённой игре.
    /// </summary>
    public async Task AddRecordAsync(string winner, string loser, int moveCount)
    {
        var records = await GetAllRecordsAsync();
        var newRecord = new Record
        {
            Winner = winner,
            Loser = loser,
            PlayedAt = DateTime.Now,
            MoveCount = moveCount
        };

        records.Add(newRecord);
        await SaveRecordsAsync(records);
    }

    /// <summary>
    /// Получает топ-N записей (по умолчанию топ-10).
    /// </summary>
    public async Task<Record[]> GetTopRecordsAsync(int count = TopRecordsCount)
    {
        var records = await GetAllRecordsAsync();
        return records
            .OrderBy(r => r.MoveCount)
            .ThenByDescending(r => r.PlayedAt)
            .Take(count)
            .ToArray();
    }

    /// <summary>
    /// Получает все записи.
    /// </summary>
    public async Task<List<Record>> GetAllRecordsAsync()
    {
        if (!File.Exists(RecordsFile))
        {
            return new List<Record>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(RecordsFile);
            var records = JsonSerializer.Deserialize<List<Record>>(json);
            return records ?? new List<Record>();
        }
        catch
        {
            return new List<Record>();
        }
    }

    private async Task SaveRecordsAsync(List<Record> records)
    {
        var json = JsonSerializer.Serialize(records, JsonOptions);
        await File.WriteAllTextAsync(RecordsFile, json);
    }
}

