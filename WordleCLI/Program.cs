namespace WordleCLI;

internal static class Program
{
    private static async Task Main()
    {
        ApiHandler handler = new();
        Game game = await Game.CreateGameAsync(handler);
        game.Start();
    }
}