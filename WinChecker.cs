using TacTickle.Core;

namespace TacTickle.Business;

/// <summary>
/// Проверяет условия победы и ничьей (Business слой).
/// </summary>
public static class WinChecker
{
    private const int MaxMoves = 30;

    /// <summary>
    /// Проверяет, выиграл ли текущий игрок.
    /// </summary>
    public static bool CheckWin(GameBoard board, CellState playerColor)
    {
        // Проверка по горизонтали
        for (var row = 0; row < GameBoard.Rows; row++)
        {
            if (CheckLine(board, row, 0, 0, 1, playerColor))
                return true;
        }

        // Проверка по вертикали
        for (var column = 0; column < GameBoard.Columns; column++)
        {
            if (CheckLine(board, 0, column, 1, 0, playerColor))
                return true;
        }

        // Проверка по диагонали (слева направо)
        if (CheckLine(board, 0, 0, 1, 1, playerColor))
            return true;

        // Проверка по диагонали (справа налево)
        if (CheckLine(board, 0, GameBoard.Columns - 1, 1, -1, playerColor))
            return true;

        return false;
    }

    /// <summary>
    /// Проверяет, достигнута ли ничья (30 ходов).
    /// </summary>
    public static bool CheckDraw(int moveCount)
    {
        return moveCount >= MaxMoves;
    }

    private static bool CheckLine(GameBoard board, int startRow, int startColumn, int deltaRow, int deltaColumn, CellState color)
    {
        var count = 0;
        var row = startRow;
        var column = startColumn;

        while (row >= 0 && row < GameBoard.Rows && column >= 0 && column < GameBoard.Columns)
        {
            if (board[row, column] == color)
            {
                count++;
                if (count >= 3)
                {
                    return true;
                }
            }
            else
            {
                count = 0;
            }

            row += deltaRow;
            column += deltaColumn;
        }

        return false;
    }
}

