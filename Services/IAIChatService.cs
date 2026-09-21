namespace UTHMLibrary.Services;

public interface IAIChatService
{
    Task<string> GetResponse(string message, string userName, string userRole);
}