using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace TacTickle.Core;

/// <summary>
/// DTO (Data Transfer Object) для сохранения состояния игры в JSON.
/// </summary>
public sealed class GameSaveData
{
    /// <summary>
    /// Форматная версия сохранения (увеличивается при несовместимых изменениях схемы).
    /// </summary>
    [JsonPropertyName("formatVersion")]
    public int FormatVersion { get; set; } = SaveLoadManager.CURRENT_FORMAT_VERSION;

    /// <summary>
    /// Минимально совместимая версия формата, с которой это приложение может прочитать файл.
    /// </summary>
    [JsonPropertyName("minCompatibleVersion")]
    public int MinCompatibleVersion { get; set; } = SaveLoadManager.MIN_COMPATIBLE_FORMAT_VERSION;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("player1Name")]
    public string Player1Name { get; set; } = string.Empty;

    [JsonPropertyName("player2Name")]
    public string Player2Name { get; set; } = string.Empty;

    [JsonPropertyName("currentPlayerIndex")]
    public int CurrentPlayerIndex { get; set; }

    [JsonPropertyName("moveCount")]
    public int MoveCount { get; set; }

    [JsonPropertyName("boardState")]
    public int[] BoardState { get; set; } = Array.Empty<int>();

    [JsonPropertyName("moveHistory")]
    public MoveDto[] MoveHistory { get; set; } = Array.Empty<MoveDto>();
}

/// <summary>
/// DTO для сохранения информации о ходе.
/// </summary>
public sealed class MoveDto
{
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = string.Empty;

    [JsonPropertyName("playerColor")]
    public string PlayerColor { get; set; } = string.Empty;

    [JsonPropertyName("fromRow")]
    public int FromRow { get; set; }

    [JsonPropertyName("fromColumn")]
    public int FromColumn { get; set; }

    [JsonPropertyName("toRow")]
    public int ToRow { get; set; }

    [JsonPropertyName("toColumn")]
    public int ToColumn { get; set; }

