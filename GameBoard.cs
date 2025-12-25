using System;

namespace TacTickle.Core;

/// <summary>
/// Игровое поле 4×4, хранящее текущее расположение фишек.
/// </summary>
public sealed class GameBoard
{
    public const int Rows = 4;
    public const int Columns = 4;

    private readonly CellState[,] _cells = new CellState[Rows, Columns];

    public CellState this[int row, int column]
    {
        get
        {
            ValidateBounds(row, column);
            return _cells[row, column];
        }
        private set
        {
            ValidateBounds(row, column);
            _cells[row, column] = value;
        }
    }

    /// <summary>
    /// Устанавливает состояние клетки (для загрузки из сохранения).
    /// </summary>
    public void SetCell(int row, int column, CellState state)
    {
        ValidateBounds(row, column);
        _cells[row, column] = state;
    }

    /// <summary>
    /// Получает копию состояния доски.
    /// </summary>
    public CellState[,] GetBoardState()
    {
        var state = new CellState[Rows, Columns];
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                state[row, column] = _cells[row, column];
            }
        }
        return state;
    }

    /// <summary>
    /// Восстанавливает состояние доски из массива.
    /// </summary>
    public void RestoreBoardState(CellState[,] state)
    {
        if (state.GetLength(0) != Rows || state.GetLength(1) != Columns)
        {
            throw new ArgumentException("Board state dimensions do not match.", nameof(state));
        }

        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                _cells[row, column] = state[row, column];
            }
        }
    }

    /// <summary>
    /// Сбрасывает поле и расставляет фишки согласно документации.
    /// </summary>
    public void Initialize()
    {
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                _cells[row, column] = CellState.Empty;
            }
        }

        // Новая расстановка: фишки чередуются по краям, как на картинке пользователя.
        // Верхняя (4-я) строка: A4-чёрная, B4-белая, C4-чёрная, D4-белая
        PlacePiece("A4", CellState.Black);
        PlacePiece("B4", CellState.White);
        PlacePiece("C4", CellState.Black);
        PlacePiece("D4", CellState.White);
        // Нижняя (1-я) строка: A1-белая, B1-чёрная, C1-белая, D1-чёрная
        PlacePiece("A1", CellState.White);
        PlacePiece("B1", CellState.Black);
        PlacePiece("C1", CellState.White);
        PlacePiece("D1", CellState.Black);
    }

    private void PlacePiece(string notation, CellState state)
    {
        var coord = Coordinate.FromNotation(notation);
        this[coord.Row, coord.Column] = state;
    }

    private static void ValidateBounds(int row, int column)
    {
        if (row is < 0 or >= Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row), row, "Row is out of board bounds.");
        }

        if (column is < 0 or >= Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column is out of board bounds.");
        }
    }
}

