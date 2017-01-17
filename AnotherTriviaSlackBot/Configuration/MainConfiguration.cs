using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Configuration
{
    public class MainConfiguration
    {
        public string SlackAuthToken { get; set; }

        public string ChannelName { get; set; }

        public bool AnnounceToChannelBeforeStart { get; set; }

        public int StartDelaySecondsAfterAnnounce { get; set; }

        public int QuestionsPerRound { get; set; }

        public int ShowHintAfterSeconds { get; set; }

        public int ShowAnswerAfterSeconds { get; set; }

        public string DefaultCategory { get; set; }

        public static MainConfiguration Load()
        {
            string configPath = Path.Combine(GetDataDirectory(), "settings.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Configuration file not found", configPath);

            try
            {
                var configJson = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<MainConfiguration>(configJson);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid configuration file.");
            }
        }

        private static string GetDataDirectory()
        {
            string text = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(text))
            {
                text = AppDomain.CurrentDomain.BaseDirectory;
            }
            return text;
        }
    }
}