    [JsonPropertyName("moveNumber")]
    public int MoveNumber { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Управляет сохранением и загрузкой состояния игры в JSON формате.
/// </summary>
public sealed class SaveLoadManager
{
    // Current save format version for this application; bump when making incompatible schema changes.
    public const int CURRENT_FORMAT_VERSION = 1;

    // Minimum compatible format version this app can read. If a save has a lower version, it's incompatible.
    public const int MIN_COMPATIBLE_FORMAT_VERSION = 1;

    private readonly string _savesDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public SaveLoadManager(string savesDirectory = "./saves")
    {
        _savesDirectory = savesDirectory ?? "./saves";
        if (!Directory.Exists(_savesDirectory))
        {
            Directory.CreateDirectory(_savesDirectory);
        }
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Возвращает список файлов сохранений (только имена файлов, без путей).
    /// </summary>
    public string[] GetSaveFiles()
    {
        if (!Directory.Exists(_savesDirectory)) return Array.Empty<string>();
        // Directory.GetFiles/Path.GetFileName can be annotated as returning nullable strings
        // on some target frameworks. Normalize and filter nulls explicitly to satisfy
        // the non-nullable return type and avoid CS8619.
        var files = Directory.GetFiles(_savesDirectory, "*.json") ?? Array.Empty<string>();
        var names = files
            .Select(Path.GetFileName)
            .Where(n => n != null)
            .Select(n => n!)
            .OrderByDescending(n => n)
            .ToArray();

        return names;
    }

    /// <summary>
    /// Сохраняет состояние игры в JSON файл.
    /// Возвращает путь к сохранённому файлу.
    /// </summary>
    public string SaveGame(GameState gameState, string filename = "")
    {
        if (gameState == null)
            throw new ArgumentNullException(nameof(gameState));

        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = $"save_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        }
        else if (!filename.EndsWith(".json"))
        {
            filename += ".json";
        }

        var filepath = Path.Combine(_savesDirectory, filename);

        // Build DTO from GameState
        var board2d = gameState.Board.GetBoardState();
        var boardState = new int[GameBoard.Rows * GameBoard.Columns];
        int idx = 0;
        for (int r = 0; r < GameBoard.Rows; r++)
        {
            for (int c = 0; c < GameBoard.Columns; c++)
            {
                boardState[idx++] = (int)board2d[r, c];
            }
        }

        var saveData = new GameSaveData
        {
            FormatVersion = CURRENT_FORMAT_VERSION,
            MinCompatibleVersion = MIN_COMPATIBLE_FORMAT_VERSION,
            Timestamp = DateTime.UtcNow,
            Player1Name = gameState.Players[0],
            Player2Name = gameState.Players[1],
            CurrentPlayerIndex = gameState.CurrentPlayerIndex,
            MoveCount = gameState.MoveCount,
            BoardState = boardState,
            MoveHistory = Array.Empty<MoveDto>()
        };

        var json = JsonSerializer.Serialize(saveData, _jsonOptions);
        File.WriteAllText(filepath, json);
        return filepath;
    }

    /// <summary>
    /// Загружает состояние игры из JSON файла.
    /// </summary>
    public GameState LoadGame(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be empty.", nameof(filename));

        if (!filename.EndsWith(".json"))
            filename += ".json";

        var filepath = Path.Combine(_savesDirectory, filename);
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"Save file not found: {filepath}");

        var json = File.ReadAllText(filepath);

        // Deserialize save DTO
        GameSaveData saveData;
        try
        {
            saveData = JsonSerializer.Deserialize<GameSaveData>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize save data.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Save file is not valid JSON or has unexpected format.", ex);
        }

        // Compatibility checks
        if (saveData.FormatVersion < MIN_COMPATIBLE_FORMAT_VERSION)
        {
            throw new InvalidOperationException($"Save format version {saveData.FormatVersion} is too old and incompatible (minimum supported: {MIN_COMPATIBLE_FORMAT_VERSION}).");
        }

        if (saveData.FormatVersion > CURRENT_FORMAT_VERSION)
        {
            throw new InvalidOperationException($"Save format version {saveData.FormatVersion} is newer than application supports (current: {CURRENT_FORMAT_VERSION}). Please update the application.");
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(saveData.Player1Name) || string.IsNullOrWhiteSpace(saveData.Player2Name))
        {
            throw new InvalidOperationException("Save file missing player names.");
        }

        if (saveData.BoardState == null || saveData.BoardState.Length != GameBoard.Rows * GameBoard.Columns)
        {
            throw new InvalidOperationException("Save file has invalid board state.");
        }

        if (saveData.CurrentPlayerIndex < 0 || saveData.CurrentPlayerIndex > 1)
        {
            throw new InvalidOperationException("Save file has invalid current player index.");
        }

        if (saveData.MoveCount < 0)
        {
            throw new InvalidOperationException("Save file has invalid move count.");
        }

        // Create new GameState and restore
        var gameState = new GameState(saveData.Player1Name, saveData.Player2Name, 0);
        // Reset to ensure clean state
        gameState.Reset();

        // Restore board
        var boardArray = new CellState[GameBoard.Rows, GameBoard.Columns];
        int id = 0;
        for (int r = 0; r < GameBoard.Rows; r++)
        {
            for (int c = 0; c < GameBoard.Columns; c++)
            {
                boardArray[r, c] = (CellState)saveData.BoardState[id++];
            }
        }
        gameState.Board.RestoreBoardState(boardArray);

        // Restore counters
        gameState.SetCurrentPlayer(saveData.CurrentPlayerIndex);
        gameState.SetMoveCount(saveData.MoveCount);

        return gameState;
    }

    /// <summary>
    /// Преобразует состояние доски в массив целых чисел.
    /// </summary>
    private static int[] SerializeBoardState(GameBoard board)
    {
        var state = new int[GameBoard.Rows * GameBoard.Columns];
        int index = 0;

        for (int row = 0; row < GameBoard.Rows; row++)
        {
            for (int col = 0; col < GameBoard.Columns; col++)
            {
                state[index++] = (int)board[row, col];
            }
        }

        return state;
    }
}
