using System.Text.Json;

namespace UTHMLibrary.Data;

public class ChatKnowledge
{
    private static Dictionary<string, List<string>> _knowledge = new();

    public static void LoadKnowledge()
    {
        // Load from JSON file or define here
        _knowledge = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // General
            ["hello hi hey greetings"] = new List<string> {
                "👋 Hello {user}! Welcome to UTHM Library. How can I help you today?",
                "Hi there {user}! 😊 What can I assist you with?",
                "Hey {user}! Ready to explore the library? 📚"
            },

            ["how are you how are u"] = new List<string> {
                "I'm doing great {user}! Thanks for asking. 😊",
                "All good {user}! Ready to help you! 🚀"
            },

            ["what is your name who are you"] = new List<string> {
                "I'm the UTHM Library AI Assistant! 🤖 I'm here to help with library services, room bookings, and resources.",
                "I'm your friendly library assistant! 📚 Ask me anything about UTHM Library."
            },

            ["library hours open close operating"] = new List<string> {
                "📚 <strong>UTHM Library Hours:</strong><br/>• Monday-Friday: <strong>8:00 AM - 10:00 PM</strong><br/>• Saturday-Sunday: <strong>9:00 AM - 6:00 PM</strong><br/>• Public Holidays: <strong>Closed</strong>"
            },

            ["book booking room study"] = new List<string> {
                "📖 <strong>Room Booking Guide:</strong><br/><br/><strong>Study Room (RM10/session):</strong><br/>• Select available room (green indicator)<br/>• Choose date & time (2 hours max)<br/>• Complete payment online<br/>• Instant confirmation<br/><br/><strong>Meeting Room (FREE):</strong><br/>• Select available room<br/>• Enter 5 unique matric numbers<br/>• Admin approval required"
            },

            ["wifi internet network"] = new List<string> {
                "🌐 <strong>WiFi Information:</strong><br/>• Network: <strong>UTHM-Library</strong><br/>• Password: <strong>library@2024</strong><br/>• Available throughout the library"
            },

            ["past year paper exam"] = new List<string> {
                "📄 <strong>Past Year Papers:</strong><br/>• Available from all faculties<br/>• Search by subject, faculty, or year<br/>• View and download from Resources section"
            },

            ["ebook e-book digital book"] = new List<string> {
                "📖 <strong>E-Books:</strong><br/>• Digital books and references<br/>• Available for download<br/>• Browse by category or author"
            },

            ["project fyp report"] = new List<string> {
                "📊 <strong>Project Reports:</strong><br/>• Undergraduate FYP archive<br/>• Faculty-specific collections<br/>• Access from Resources section"
            },

            ["register signup create account"] = new List<string> {
                "📝 <strong>Registration Guide:</strong><br/>1. Click 'Register' on login page<br/>2. Fill in your details<br/>3. Create a password (min 6 chars)<br/>4. Click 'Register'<br/>✅ You'll be logged in automatically!"
            },

            ["contact help support email phone"] = new List<string> {
                "📞 <strong>Contact Us:</strong><br/>📧 Email: library@uthm.edu.my<br/>📱 Phone: +607-453 7000<br/>🏛️ Location: Perpustakaan Tunku Tun Aminah, UTHM"
            },

            ["rules policy regulation"] = new List<string> {
                "📋 <strong>Library Rules:</strong><br/>✅ <strong>Allowed:</strong> Quiet study, Group discussions (meeting rooms), Using resources & WiFi<br/>❌ <strong>Not Allowed:</strong> Food & drinks (except water), Loud noise, Smoking, Damaging books or equipment"
            },

            ["payment pay fee cost price"] = new List<string> {
                "💳 <strong>Payment Information:</strong><br/><strong>Study Room:</strong> RM10 per session<br/><strong>Meeting Room:</strong> FREE<br/>💡 All payments are secure and encrypted."
            },

            ["location where find address"] = new List<string> {
                "📍 <strong>Library Location:</strong><br/>🏛️ Perpustakaan Tunku Tun Aminah<br/>Universiti Tun Hussein Onn Malaysia (UTHM)<br/>Parit Raja, 86400 Batu Pahat<br/>Johor, Malaysia"
            },

            ["bye goodbye see you thank thanks"] = new List<string> {
                "👋 Goodbye {user}! Have a great day at UTHM Library! 📚",
                "Take care {user}! See you at the library! 👋",
                "You're welcome {user}! Happy studying! 📚"
            }
        };
    }

    public static string GetResponse(string message, string userName)
    {
        LoadKnowledge();

        var msg = message.ToLower().Trim();
        var rand = new Random();  // ← Use 'rand' instead of 'random'

        foreach (var kvp in _knowledge)
        {
            var keywords = kvp.Key.Split(' ');
            if (keywords.Any(k => msg.Contains(k)))
            {
                var responses = kvp.Value;
                var response = responses[rand.Next(responses.Count)];
                return response.Replace("{user}", userName);
            }
        }

        // Default fallback responses
        var fallbacks = new[]
        {
            "🤖 I can help with:<br/>• 📚 Library hours<br/>• 🏠 Room booking<br/>• 📖 Resources<br/>• 🌐 WiFi<br/>• 📝 Registration<br/>• 📞 Contact",
            "💡 Try asking me about:<br/>• ⏰ Operating hours<br/>• 💰 Payment information<br/>• 📋 Library rules<br/>• 📍 Location",
            "📚 I'm here to help! Try:<br/>• 'What are the library hours?'<br/>• 'How to book a room?'<br/>• 'What's the WiFi password?'"
        };

        return fallbacks[rand.Next(fallbacks.Length)];  // ← Reuse 'rand' here
    }
}