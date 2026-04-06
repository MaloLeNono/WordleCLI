using Newtonsoft.Json;

namespace WordleCLI.ApiHandler;

public class ApiHandler : IApiHandler
{
    private const string WordUrl = "https://wordlehints.co.uk/wp-json/wordlehint/v1/answers/latest";
    private const string DictUrl = "https://api.dictionaryapi.dev/api/v2/entries/en";
    private static readonly HttpClient Client = new();
    
    public async Task<string> GetWord()
    {
        string json = await Client.GetStringAsync(WordUrl);
        var responseModel = new { answer = "" };
        var response = JsonConvert.DeserializeAnonymousType(json, responseModel);
        return response is null
            ? throw new HttpRequestException("Error fetching daily word.")
            : response.answer;
    }

    public async Task<bool> WordExists(string word)
    {
        string fullUrl = $"{DictUrl}/{word}";
        var response = await Client.GetAsync(fullUrl);
        return response.IsSuccessStatusCode;
    }
}