using TacTickle.Core;

namespace TacTickle.Data;

/// <summary>
/// Данные для сохранения состояния игры в файл.
/// </summary>
public sealed class SaveGameData
{
    public required string Player1 { get; init; }
    public required string Player2 { get; init; }
    public required int CurrentPlayerIndex { get; init; }
    public required int MoveCount { get; init; }
    public required CellState[,] BoardState { get; init; }
    public required DateTime SavedAt { get; init; }
    public required string SaveName { get; init; }
}

