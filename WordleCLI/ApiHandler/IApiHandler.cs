namespace WordleCLI.ApiHandler;

public interface IApiHandler
{
    public Task<string> GetWord();
    public Task<bool> WordExists(string word);
}