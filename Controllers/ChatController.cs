using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTHMLibrary.Services;

namespace UTHMLibrary.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IAIChatService _aiService;
    private readonly IConfiguration _configuration;

    public ChatController(IAIChatService aiService, IConfiguration configuration)
    {
        _aiService = aiService;
        _configuration = configuration;
    }

    private static Dictionary<string, List<ChatMessage>> _chatHistory = new();

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<JsonResult> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return Json(new { success = false, response = "Please enter a message." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
            var userName = User.Identity?.Name ?? "Student";
            var userRole = User.IsInRole("Admin") ? "Admin" : "Student";

            // Save user message
            if (!_chatHistory.ContainsKey(userId))
                _chatHistory[userId] = new List<ChatMessage>();

            _chatHistory[userId].Add(new ChatMessage
            {
                UserId = userId,
                Message = request.Message,
                Sender = "user",
                Timestamp = DateTime.UtcNow
            });

            // Get AI response from Gemini
            string response;
            try
            {
                response = await _aiService.GetResponse(request.Message, userName, userRole);
            }
            catch (Exception)
            {
                // Fallback to local responses if AI fails
                response = GetLocalResponse(request.Message, userName);
            }

            // Save bot response
            _chatHistory[userId].Add(new ChatMessage
            {
                UserId = userId,
                Message = response,
                Response = response,
                Sender = "bot",
                Timestamp = DateTime.UtcNow
            });

            return Json(new
            {
                success = true,
                response = response,
                timestamp = DateTime.Now.ToString("hh:mm tt")
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                response = "I'm having trouble processing your request. Please try again later."
            });
        }
    }

    private string GetLocalResponse(string message, string userName)
    {
        var msg = message.ToLower().Trim();
        var rand = new Random();

        // WiFi
        if (msg.Contains("wifi") || msg.Contains("internet") || msg.Contains("password") || msg.Contains("network"))
        {
            return "🌐 <strong>WiFi Information:</strong><br/>• Network: <strong>UTHM-Library</strong><br/>• Password: <strong>library@2024</strong><br/>• Available throughout the library<br/>• Speed: High-speed connection<br/><br/>⚠️ <em>For technical issues, contact IT Support at 07-453 7000</em>";
        }

        // Library Hours
        if (msg.Contains("hour") || msg.Contains("open") || msg.Contains("close") || msg.Contains("time") || msg.Contains("schedule"))
        {
            return "📚 <strong>UTHM Library Hours:</strong><br/>• Monday-Friday: <strong>8:00 AM - 10:00 PM</strong><br/>• Saturday-Sunday: <strong>9:00 AM - 6:00 PM</strong><br/>• Public Holidays: <strong>Closed</strong><br/><br/>📍 <em>Perpustakaan Tunku Tun Aminah</em>";
        }

        // Room Booking
        if (msg.Contains("book") || msg.Contains("room") || msg.Contains("study") || msg.Contains("reserve") || msg.Contains("meeting"))
        {
            return "📖 <strong>Room Booking Guide:</strong><br/><br/>🔹 <strong>Study Room (RM10/session):</strong><br/>• Select available room (green indicator)<br/>• Choose date & time (2 hours max)<br/>• Complete payment online<br/>• Instant confirmation<br/><br/>🔹 <strong>Meeting Room (FREE):</strong><br/>• Select available room<br/>• Enter 5 unique matric numbers<br/>• Admin approval required<br/><br/>💡 <em>Book early to secure your slot!</em>";
        }

        // Resources
        if (msg.Contains("resource") || msg.Contains("paper") || msg.Contains("ebook") || msg.Contains("project") || msg.Contains("book"))
        {
            return "📚 <strong>Available Resources:</strong><br/><br/>📄 Past Year Papers<br/>📖 E-Books<br/>📊 Project Reports<br/>📑 General Resources<br/><br/>Visit the Resources section to browse them all!";
        }

        // Registration
        if (msg.Contains("register") || msg.Contains("sign up") || msg.Contains("create account") || msg.Contains("enroll"))
        {
            return "📝 <strong>Registration Guide:</strong><br/><br/>1️⃣ Click 'Register' on the login page<br/>2️⃣ Fill in your details:<br/>• Full Name<br/>• Matric Number<br/>• Email (UTHM email)<br/>• Faculty<br/>• Phone<br/>3️⃣ Create a password (min 6 chars)<br/>4️⃣ Click 'Register'<br/><br/>✅ <strong>You'll be logged in automatically!</strong>";
        }

        // Contact
        if (msg.Contains("contact") || msg.Contains("help") || msg.Contains("support") || msg.Contains("email") || msg.Contains("phone"))
        {
            return "📞 <strong>Contact Us:</strong><br/><br/>📧 <strong>Email:</strong> library@uthm.edu.my<br/>📱 <strong>Phone:</strong> +607-453 7000<br/>🏛️ <strong>Location:</strong> Perpustakaan Tunku Tun Aminah, UTHM<br/>🕐 <strong>Office Hours:</strong> 8:00 AM - 5:00 PM (Weekdays)<br/><br/>👤 <em>Librarian on duty available during office hours</em>";
        }

        // Rules
        if (msg.Contains("rule") || msg.Contains("policy") || msg.Contains("regulation") || msg.Contains("guideline"))
        {
            return "📋 <strong>Library Rules:</strong><br/><br/>✅ <strong>Allowed:</strong><br/>• Quiet study<br/>• Group discussions (meeting rooms)<br/>• Using resources & WiFi<br/><br/>❌ <strong>Not Allowed:</strong><br/>• Food & drinks (except water)<br/>• Loud noise<br/>• Smoking<br/>• Damaging books or equipment<br/><br/>⚠️ <em>Violations may result in penalties</em>";
        }

        // Payment
        if (msg.Contains("payment") || msg.Contains("pay") || msg.Contains("fee") || msg.Contains("cost") || msg.Contains("price"))
        {
            return "💳 <strong>Payment Information:</strong><br/><br/>🔹 <strong>Study Room:</strong> RM10 per session<br/>🔹 <strong>Meeting Room:</strong> FREE<br/><br/>💡 <strong>All payments are:</strong><br/>• Secure 🔒<br/>• Encrypted 🔐<br/>• Non-refundable<br/><br/>📌 <em>Payment is processed online via card or e-wallet</em>";
        }

        // Location
        if (msg.Contains("location") || msg.Contains("where") || msg.Contains("find") || msg.Contains("address") || msg.Contains("map"))
        {
            return "📍 <strong>Library Location:</strong><br/><br/>🏛️ <strong>Perpustakaan Tunku Tun Aminah</strong><br/>Universiti Tun Hussein Onn Malaysia (UTHM)<br/>Parit Raja, 86400 Batu Pahat<br/>Johor, Malaysia<br/><br/>🗺️ <strong>How to find us:</strong><br/>• Main building near the central plaza<br/>• Opposite the Chancellery building<br/>• Accessible via the main entrance<br/><br/>🅿️ <em>Parking available at the library parking lot</em>";
        }

        // Greetings
        if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("hey") || msg.Contains("greetings") || msg.Contains("good morning"))
        {
            var greetings = new[]
            {
                $"👋 Hello {userName}! Welcome to UTHM Library. How can I help you today?",
                $"Hi there {userName}! 😊 What can I assist you with?",
                $"Hey {userName}! Ready to explore the library? 📚"
            };
            return greetings[rand.Next(greetings.Length)];
        }

        // How are you
        if (msg.Contains("how are you") || msg.Contains("how are u") || msg.Contains("how's it going"))
        {
            var responses = new[]
            {
                $"I'm doing great {userName}! Thanks for asking. How can I help you? 😊",
                $"All good {userName}! Ready to help you with your library needs! 🚀",
                $"I'm fantastic {userName}! What brings you here today? 📚"
            };
            return responses[rand.Next(responses.Length)];
        }

        // Goodbye
        if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("see you") || msg.Contains("take care") || msg.Contains("thanks") || msg.Contains("thank you"))
        {
            var goodbyes = new[]
            {
                $"👋 Goodbye {userName}! Have a great day at UTHM Library! 📚",
                $"Take care {userName}! See you at the library! 👋",
                $"You're welcome {userName}! Happy studying! 📚✨"
            };
            return goodbyes[rand.Next(goodbyes.Length)];
        }

        // Fallback
        var fallbacks = new[]
        {
            $"🤖 I can help with:<br/>• 📚 Library hours<br/>• 🏠 Room booking<br/>• 📖 Resources<br/>• 🌐 WiFi<br/>• 📝 Registration<br/>• 📞 Contact",
            $"💡 Try asking me about:<br/>• ⏰ Operating hours<br/>• 💰 Payment information<br/>• 📋 Library rules<br/>• 📍 Location",
            $"📚 I'm here to help! Try:<br/>• 'What are the library hours?'<br/>• 'How to book a room?'<br/>• 'What's the WiFi password?'"
        };
        return fallbacks[rand.Next(fallbacks.Length)];
    }

    [HttpGet]
    public JsonResult GetChatHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

        if (_chatHistory.ContainsKey(userId))
        {
            var history = _chatHistory[userId]
                .OrderByDescending(c => c.Timestamp)
                .Take(50)
                .Select(c => new
                {
                    c.Message,
                    c.Response,
                    c.Sender,
                    Timestamp = c.Timestamp.ToString("hh:mm tt")
                })
                .ToList();

            return Json(history);
        }

        return Json(new List<object>());
    }

    [HttpPost]
    public JsonResult ClearHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

        if (_chatHistory.ContainsKey(userId))
        {
            _chatHistory[userId].Clear();
        }

        return Json(new { success = true });
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    private class ChatMessage
    {
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string Sender { get; set; } = "user";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}