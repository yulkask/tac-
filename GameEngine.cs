using TacTickle.Core;

namespace TacTickle.Business;

/// <summary>
/// Игровой движок, управляющий логикой игры (Business слой).
/// </summary>
public sealed class GameEngine
{
    private readonly GameState _gameState;

    public GameEngine(GameState gameState)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }

    /// <summary>
    /// Выполняет ход игрока.
    /// </summary>
    public MoveResult MakeMove(Coordinate from, Direction direction)
    {
        var playerColor = GetCurrentPlayerColor();
        var validation = MoveValidator.ValidateMove(_gameState.Board, from, direction, playerColor);

        if (!validation.IsValid)
        {
            return new MoveResult(false, validation.ErrorMessage, null);
        }

        // Выполняем перемещение
        if (!MoveValidator.CanMoveInDirection(_gameState.Board, from, direction, out var target))
        {
            return new MoveResult(false, "Невозможно выполнить ход!", null);
        }

        // Перемещаем фишку
        _gameState.Board.SetCell(from.Row, from.Column, CellState.Empty);
        _gameState.Board.SetCell(target!.Value.Row, target.Value.Column, playerColor);

        // Проверяем победу
        var hasWon = WinChecker.CheckWin(_gameState.Board, playerColor);
        if (hasWon)
        {
            return new MoveResult(true, $"Ура! Победа игрока {_gameState.CurrentPlayer}!", GameResult.Win);
        }

        // Проверяем ничью
        var isDraw = WinChecker.CheckDraw(_gameState.MoveCount + 1);
        if (isDraw)
        {
            return new MoveResult(true, "Ничья! Игра завершена после 30 ходов.", GameResult.Draw);
        }

        // Переключаем ход
        _gameState.NextTurn();

        return new MoveResult(true, string.Empty, GameResult.Continue);
    }

    /// <summary>
    /// Получает цвет текущего игрока.
    /// </summary>
    public CellState GetCurrentPlayerColor()
    {
        // Игрок 0 (первый) играет белыми, игрок 1 (второй) - чёрными
        return _gameState.CurrentPlayerIndex == 0 ? CellState.White : CellState.Black;
    }

    public GameState GameState => _gameState;
}

/// <summary>
/// Результат выполнения хода.
/// </summary>
public sealed class MoveResult
{
    public bool Success { get; }
    public string Message { get; }
    public GameResult? Result { get; }

    public MoveResult(bool success, string message, GameResult? result)
    {
        Success = success;
        Message = message;
        Result = result;
    }
}

/// <summary>
/// Результат игры.
/// </summary>
public enum GameResult
{
    Continue,
    Win,
    Draw
}

