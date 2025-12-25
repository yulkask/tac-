using TacTickle.Core;
using TacTickle.Business;

namespace TacTickle.Presentation;

/// <summary>
/// Отображение игрового интерфейса (Presentation слой).
/// </summary>
public static class GameUI
{
    private const char BlackPiece = '●';
    private const char WhitePiece = '○';
    private const char EmptyCell = ' ';

    /// <summary>
    /// Отображает игровое поле.
    /// </summary>
    public static void DisplayBoard(GameBoard board)
    {
        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine("   A   B   C   D");
        ConsoleIO.WriteLine("  ┌───┬───┬───┬───┐");

        for (var row = 0; row < GameBoard.Rows; row++)
        {
            ConsoleIO.Write($"{(row + 1)} │");
            for (var column = 0; column < GameBoard.Columns; column++)
            {
                var cell = board[row, column];
                var symbol = GetCellSymbol(cell);
                ConsoleIO.Write($" {symbol} │");
            }
            ConsoleIO.WriteLine();

            if (row < GameBoard.Rows - 1)
            {
                ConsoleIO.WriteLine("  ├───┼───┼───┼───┤");
            }
        }

        ConsoleIO.WriteLine("  └───┴───┴───┴───┘");
        ConsoleIO.WriteLine();
    }

    /// <summary>
    /// Отображает информацию о текущем ходе.
    /// </summary>
    public static void DisplayTurnInfo(GameState gameState, CellState playerColor)
    {
        var colorName = playerColor == CellState.White ? "белые" : "чёрные";
        ConsoleIO.WriteLine($"Ход игрока: {gameState.CurrentPlayer} ({colorName})");
        ConsoleIO.WriteLine($"Количество ходов: {gameState.MoveCount}");
        ConsoleIO.WriteLine();
    }

    /// <summary>
    /// Отображает инструкции по управлению.
    /// </summary>
    public static void DisplayMoveInstructions()
    {
        ConsoleIO.WriteLine("Инструкция:");
        ConsoleIO.WriteLine("1. Введите координату фишки (например: A1)");
        ConsoleIO.WriteLine("2. Используйте клавиши для движения:");
        ConsoleIO.WriteLine("   w - вверх, s - вниз, a - влево, d - вправо");
        ConsoleIO.WriteLine("3. Нажмите Enter для подтверждения хода");
        ConsoleIO.WriteLine("4. Введите 'выйти' для выхода из игры");
        ConsoleIO.WriteLine();
    }

    /// <summary>
    /// Отображает сообщение об ошибке.
    /// </summary>
    public static void DisplayError(string message)
    {
        ConsoleIO.WriteLine($"Ошибка: {message}");
        ConsoleIO.WriteLine();
    }

    /// <summary>
    /// Отображает сообщение о победе.
    /// </summary>
    public static void DisplayWinMessage(string winner)
    {
        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine($"═══════════════════════════════════");
        ConsoleIO.WriteLine($"  Ура! Победа игрока {winner}!");
        ConsoleIO.WriteLine($"═══════════════════════════════════");
        ConsoleIO.WriteLine();
    }

    /// <summary>
    /// Отображает сообщение о ничьей.
    /// </summary>
    public static void DisplayDrawMessage()
    {
        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine($"═══════════════════════════════════");
        ConsoleIO.WriteLine($"  Ничья! Игра завершена.");
        ConsoleIO.WriteLine($"═══════════════════════════════════");
        ConsoleIO.WriteLine();
    }

    private static char GetCellSymbol(CellState cell)
    {
        return cell switch
        {
            CellState.Black => BlackPiece,
            CellState.White => WhitePiece,
            CellState.Empty => EmptyCell,
            _ => EmptyCell
        };
    }
}

