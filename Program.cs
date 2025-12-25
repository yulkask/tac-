using System;
using TacTickle.Core;
using TacTickle.Business;
using TacTickle.Data;
using TacTickle.Presentation;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var savesDir = "./saves";
        var saver = new SaveLoadManager(savesDir);

        var highs = new HighScoreTable();
        highs.Load();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Так-Тикль ===");
            Console.WriteLine("1) Новая игра");
            Console.WriteLine("2) Загрузить игру");
            Console.WriteLine("3) Показать рекорды");
                Console.WriteLine("5) Диагностика поля");
            Console.WriteLine("4) Выйти");
            Console.Write("Выберите пункт: ");

            var key = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(key)) continue;

            if (key == "1")
            {
                await StartNewGameAsync(saver, highs);
            }
            else if (key == "2")
            {
                ShowLoadMenu(saver);
            }
            else if (key == "3")
            {
                ShowHighScores(highs);
            }
                else if (key == "5")
                {
                    ShowDiagnostics();
                }
            else if (key == "4")
            {
                break;
            }
        }
    }

    private static async Task StartNewGameAsync(SaveLoadManager saver, HighScoreTable highs)
    {
        Console.Clear();
        Console.WriteLine("=== Новая игра ===");

        string ReadName(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Имя не может быть пустым. Попробуйте ещё раз.");
                    continue;
                }
                return input.Trim();
            }
        }

        var name1 = ReadName("Имя игрока 1: ");
        var name2 = ReadName("Имя игрока 2: ");

        try
        {
            var gameState = new GameState(name1, name2, 0);

            // Use Presentation layer controller to run the game loop
            var saveLoadService = new SaveLoadService();
            var recordsService = new RecordsService();
            var controller = new GameController(gameState, saveLoadService, recordsService);

            await controller.PlayAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось создать новую игру: {ex.Message}");
            Pause();
            return;
        }

        Pause();
    }

    private static void ShowLoadMenu(SaveLoadManager saver)
    {
        var files = saver.GetSaveFiles();
        Console.Clear();
        Console.WriteLine("=== Загрузить игру ===");

        if (files.Length == 0)
        {
            Console.WriteLine("Игp пока не было. Сохранений не найдено.");
            Pause();
            return;
        }

        for (int i = 0; i < files.Length; i++)
        {
            Console.WriteLine($"{i + 1}) {files[i]}");
        }

        Console.WriteLine("0) Отмена");
        Console.Write("Выберите файл (номер): ");
        var input = Console.ReadLine()?.Trim();
        if (!int.TryParse(input, out var idx))
        {
            Console.WriteLine("Неверный ввод.");
            Pause();
            return;
        }

        if (idx == 0) return;

        if (idx < 1 || idx > files.Length)
        {
            Console.WriteLine("Номер вне диапазона.");
            Pause();
            return;
        }

        var filename = files[idx - 1];
        try
        {
            var gameState = saver.LoadGame(filename);
            Console.WriteLine($"Сохранение '{filename}' успешно загружено.");
            Console.WriteLine($"Игроки: {gameState.Players[0]} vs {gameState.Players[1]}");
            Console.WriteLine($"Текущий игрок: {gameState.CurrentPlayer}");
            Console.WriteLine($"Счет ходов: {gameState.MoveCount}");
            Console.WriteLine("Поле:");
            var board = gameState.Board.GetBoardState();
            for (int r = 0; r < GameBoard.Rows; r++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    Console.Write(board[r, c] == CellState.Black ? 'B' : board[r, c] == CellState.White ? 'W' : '.');
                    if (c < GameBoard.Columns - 1) Console.Write(' ');
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке: {ex.Message}");
        }

        Pause();
    }

    private static void ShowHighScores(HighScoreTable highs)
    {
        Console.Clear();
        Console.WriteLine("=== Рекорды ===");
        highs.Load();
        var entries = highs.GetTop(10);
        if (entries.Length == 0)
        {
            Console.WriteLine("Таблица рекордов пока пуста.");
            Pause();
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            Console.WriteLine($"{i + 1}) {e.Name} — {e.Score} ({e.When:u})");
        }

        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для возврата в меню...");
        Console.ReadLine();
    }

        private static void ShowDiagnostics()
        {
            Console.Clear();
            Console.WriteLine("=== Диагностика поля ===");

            var gs = new GameState("P1", "P2", 0);
            var board = gs.Board.GetBoardState();
            Console.WriteLine($"CurrentPlayerIndex: {gs.CurrentPlayerIndex} (CurrentPlayer: {gs.CurrentPlayer})");
            Console.WriteLine("Board layout (rows 1..4):");
            for (int r = 0; r < GameBoard.Rows; r++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    var cell = board[r, c];
                    Console.Write(cell == CellState.Black ? 'B' : cell == CellState.White ? 'W' : '.');
                    if (c < GameBoard.Columns - 1) Console.Write(' ');
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Пробуем проверить выбор каждой клетки для текущего игрока (ожидается белые):");
            var engine = new GameEngine(gs);
            var playerColor = engine.GetCurrentPlayerColor();
            for (int r = 0; r < GameBoard.Rows; r++)
            {
                for (int c = 0; c < GameBoard.Columns; c++)
                {
                    var coord = new Coordinate(r, c);
                    var canSelect = MoveValidator.CanSelectPiece(gs.Board, coord, playerColor);
                    Console.Write(canSelect ? 'Y' : 'N');
                    if (c < GameBoard.Columns - 1) Console.Write(' ');
                }
                Console.WriteLine();
            }

            Pause();
        }
}

