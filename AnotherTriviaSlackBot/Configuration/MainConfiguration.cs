using Newtonsoft.Json;
using NLog;
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
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties
        /// <summary>
        /// Gets or sets the slack authentication token.
        /// </summary>
        /// <value>
        /// The slack authentication token.
        /// </value>
        public string SlackAuthToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the channel for the bot.
        /// </summary>
        /// <value>
        /// The name of the channel for the bot.
        /// </value>
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the bot should announce to channel before start.
        /// </summary>
        /// <value>
        /// <c>true</c> if the bot should announce to channel before start; otherwise, <c>false</c>.
        /// </value>
        public bool AnnounceToChannelBeforeStart { get; set; }

        /// <summary>
        /// Gets or sets the start delay seconds after announce.
        /// </summary>
        /// <value>
        /// The start delay seconds after announce.
        /// </value>
        public int StartDelaySecondsAfterAnnounce { get; set; }

        /// <summary>
        /// Gets or sets the questions per round.
        /// </summary>
        /// <value>
        /// The questions per round.
        /// </value>
        public int QuestionsPerRound { get; set; }

        /// <summary>
        /// Gets or sets the show hint after seconds.
        /// </summary>
        /// <value>
        /// The show hint after seconds.
        /// </value>
        public int ShowHintAfterSeconds { get; set; }

        /// <summary>
        /// Gets or sets the show answer after seconds.
        /// </summary>
        /// <value>
        /// The show answer after seconds.
        /// </value>
        public int ShowAnswerAfterSeconds { get; set; }

        /// <summary>
        /// Gets or sets the default category.
        /// </summary>
        /// <value>
        /// The default category.
        /// </value>
        public string DefaultCategory { get; set; }
        #endregion

        #region Load configuration
        /// <summary>
        /// Gets or sets the settings file that contains JSON configuration.
        /// </summary>
        /// <value>
        /// The settings file that contains JSON configuration.
        /// </value>
        public static string SettingsFile { get; set; }

        /// <summary>
        /// Loads this the settings from the JSON file.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Configuration file not found</exception>
        /// <exception cref="ArgumentException">Invalid configuration file.</exception>
        public static MainConfiguration Load()
        {
            // get the path to the default settings file
            string configPath = Path.Combine(GetDataDirectory(), "settings.json");

            // check if a custom settings file is specified
            if (!string.IsNullOrWhiteSpace(SettingsFile))
            {
                // if it is a full path to settings file, just set it
                if (File.Exists(SettingsFile))
                {
                    configPath = SettingsFile;
                }
                else
                {
                    // else assume its relative from the current data directory
                    configPath = Path.Combine(GetDataDirectory(), SettingsFile);
                }
            }
            
            // check if the settings file exists
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Configuration file not found", configPath);

            try
            {
                // try to load settings file and parse the json to this object
                var configJson = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<MainConfiguration>(configJson);
            }
            catch (Exception ex)
            {
                // if it fails, log and throw invalid config exception
                logger.Error(ex, $"Failed to load the configuration file '{configPath}'");
                throw new ArgumentException("Invalid configuration file.");
            }
        }

        /// <summary>
        /// Gets the data directory for the app domain (current working folder).
        /// </summary>
        /// <returns></returns>
        private static string GetDataDirectory()
        {
            string text = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(text))
            {
                text = AppDomain.CurrentDomain.BaseDirectory;
            }
            return text;
        }
        #endregion
    }
}
