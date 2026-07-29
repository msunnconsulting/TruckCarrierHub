using System.Linq;
using System.Text.RegularExpressions;

namespace PartnerCarrier.ViewModels.User
{
    public class TextHelper
    {
        public static TextHelperResponse TruncateText(string text, int wordLimit)
        {
            var response = new TextHelperResponse();

            // Count words from plain text so HTML tags don't inflate the threshold check.
            var plain = Regex.Replace(text ?? "", "<[^>]+>", " ");
            plain = Regex.Replace(plain, @"\s+", " ").Trim();
            var plainWordCount = plain.Length == 0 ? 0 : plain.Split(' ').Length;

            if (plainWordCount <= wordLimit)
            {
                response.truncatedText = text;
                response.hasMore = false;
                return response;
            }

            // Truncate the original (preserving any markup) at the word-limit boundary.
            var originalWords = text.Split(' ');
            response.truncatedText = string.Join(" ", originalWords.Take(wordLimit)) + "...";
            response.hasMore = true;
            return response;
        }

        public class TextHelperResponse
        {
            public string truncatedText { get; set; }
            public bool hasMore { get; set; }
        }
    }
}
