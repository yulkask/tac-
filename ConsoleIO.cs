namespace TacTickle.Presentation;

/// <summary>
/// Сервис для ввода-вывода в консоль (Presentation слой).
/// </summary>
public static class ConsoleIO
{
    /// <summary>
    /// Выводит сообщение.
    /// </summary>
    public static void WriteLine(string? message = null)
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Выводит сообщение без перевода строки.
    /// </summary>
    public static void Write(string? message = null)
    {
        Console.Write(message);
    }

    /// <summary>
    /// Выводит сообщение об ошибке в поток ошибок (stderr).
    /// </summary>
    public static void WriteError(string? message = null)
    {
        Console.Error.WriteLine(message);
    }

    /// <summary>
    /// Читает строку из консоли.
    /// </summary>
    public static string? ReadLine()
    {
        return Console.ReadLine();
    }

    /// <summary>
    /// Очищает консоль.
    /// </summary>
    public static void Clear()
    {
        Console.Clear();
    }

    /// <summary>
    /// Ожидает нажатия клавиши.
    /// </summary>
    public static void WaitForKey()
    {
        try
        {
            Console.ReadKey(true);
        }
        catch (InvalidOperationException)
        {
            // Если консольный ввод перенаправлен, используем Console.Read
            Console.Read();
        }
    }
}

