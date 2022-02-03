using System.Linq;
using System.Text.RegularExpressions;

namespace Server
{
    static class FilterSys
    {
        static string[] BlacklistUsername = new string[]
        {
            "server",
            "admin",
            "mod",
            "administrator",
            "moderator",
            "system",
            "test",
        };
        static string[] BlacklistWord = new string[]
        {
            "nigg",
            "fag",
        };
        /// <summary>
        /// Simple Username Filteration System
        /// </summary>
        public static bool Username(string name)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9]+$")) return false; // Can only contain letter and numbers
            if (BlacklistUsername.Any(name.ToLower().Contains)) return false; // Check the blacklist
            return true;
        }
        /// <summary>
        /// Simple Message Filteration System
        /// </summary>

        public static bool Message(string msg)
        {
            if (!Regex.IsMatch(msg, "^[a-zA-Z0-9_!@#$%^&*()-+={}[\\]<>:;<>,.\\?~\\\" ']+$")) return false; // RegExp check the sentence to ensure it doesn't have any weird character
            if (BlacklistWord.Any(msg.ToLower().Contains)) return false; // Ban some extreme words
            if (msg.Length > 3000) return false; // If character exceed 3000. *Not Byte/Bit*
            if (string.IsNullOrWhiteSpace(msg)) return false; // If the message are blank
            return true;
        }
    }
}
