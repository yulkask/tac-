using TacTickle.Core;

namespace TacTickle.Business;

/// <summary>
/// Валидатор ходов (Business слой).
/// </summary>
public static class MoveValidator
{
    /// <summary>
    /// Проверяет, может ли игрок выбрать указанную фишку.
    /// </summary>
    public static bool CanSelectPiece(GameBoard board, Coordinate coord, CellState playerColor)
    {
        if (board[coord.Row, coord.Column] != playerColor)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Проверяет, можно ли переместить фишку в указанном направлении.
    /// </summary>
    public static bool CanMoveInDirection(GameBoard board, Coordinate from, Direction direction, out Coordinate? target)
    {
        target = null;

        var (deltaRow, deltaColumn) = GetDirectionDelta(direction);
        var newRow = from.Row + deltaRow;
        var newColumn = from.Column + deltaColumn;

        // Проверка границ
        if (newRow < 0 || newRow >= GameBoard.Rows || newColumn < 0 || newColumn >= GameBoard.Columns)
        {
            return false;
        }

        var targetCoord = new Coordinate(newRow, newColumn);

        // Проверка, что целевая клетка свободна
        if (board[targetCoord.Row, targetCoord.Column] != CellState.Empty)
        {
            return false;
        }

        target = targetCoord;
        return true;
    }

    /// <summary>
    /// Проверяет валидность полного хода (выбор фишки + направление).
    /// </summary>
    public static MoveValidationResult ValidateMove(GameBoard board, Coordinate from, Direction direction, CellState playerColor)
    {
        // Проверка 1: Выбрана ли фишка игрока
        if (!CanSelectPiece(board, from, playerColor))
        {
            return new MoveValidationResult(false, "Вы выбрали не свою фишку!");
        }

        // Проверка 2: Можно ли переместить в этом направлении
        if (!CanMoveInDirection(board, from, direction, out var target))
        {
            if (target == null)
            {
                return new MoveValidationResult(false, "Невозможно переместить фишку в этом направлении! Клетка занята или выходит за границы поля.");
            }
            return new MoveValidationResult(false, "Невозможно переместить фишку в этом направлении!");
        }

        return new MoveValidationResult(true, string.Empty);
    }

    private static (int deltaRow, int deltaColumn) GetDirectionDelta(Direction direction)
    {
        return direction switch
        {
            Direction.Up => (-1, 0),
            Direction.Down => (1, 0),
            Direction.Left => (0, -1),
            Direction.Right => (0, 1),
            _ => throw new ArgumentException($"Unknown direction: {direction}", nameof(direction))
        };
    }
}

/// <summary>
/// Направление движения фишки.
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Результат валидации хода.
/// </summary>
public sealed class MoveValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    public MoveValidationResult(bool isValid, string errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }
}

