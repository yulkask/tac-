using System;

namespace TacTickle.Core;

/// <summary>
/// Хранит состояние текущей партии: игроки, игровое поле, текущий игрок и счётчик ходов.
/// </summary>
public sealed class GameState
{
    public string[] Players { get; }

    public GameBoard Board { get; }

    /// <summary>
    /// Индекс текущего игрока в массиве Players (0 или 1).
    /// </summary>
    public int CurrentPlayerIndex { get; private set; }

    /// <summary>
    /// Имя текущего игрока.
    /// </summary>
    public string CurrentPlayer => Players[CurrentPlayerIndex];

    /// <summary>
    /// Количество совершённых ходов в партии.
    /// </summary>
    public int MoveCount { get; private set; }

    /// <summary>
    /// Устанавливает количество ходов (для загрузки из сохранения).
    /// </summary>
    public void SetMoveCount(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Move count cannot be negative.");
        }
        MoveCount = count;
    }

    public GameState(string player1, string player2, int startingPlayerIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(player1)) throw new ArgumentException("Player1 name cannot be empty.", nameof(player1));
        if (string.IsNullOrWhiteSpace(player2)) throw new ArgumentException("Player2 name cannot be empty.", nameof(player2));

        Players = new[] { player1.Trim(), player2.Trim() };
        Board = new GameBoard();
        Board.Initialize();
        SetCurrentPlayer(startingPlayerIndex);
        MoveCount = 0;
    }

    /// <summary>
    /// Переключает ход на следующего игрока и увеличивает счётчик ходов.
    /// </summary>
    public void NextTurn()
    {
        MoveCount++;
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Length;
    }

    /// <summary>
    /// Устанавливает текущего игрока по индексу (0 или 1).
    /// </summary>
    public void SetCurrentPlayer(int index)
    {
        if (index < 0 || index >= Players.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Player index is out of range.");
        }

        CurrentPlayerIndex = index;
    }

    /// <summary>
    /// Сбрасывает поле и счётчик ходов, оставляя имена игроков.
    /// </summary>
    public void Reset()
    {
        Board.Initialize();
        MoveCount = 0;
        CurrentPlayerIndex = 0;
    }
}
