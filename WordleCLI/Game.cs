using Spectre.Console;

namespace WordleCLI;

public class Game
{
    private const int Attempts = 6;
    private string? _word;
    private readonly IApiHandler _apiHandler;

    private Game(IApiHandler handler) => _apiHandler = handler;

    public static async Task<Game> CreateGameAsync(IApiHandler apiHandler)
    {
        Game game = new(apiHandler);
        await game.InitializeWordAsync();
        return game;
    }
    
    public void Start()
    {
        if (_word is null) return;
        
        string guess = string.Empty;

        for (int attempt = 1; attempt <= Attempts; attempt++)
        {
            AnsiConsole.WriteLine($"Attempt {attempt} ({Attempts - attempt} left):");

            guess = AnsiConsole.Prompt(
                new TextPrompt<string>("Guess:")
                    .Validate(x =>
                    {
                        if (x.Length != 5) return ValidationResult.Error("[red]You need to type 5 letters.[/]");
                        if (!x.All(char.IsLetter)) return ValidationResult.Error("[red]You can only type letters.[/]");
                        return !_apiHandler.WordExists(x).GetAwaiter().GetResult() 
                            ? ValidationResult.Error("[red]This word does not exist.[/]") 
                            : ValidationResult.Success();
                    })
            ).ToUpper();

            for (int i = 0; i < guess.Length; i++)
            {
                string letterColor;
                if (guess[i] == _word[i])
                    letterColor = Color.Green.ToMarkup();
                else if (_word.Contains(guess[i]))
                    letterColor = Color.Yellow.ToMarkup();
                else
                    letterColor = Color.Grey.ToMarkup();

                AnsiConsole.Markup($"[white on {letterColor}] {guess[i]} [/]");
                Thread.Sleep(250);
            }
            
            AnsiConsole.WriteLine("\n");
            
            if (guess == _word)
                break;
        }

        AnsiConsole.WriteLine(guess != _word
            ? $"Nice try. The word was {_word}."
            : $"Congrats! The word was {_word}."
        );
    }

    private async Task InitializeWordAsync() => _word = await _apiHandler.GetWord();
}