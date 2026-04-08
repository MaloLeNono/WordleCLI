using Newtonsoft.Json;

namespace WordleCLI.ApiHandler;

public class ApiHandler : IApiHandler
{
    private const string WordUrl = "https://www.nytimes.com/svc/wordle/v2";
    private const string DictUrl = "https://api.dictionaryapi.dev/api/v2/entries/en";
    private static readonly HttpClient Client = new();
    
    public async Task<string> GetWord()
    {
        DateTime now = DateTime.Now;
        string fullUrl = $"{WordUrl}/{now:yyyy-MM-dd}.json";
        string json = await Client.GetStringAsync(fullUrl);
        var responseModel = new { solution = "" };
        var response = JsonConvert.DeserializeAnonymousType(json, responseModel);
        return response is null
            ? throw new HttpRequestException("Error fetching daily word.")
            : response.solution.ToUpper();
    }

    public async Task<bool> WordExists(string word)
    {
        string fullUrl = $"{DictUrl}/{word}";
        var response = await Client.GetAsync(fullUrl);
        return response.IsSuccessStatusCode;
    }
}