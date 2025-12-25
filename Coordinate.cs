using System;
using System.Globalization;

namespace TacTickle.Core;

/// <summary>
/// Представляет координату поля (строка, столбец) и конвертацию в нотацию «A1».
/// </summary>
public readonly struct Coordinate
{
    public int Row { get; }
    public int Column { get; }

    public Coordinate(int row, int column)
    {
        if (row is < 0 or >= GameBoard.Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(row), row, "Row is out of board bounds.");
        }

        if (column is < 0 or >= GameBoard.Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column is out of board bounds.");
        }

        Row = row;
        Column = column;
    }

    public static Coordinate FromNotation(string notation)
    {
        if (string.IsNullOrWhiteSpace(notation))
        {
            throw new ArgumentException("Notation cannot be null or whitespace.", nameof(notation));
        }

        notation = notation.Trim();
        var columnChar = char.ToUpperInvariant(notation[0]);
        if (columnChar < 'A' || columnChar >= 'A' + GameBoard.Columns)
        {
            throw new ArgumentOutOfRangeException(nameof(notation), notation, "Column letter is outside of the board range.");
        }

        if (!int.TryParse(notation.AsSpan(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rowNumber))
        {
            throw new FormatException($"Unable to parse row number from notation '{notation}'.");
        }

        if (rowNumber < 1 || rowNumber > GameBoard.Rows)
        {
            throw new ArgumentOutOfRangeException(nameof(notation), notation, "Row number is outside of the board range.");
        }

        var rowIndex = rowNumber - 1;
        var columnIndex = columnChar - 'A';
        return new Coordinate(rowIndex, columnIndex);
    }

    public string ToNotation()
    {
        var columnChar = (char)('A' + Column);
        var rowNumber = Row + 1;
        return string.Create(CultureInfo.InvariantCulture, $"{columnChar}{rowNumber}");
    }

    public override string ToString() => ToNotation();
}

