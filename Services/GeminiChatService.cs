using System.Text;
using System.Text.Json;

namespace UTHMLibrary.Services;

public class GeminiChatService : IAIChatService
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiChatService> _logger;

    // All available models - will try fallbacks if primary fails
    private static readonly string[] FallbackModels = new[]
    {
        "gemini-3.5-flash-lite",  // Your primary model
        "gemini-3.5-flash",
        "gemini-3.6-flash",
        "gemini-3.7-flash",
        "gemini-flash-latest",
        "gemini-pro-latest"
    };

    public GeminiChatService(IConfiguration configuration, ILogger<GeminiChatService> logger)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? "";
        _model = configuration["Gemini:Model"] ?? "gemini-3.5-flash-lite";
        _logger = logger;

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("⚠️ GEMINI API KEY IS MISSING!");
        }
        else
        {
            _logger.LogInformation($"✅ API Key loaded: {_apiKey.Substring(0, 10)}...");
            _logger.LogInformation($"✅ Using model: {_model}");
        }
    }

    public async Task<string> GetResponse(string message, string userName, string userRole)
    {
        _logger.LogInformation($"🤖 Request from {userName}: {message}");

        if (string.IsNullOrEmpty(_apiKey))
        {
            return "⚠️ API key not configured. Please add your API key to appsettings.json";
        }

        // Try the configured model first
        var modelsToTry = new List<string> { _model };
        modelsToTry.AddRange(FallbackModels.Where(m => m != _model));

        foreach (var model in modelsToTry)
        {
            try
            {
                _logger.LogInformation($"🔄 Trying model: {model}");

                var result = await TryModel(model, message, userName, userRole);
                if (result != null)
                {
                    _logger.LogInformation($"✅ Success with: {model}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"❌ {model} failed: {ex.Message}");
            }
        }

        _logger.LogError("❌ All models failed");
        return GetFallbackResponse(message, userName);
    }

    private async Task<string?> TryModel(string model, string message, string userName, string userRole)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

            var prompt = $@"You are a friendly AI assistant for UTHM Library. User: {userName} ({userRole}).

LIBRARY FACTS:
- Hours: Mon-Fri 8AM-10PM, Sat-Sun 9AM-6PM
- Study Room: RM10/session (2 hours max)
- Meeting Room: FREE (5 matric numbers, admin approval)
- WiFi: UTHM-Library / library@2024
- Email: library@uthm.edu.my
- Phone: +607-453-7000
- Location: Perpustakaan Tunku Tun Aminah

RULES:
- Quiet study allowed
- Group discussions in meeting rooms only
- No food/drinks (except water)
- No loud noise or smoking

User Question: {message}

Respond in a friendly, helpful way. Keep it concise (2-3 paragraphs). Use emojis when appropriate.";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 500,
                    topP = 0.9
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentObj) &&
                            contentObj.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            var part = parts[0];
                            if (part.TryGetProperty("text", out var textElement))
                            {
                                var aiResponse = textElement.GetString();
                                if (!string.IsNullOrEmpty(aiResponse))
                                {
                                    // Clean up formatting
                                    aiResponse = aiResponse.Replace("**", "<strong>").Replace("**", "</strong>");
                                    return aiResponse;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Parse error: {ex.Message}");
                }
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogInformation("⏳ Rate limited, waiting...");
                    await Task.Delay(3000);
                }
                else
                {
                    _logger.LogWarning($"Status: {response.StatusCode}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error: {ex.Message}");
        }

        return null;
    }

    private string GetFallbackResponse(string message, string userName)
    {
        var msg = message.ToLower().Trim();
        var rand = new Random();

        // Common questions
        if (msg.Contains("wifi") || msg.Contains("password"))
        {
            return "🌐 <strong>WiFi Information:</strong><br/>• Network: <strong>UTHM-Library</strong><br/>• Password: <strong>library@2024</strong>";
        }

        if (msg.Contains("hour") || msg.Contains("open") || msg.Contains("close"))
        {
            return "📚 <strong>UTHM Library Hours:</strong><br/>• Monday-Friday: <strong>8:00 AM - 10:00 PM</strong><br/>• Saturday-Sunday: <strong>9:00 AM - 6:00 PM</strong><br/>• Public Holidays: <strong>Closed</strong>";
        }

        if (msg.Contains("book") || msg.Contains("room") || msg.Contains("study") || msg.Contains("meeting"))
        {
            return "📖 <strong>Room Booking:</strong><br/><br/>🔹 <strong>Study Room:</strong> RM10/session (2 hours max)<br/>🔹 <strong>Meeting Room:</strong> FREE (5 matric numbers, admin approval)";
        }

        if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey") || msg.Contains("halo"))
        {
            var greetings = new[]
            {
                $"👋 Hello {userName}! Welcome to UTHM Library. How can I help you today?",
                $"Hi {userName}! 😊 What can I assist you with?",
                $"Selamat datang {userName}! 🇲🇾 How can I help you?"
            };
            return greetings[rand.Next(greetings.Length)];
        }

        // Default
        var fallbacks = new[]
        {
            $"🤖 I can help with:<br/>• 📚 Library hours<br/>• 🏠 Room booking<br/>• 🌐 WiFi<br/>• 📝 Registration<br/>• 📞 Contact",
            $"💡 Try asking about: hours, booking, wifi, rules, contact"
        };
        return fallbacks[rand.Next(fallbacks.Length)];
    }
}