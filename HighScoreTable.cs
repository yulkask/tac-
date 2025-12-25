using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TacTickle.Core;

public sealed class HighScoreEntry
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("score")] public int Score { get; set; }
    [JsonPropertyName("when")] public DateTime When { get; set; }
}

public sealed class HighScoreTable
{
    private readonly string _filepath;
    private readonly JsonSerializerOptions _opts = new JsonSerializerOptions { WriteIndented = true };

    public List<HighScoreEntry> Entries { get; } = new List<HighScoreEntry>();

    public HighScoreTable(string filepath = "./saves/highscores.json")
    {
        _filepath = filepath;
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_filepath))
            {
                Entries.Clear();
                return;
            }

            var json = File.ReadAllText(_filepath);
            var arr = JsonSerializer.Deserialize<HighScoreEntry[]>(json, _opts) ?? Array.Empty<HighScoreEntry>();
            Entries.Clear();
            Entries.AddRange(arr.OrderByDescending(e => e.Score));
        }
        catch
        {
            Entries.Clear();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(_filepath) ?? "./saves";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(Entries.ToArray(), _opts);
        File.WriteAllText(_filepath, json);
    }

    public void AddEntry(string name, int score)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "Anonymous";
        var cleanName = name.Trim();

        // Ensure uniqueness by player name (case-insensitive). If an entry exists,
        // update it only if the new score is higher; otherwise do nothing.
        var existing = Entries.Find(e => string.Equals(e.Name, cleanName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            if (score > existing.Score)
            {
                existing.Score = score;
                existing.When = DateTime.UtcNow;
                Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
            }
            return;
        }

        Entries.Add(new HighScoreEntry { Name = cleanName, Score = score, When = DateTime.UtcNow });
        Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
        // keep top 10
        if (Entries.Count > 10) Entries.RemoveRange(10, Entries.Count - 10);
    }

    public HighScoreEntry[] GetTop(int count = 10) => Entries.Take(count).ToArray();
}
