
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
