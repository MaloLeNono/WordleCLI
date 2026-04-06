using Spectre.Console;
using WordleCLI.Enum;

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
        AnsiConsole.Write(
            new FigletText("WordleCLI")
                .LeftJustified()
                .Color(Color.Yellow)
        );
        
        AnsiConsole.MarkupLine("[yellow]v1.0.1 - by Malo[/]");
        AnsiConsole.Write(new Rule());
        
        if (_word is null) return;
        
        string guess = string.Empty;

        for (int attempt = 1; attempt <= Attempts; attempt++)
        {
            AnsiConsole.MarkupLine($"\n[bold white on gray11]Attempt {attempt} ({Attempts - attempt} left):[/]\n");

            guess = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]>[/]")
                    .Validate(x =>
                    {
                        if (x.Length != 5) return ValidationResult.Error("[white on red]You need to type 5 letters.[/]");
                        if (!x.All(char.IsLetter)) return ValidationResult.Error("[white on red]You can only type letters.[/]");
                        return !_apiHandler.WordExists(x).GetAwaiter().GetResult() 
                            ? ValidationResult.Error("[white on red]This word does not exist.[/]") 
                            : ValidationResult.Success();
                    })
            ).ToUpper();
            
            AnsiConsole.WriteLine();

            Dictionary<char, int> remainingOccurrences = GetLetterOccurrences(_word);
            LetterState[] letterStates = Enumerable.Repeat(LetterState.Incorrect, 5).ToArray();
            for (int i = 0; i < guess.Length; i++)
            {
                char c = guess[i];
                if (c != _word[i]) continue;
                letterStates[i] = LetterState.Correct;
                remainingOccurrences[c]--;
            }

            for (int i = 0; i < guess.Length; i++)
            {
                if (letterStates[i] == LetterState.Correct) continue;
                char c = guess[i];
                if (!remainingOccurrences.TryGetValue(c, out int value) || value <= 0) continue;
                letterStates[i] = LetterState.WrongPosition;
                remainingOccurrences[c] = --value;
            }

            for (int i = 0; i < guess.Length; i++)
            {
                string colour = letterStates[i] switch
                {
                    LetterState.Correct => Color.Green.ToMarkup(),
                    LetterState.WrongPosition => Color.Gold3_1.ToMarkup(),
                    LetterState.Incorrect => Color.Gray.ToMarkup(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                AnsiConsole.Markup($"[white on {colour}] {guess[i]} [/]");
                Thread.Sleep(250);
            }
            
            
            AnsiConsole.WriteLine("\n");
            AnsiConsole.Write(new Rule());
            
            if (guess == _word)
                break;
        }

        AnsiConsole.MarkupLine(guess != _word
            ? $"Nice try. The word was [yellow]{_word}[/]."
            : $"Congrats! The word was [yellow]{_word}[/]."
        );

        AnsiConsole.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static Dictionary<char, int> GetLetterOccurrences(string word)
    {
        Dictionary<char, int> letterOccurences = new();

        foreach (char c in word)
        {
            if (letterOccurences.TryGetValue(c, out int value))
                letterOccurences[c] = ++value;
            else
                letterOccurences.TryAdd(c, 1);
        }

        return letterOccurences;
    }

    private async Task InitializeWordAsync() => _word = await _apiHandler.GetWord();
}