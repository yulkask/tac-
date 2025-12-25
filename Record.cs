namespace TacTickle.Data;

/// <summary>
/// Запись в таблице рекордов.
/// </summary>
public sealed class Record
{
    public required string Winner { get; init; }
    public required string Loser { get; init; }
    public required DateTime PlayedAt { get; init; }
    public required int MoveCount { get; init; }
}

