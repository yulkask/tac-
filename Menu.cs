using System;
using TacTickle.Data;

namespace TacTickle.Presentation;

/// <summary>
/// Главное меню приложения (Presentation слой).
/// </summary>
public sealed class Menu
{
    private readonly SaveLoadService _saveLoadService;
    private readonly RecordsService _recordsService;

    public Menu(SaveLoadService saveLoadService, RecordsService recordsService)
    {
        _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
        _recordsService = recordsService ?? throw new ArgumentNullException(nameof(recordsService));
    }

    /// <summary>
    /// Отображает главное меню.
    /// </summary>
    public void ShowMainMenu()
    {
        ConsoleIO.Clear();
        ConsoleIO.WriteLine("═══════════════════════════════════");
        ConsoleIO.WriteLine("        ИГРА ТАК-ТИКЛЬ");
        ConsoleIO.WriteLine("═══════════════════════════════════");
        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine("1. Начать новую игру");
        ConsoleIO.WriteLine("2. Загрузить игру");
        ConsoleIO.WriteLine("3. Таблица рекордов");
        ConsoleIO.WriteLine("4. Правила игры");
        ConsoleIO.WriteLine("0. Выход");
        ConsoleIO.WriteLine();
        ConsoleIO.Write("Введите пункт: ");
    }

    /// <summary>
    /// Обрабатывает выбор пользователя в главном меню.
    /// </summary>
    public async Task<bool> HandleMainMenuChoiceAsync(string? choice)
    {
        switch (choice)
        {
            case "1":
                await StartNewGameAsync();
                return true;

            case "2":
                await LoadGameMenuAsync();
                return true;

            case "3":
                await ShowRecordsAsync();
                return true;

            case "4":
                ShowRules();
                return true;

            case "0":
                return false;

            default:
                ConsoleIO.WriteLine("Ошибка ввода!");
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();
                return true;
        }
    }

    private async Task StartNewGameAsync()
    {
        ConsoleIO.Clear();
        ConsoleIO.WriteLine("=== Новая игра ===");
        ConsoleIO.WriteLine();

        ConsoleIO.Write("Введите имя игрока 1 (белые): ");
        var player1 = ConsoleIO.ReadLine();
        if (string.IsNullOrWhiteSpace(player1))
        {
            player1 = "Игрок 1";
        }

        ConsoleIO.Write("Введите имя игрока 2 (чёрные): ");
        var player2 = ConsoleIO.ReadLine();
        if (string.IsNullOrWhiteSpace(player2))
        {
            player2 = "Игрок 2";
        }

        var gameState = new TacTickle.Core.GameState(player1, player2);
        var gameController = new GameController(gameState, _saveLoadService, _recordsService);
        await gameController.PlayAsync();
    }

    private async Task LoadGameMenuAsync()
    {
        ConsoleIO.Clear();
        ConsoleIO.WriteLine("=== Загрузить игру ===");
        ConsoleIO.WriteLine();

        var saves = _saveLoadService.GetSavedGames();
        if (saves.Length == 0)
        {
            ConsoleIO.WriteLine("Игр пока не было.");
            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
            ConsoleIO.WaitForKey();
            return;
        }

        ConsoleIO.WriteLine("Список сохранений:");
        for (var i = 0; i < saves.Length; i++)
        {
            ConsoleIO.WriteLine($"{i + 1}. {saves[i]}");
        }
        ConsoleIO.WriteLine("0. Выйти");
        ConsoleIO.WriteLine();
        ConsoleIO.Write("Выберите игру: ");

        var choice = ConsoleIO.ReadLine();
        if (choice == "0" || string.IsNullOrWhiteSpace(choice))
        {
            return;
        }

        if (int.TryParse(choice, out var index) && index >= 1 && index <= saves.Length)
        {
            var saveName = saves[index - 1];
            var gameState = await _saveLoadService.LoadGameAsync(saveName);
            if (gameState != null)
            {
                ConsoleIO.WriteLine("Игра загружена.");
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();

                var gameController = new GameController(gameState, _saveLoadService, _recordsService);
                await gameController.PlayAsync();
            }
            else
            {
                ConsoleIO.WriteLine("Ошибка загрузки игры.");
                ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
                ConsoleIO.WaitForKey();
            }
        }
        else
        {
            ConsoleIO.WriteLine("Ошибка ввода!");
            ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
            ConsoleIO.WaitForKey();
        }
    }

    private async Task ShowRecordsAsync()
    {
        ConsoleIO.Clear();
        ConsoleIO.WriteLine("=== Таблица рекордов ===");
        ConsoleIO.WriteLine();

        var records = await _recordsService.GetTopRecordsAsync(10);
        if (records.Length == 0)
        {
            ConsoleIO.WriteLine("Рекордов пока нет.");
        }
        else
        {
            ConsoleIO.WriteLine($"{"Победитель",-20} {"Проигравший",-20} {"Дата",-20} {"Ходов"}");
            ConsoleIO.WriteLine(new string('-', 70));
            foreach (var record in records)
            {
                ConsoleIO.WriteLine($"{record.Winner,-20} {record.Loser,-20} {record.PlayedAt:yyyy-MM-dd HH:mm}   Победа за {record.MoveCount} ходов");
            }
        }

        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine("Введите 'выйти' для возврата в меню...");
        while (ConsoleIO.ReadLine()?.ToLower() != "выйти")
        {
            // Ждём ввода "выйти"
        }
    }

    private void ShowRules()
    {
        ConsoleIO.Clear();
        ConsoleIO.WriteLine("=== Правила игры ===");
        ConsoleIO.WriteLine();

        if (File.Exists("ПРАВИЛА_ИГРЫ.md"))
        {
            var rules = File.ReadAllText("ПРАВИЛА_ИГРЫ.md");
            ConsoleIO.WriteLine(rules);
        }
        else
        {
            ConsoleIO.WriteLine("Цель игры: выстроить линию из трёх своих фишек по горизонтали, вертикали или диагонали.");
            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Правила:");
            ConsoleIO.WriteLine("- Игроки ходят по очереди");
            ConsoleIO.WriteLine("- За один ход можно передвинуть свою фишку на одну свободную клетку");
            ConsoleIO.WriteLine("- Движение возможно только вверх, вниз, влево или вправо (не по диагонали)");
            ConsoleIO.WriteLine("- Выигрывает тот, кто первым выстроит три фишки подряд");
            ConsoleIO.WriteLine("- Если игроки сделали 30 ходов, игра считается ничьей");
        }

        ConsoleIO.WriteLine();
        ConsoleIO.WriteLine("Введите 'выйти' для возврата в меню...");
        while (ConsoleIO.ReadLine()?.ToLower() != "выйти")
        {
            // Ждём ввода "выйти"
        }
    }
}

