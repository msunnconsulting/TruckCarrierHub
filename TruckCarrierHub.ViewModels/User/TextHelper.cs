using System.Linq;

namespace PartnerCarrier.ViewModels.User
{
    public class TextHelper
    {
        public static TextHelperResponse TruncateText(string text, int wordLimit)
        {
            var resposne = new TextHelperResponse();
            var words = text.Split(' ');
            if (words.Length <= wordLimit)
            {
                resposne.truncatedText = text;
                resposne.hasMore = false;
                return resposne;
            }

            var truncated = string.Join(" ", words.Take(wordLimit)) + "...";
            resposne.hasMore = true;
            resposne.truncatedText = truncated;
            return resposne;
        }

        public class TextHelperResponse
        {
            public string truncatedText { get; set; }
            public bool hasMore { get; set; }
        }
    }
}
