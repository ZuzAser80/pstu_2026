namespace Lab10.Tests;

/// <summary>
/// Captures whatever the code under test writes to System.Console.
/// The production classes call Console.WriteLine directly, so there is no injection point;
/// redirecting the stream is the only way to observe them.
/// </summary>
internal static class ConsoleCapture
{
    public static string Capture(Action action)
    {
        var originalOut = Console.Out;
        var buffer = new StringWriter();
        try
        {
            Console.SetOut(buffer);
            action();
        }
        finally
        {
            // Restoring matters: without it every later test would keep writing into buffer.
            Console.SetOut(originalOut);
        }
        return buffer.ToString();
    }
}