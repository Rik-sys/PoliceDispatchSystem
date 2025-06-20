//מחלקה שבעצם תיקרא לי את הנתונים מתוך הקובץ קונפיגורציה ותשפוך אותם כביכול למחלקה Config
//כדי שיהיה נח להשתמש בנתונים של הקובץ קונפיגורציה

using Microsoft.Extensions.Configuration;

namespace DTO
{
    public static class ConfigurationLoader
    {
        public static void LoadOnewaySettings(IConfiguration configuration)
        {
            var onewaySection = configuration.GetSection("OnewaySettings");

            if (onewaySection.Exists())
            {
                if (double.TryParse(onewaySection["ReverseDirectionPenalty"], out double penalty))
                    Config.ReverseDirectionPenalty = penalty;

                if (bool.TryParse(onewaySection["AllowReverseDirection"], out bool allowReverse))
                    Config.AllowReverseDirection = allowReverse;

                if (bool.TryParse(onewaySection["VerboseOnewayLogging"], out bool verbose))
                    Config.VerboseOnewayLogging = verbose;
            }
        }
    }
}
