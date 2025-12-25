using TacTickle.Core;
using TacTickle.Business;
using TacTickle.Data;

namespace TacTickle.Presentation;

/// <summary>
/// Контроллер игрового процесса (Presentation слой).
/// </summary>
public sealed class GameController
{
    private readonly GameState _gameState;
    private readonly SaveLoadService _saveLoadService;
    private readonly RecordsService _recordsService;
    private readonly GameEngine _engine;

    public GameController(GameState gameState, SaveLoadService saveLoadService, RecordsService recordsService)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
        _recordsService = recordsService ?? throw new ArgumentNullException(nameof(recordsService));
        _engine = new GameEngine(_gameState);
    }

    /// <summary>
    /// Запускает игровой цикл.
    /// </summary>
    public async Task PlayAsync()
    {
        while (true)
        {
            ConsoleIO.Clear();
            GameUI.DisplayBoard(_gameState.Board);
            GameUI.DisplayTurnInfo(_gameState, _engine.GetCurrentPlayerColor());
            GameUI.DisplayMoveInstructions();

            // Запрос координаты фишки
            ConsoleIO.Write("Введите координату фишки (или 'выйти'): ");
            var input = ConsoleIO.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "выйти")
            {
                await HandleExitAsync();
                return;
            }

            // Парсинг координаты
            Coordinate? fromCoord = null;
            try
            {
                fromCoord = Coordinate.FromNotation(input);
            }
            catch
            {
                GameUI.DisplayError("Неверный формат координаты! Используйте формат A1-D4.");
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();
                continue;
            }

            // Проверка выбора фишки
            var playerColor = _engine.GetCurrentPlayerColor();
            if (!MoveValidator.CanSelectPiece(_gameState.Board, fromCoord.Value, playerColor))
            {
                 // Диагностика: покажем, что на выбранной клетке и какого цвета ожидается фишка
                 var actual = _gameState.Board[fromCoord.Value.Row, fromCoord.Value.Column];
                 GameUI.DisplayError($"Вы выбрали не свою фишку! На клетке {fromCoord.Value} находится: {actual} (ожидается: {playerColor})");
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();
                continue;
            }

            // Запрос направления движения
            ConsoleIO.WriteLine("Выберите направление движения:");
            ConsoleIO.WriteLine("w - вверх, s - вниз, a - влево, d - вправо");
            ConsoleIO.Write("Направление (или 'отмена'): ");
            var directionInput = ConsoleIO.ReadLine()?.Trim().ToLower();

            if (directionInput == "отмена")
            {
                continue;
            }

            var direction = ParseDirection(directionInput);
            if (direction == null)
            {
                GameUI.DisplayError("Неверное направление! Используйте w, s, a или d.");
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();
                continue;
            }

            // Выполнение хода
            var result = _engine.MakeMove(fromCoord.Value, direction.Value);

            if (!result.Success)
            {
                GameUI.DisplayError(result.Message);
                ConsoleIO.WriteLine("Нажмите любую клавишу для продолжения...");
                ConsoleIO.WaitForKey();
                continue;
            }

            // Показываем новое состояние поля сразу после хода
            ConsoleIO.Clear();
            GameUI.DisplayBoard(_gameState.Board);
            GameUI.DisplayTurnInfo(_gameState, _engine.GetCurrentPlayerColor());
            ConsoleIO.WriteLine();

            // Проверка результата игры
            if (result.Result == GameResult.Win)
            {
                GameUI.DisplayWinMessage(_gameState.CurrentPlayer);

                // Запись рекорда
                var winner = _gameState.CurrentPlayer;
                var loser = _gameState.Players[(_gameState.CurrentPlayerIndex + 1) % 2];
                // Для победы включаем текущий совершённый ход в счётчик
                await _recordsService.AddRecordAsync(winner, loser, _gameState.MoveCount + 1);

                ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
                ConsoleIO.WaitForKey();
                return;
            }

            if (result.Result == GameResult.Draw)
            {
                GameUI.DisplayDrawMessage();

                ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
                ConsoleIO.WaitForKey();
                return;
            }
        }
    }

    private async Task HandleExitAsync()
    {
        ConsoleIO.WriteLine();
        ConsoleIO.Write("Сохранить игру? (да/нет): ");
        var answer = ConsoleIO.ReadLine()?.Trim().ToLower();

        if (answer == "да" || answer == "д" || answer == "y" || answer == "yes")
        {
            ConsoleIO.Write("Введите название сохранения: ");
            var saveName = ConsoleIO.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(saveName))
            {
                saveName = $"{_gameState.Players[0]}_{_gameState.Players[1]}_{DateTime.Now:yyyyMMdd_HHmmss}";
            }

            await _saveLoadService.SaveGameAsync(_gameState, saveName);
            ConsoleIO.WriteLine("Игра сохранена.");
            ConsoleIO.WriteLine("Нажмите любую клавишу для возврата в меню...");
            ConsoleIO.WaitForKey();
        }
    }

    private static Direction? ParseDirection(string? input)
    {
        return input switch
        {
            "w" => Direction.Up,
            "s" => Direction.Down,
            "a" => Direction.Left,
            "d" => Direction.Right,
            _ => null
        };
    }
}

