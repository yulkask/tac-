using System.Text.Json;
using TacTickle.Core;

namespace TacTickle.Data;

/// <summary>
/// Сервис для сохранения и загрузки игр (Data слой).
/// </summary>
public sealed class SaveLoadService
{
    private const string SavesDirectory = "saves";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SaveLoadService()
    {
        Directory.CreateDirectory(SavesDirectory);
    }

    /// <summary>
    /// Сохраняет состояние игры в файл.
    /// </summary>
    public async Task SaveGameAsync(GameState gameState, string saveName)
    {
        var data = new SaveGameData
        {
            Player1 = gameState.Players[0],
            Player2 = gameState.Players[1],
            CurrentPlayerIndex = gameState.CurrentPlayerIndex,
            MoveCount = gameState.MoveCount,
            BoardState = ExtractBoardState(gameState.Board),
            SavedAt = DateTime.Now,
            SaveName = saveName
        };

        var fileName = GetSaveFileName(saveName);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(fileName, json);
    }

    /// <summary>
    /// Загружает состояние игры из файла.
    /// </summary>
    public async Task<GameState?> LoadGameAsync(string saveName)
    {
        var fileName = GetSaveFileName(saveName);
        if (!File.Exists(fileName))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(fileName);
        var data = JsonSerializer.Deserialize<SaveGameData>(json);
        if (data == null)
        {
            return null;
        }

        var gameState = new GameState(data.Player1, data.Player2, data.CurrentPlayerIndex);
        gameState.SetCurrentPlayer(data.CurrentPlayerIndex);
        gameState.SetMoveCount(data.MoveCount);
        gameState.Board.RestoreBoardState(data.BoardState);

        return gameState;
    }

    /// <summary>
    /// Получает список всех сохранённых игр.
    /// </summary>
    public string[] GetSavedGames()
    {
        if (!Directory.Exists(SavesDirectory))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(SavesDirectory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()
            .ToArray();
    }

    /// <summary>
    /// Удаляет сохранённую игру.
    /// </summary>
    public void DeleteSave(string saveName)
    {
        var fileName = GetSaveFileName(saveName);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    private static string GetSaveFileName(string saveName)
    {
        var safeName = string.Join("_", saveName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(SavesDirectory, $"{safeName}.json");
    }

    private static CellState[,] ExtractBoardState(GameBoard board)
    {
        return board.GetBoardState();
    }
}

