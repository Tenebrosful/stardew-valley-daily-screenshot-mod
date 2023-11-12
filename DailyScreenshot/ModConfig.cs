﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace DailyScreenshot
{
    class ModConfig
    {

        /// <summary>
        /// Trace messages to the console
        /// </summary>
        /// <param name="message">Text to display</param>
        void MTrace(string message) => ModEntry.g_dailySS.MTrace(message);

        /// <summary>
        /// Warning messages to the console
        /// </summary>
        /// <param name="message">Text to display</param>
        void MWarn(string message) => ModEntry.g_dailySS.MWarn(message);

        /// <summary>
        /// Error messages to the console
        /// </summary>
        /// <param name="message">Text to display</param>
        void MError(string message) => ModEntry.g_dailySS.MError(message);

        /// <summary>
        /// GUID used to identify autogenerated rules
        /// </summary>
        private string m_launchGuid;

        /// <summary>
        /// String to use to indicate a default value
        /// </summary>
        public static string DEFAULT_STRING = "Default";

        /// <summary>
        /// Zoom to use (reduce map to 1/4 of original size)
        /// </summary>
        public const float DEFAULT_ZOOM = 0.25f;

        /// <summary>
        /// Start of the day in Stardew Valley (6 am)
        /// </summary>
        public const int DEFAULT_START_TIME = 600;

        /// <summary>
        /// End of the day in Stardew Valley (2 am)
        /// </summary>
        public const int DEFAULT_END_TIME = 2600;

        /// <summary>
        /// Configurable toggle for auditory effects when taking screenshot.
        /// </summary>
        public bool auditoryEffects = true;

        /// <summary>
        /// Configurable toggle for visual effects when taking screenshot.
        /// </summary>
        public bool visualEffects = true;

        /// <summary>
        /// Configurable toggle for ingame notifications when taking screenshot.
        /// </summary>
        public bool screenshotNotifications = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is any.
        /// </summary>
        public bool weatherAny = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is sunny.
        /// </summary>
        public bool weatherSunny = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is rainy.
        /// </summary>
        public bool weatherRainy = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is windy.
        /// </summary>
        public bool weatherWindy = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is stormy.
        /// </summary>
        public bool weatherStormy = true;

        /// <summary>
        /// Configurable toggle for allowing screenshots when weather is snowy.
        /// </summary>
        public bool weatherSnowy = true;

        /// <summary>
        /// Rules loaded from the config file
        /// </summary>
        public List<ModRule> SnapshotRules { get; set; } = new List<ModRule>();

        // Place to put json that doesn't match properties here
        // This can be used to upgrade the config file
        // See: https://www.newtonsoft.com/json/help/html/SerializationAttributes.htm#JsonExtensionDataAttribute
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = null;

        [JsonIgnore]
        internal bool RulesModified { get; set; } = false;

#if false
        public SButton TakeScreenshotKey { get; set; }

        public float TakeScreenshotKeyZoomLevel { get; set; }

        public string FolderDestinationForKeypressScreenshots { get; set; }
#endif

        public ModConfig()
        {
            m_launchGuid = Guid.NewGuid().ToString();
            SnapshotRules.Add(new ModRule());
            SnapshotRules[0].Name = m_launchGuid;
        }

        public void Reset()
        {
            auditoryEffects = true;
            visualEffects = true;
            screenshotNotifications = true;
        }

        private T GetOldData<T>(IDictionary<string, JToken> oldDatDict, string key, T defaultValue)
        {
            if (oldDatDict.TryGetValue(key, out JToken value))
            {
                return value.ToObject<T>();
            }
            return default;
        }

        /// <summary>
        /// If the user has the old mod configuration format,
        /// migrate it to the new format
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        private void OnDeserializedFixup(StreamingContext context)
        {
            // If there's no extra Json attributes, there's nothing to fixup
            if (_additionalData == null)
                return;
            try
            {
                // Convert the automatic snapshot rules to the new format
                if (_additionalData.TryGetValue("HowOftenToTakeScreenshot", out JToken oldSSRules))
                {
                    ModRule autoRule;
                    if (SnapshotRules.Count == 1 && SnapshotRules[0].Name == m_launchGuid)
                        autoRule = SnapshotRules[0];
                    else
                    {
                        autoRule = new ModRule();
                        SnapshotRules.Add(autoRule);
                    }
                    ModTrigger autoTrigger = autoRule.Trigger;
                    autoRule.FileName = ModRule.FileNameFlags.Default;
                    autoTrigger.Location = ModTrigger.LocationFlags.Farm;
                    autoTrigger.EndTime = DEFAULT_END_TIME;
                    if (_additionalData.TryGetValue("TakeScreenshotOnRainyDays", out JToken rainyDays))
                    {
                        if (!(bool)rainyDays)
                            autoTrigger.Weather = ModTrigger.WeatherFlags.Snowy |
                                ModTrigger.WeatherFlags.Sunny |
                                ModTrigger.WeatherFlags.Windy;
                        else
                            autoTrigger.Weather = ModTrigger.WeatherFlags.Any;
                    }
                    else
                        autoTrigger.Weather = ModTrigger.WeatherFlags.Any;
                    // Clear the default for a new value
                    autoTrigger.Days = ModTrigger.DateFlags.AnySeason;
                    Dictionary<string, bool> ssDict = oldSSRules.ToObject<Dictionary<string, bool>>();
                    foreach (string key in ssDict.Keys)
                    {
                        if (ssDict[key])
                        {
                            // Replace "Last Day of Month" with "LastDayOfTheMonth"
                            string key_to_enum = key.Replace("of", "OfThe").Replace(" ", "");
                            if (Enum.TryParse<ModTrigger.DateFlags>(key_to_enum, out ModTrigger.DateFlags date))
                                autoTrigger.Days |= date;
                            else
                                MWarn($"Unknown key: \"{key}\"");
                        }
                    }

                    autoRule.Directory = GetOldData<string>(_additionalData,
                        "FolderDestinationForDailyScreenshots", DEFAULT_STRING);
                    autoRule.ZoomLevel = GetOldData<float>(_additionalData,
                        "TakeScreenshotZoomLevel", DEFAULT_ZOOM);
                    autoTrigger.StartTime = GetOldData<int>(_additionalData,
                        "TimeScreenshotGetsTakenAfter", DEFAULT_START_TIME);
                    RulesModified = true;
                    SButton button = GetOldData<SButton>(_additionalData,
                        "TakeScreenshotKey", SButton.None);
                    if (button != SButton.None)
                    {
                        ModRule keyRule = new ModRule();
                        keyRule.Trigger.Key = button;
                        keyRule.Trigger.Location = ModTrigger.LocationFlags.Any;
                        keyRule.ZoomLevel = GetOldData<float>(_additionalData,
                            "TakeScreenshotKeyZoomLevel", DEFAULT_ZOOM);
                        keyRule.Directory = GetOldData<string>(_additionalData,
                            "FolderDestinationForKeypressScreenshots", DEFAULT_STRING);
                        keyRule.FileName = ModRule.FileNameFlags.None;
                        SnapshotRules.Add(keyRule);
                    }

                }
            }
            catch (Exception ex)
            {
                MWarn($"Unable to read old config. Technical details:{ex}");
            }
            _additionalData = new Dictionary<string, JToken>();
        }

        /// <summary>
        /// Used to sort the rules so that items that are being
        /// moved are created first (to prevent a missing file)
        /// in the snapshots directory
        /// </summary>
        internal void SortRules()
        {
            SnapshotRules.Sort();
        }

        /// <summary>
        /// Validates and fixes up as best as possible all user input
        /// Sets the RulesModified property if fixup was required
        /// </summary>
        public void ValidateUserInput()
        {
            int cnt = 0;
            foreach (ModRule rule in SnapshotRules)
            {
                if (string.IsNullOrEmpty(rule.Name) || rule.Name == m_launchGuid)
                {
                    cnt++;
                    rule.Name = $"Unnamed Rule {cnt}";
                    MWarn(I18n.Warning_UnnamedRule(rule.Name));
                    RulesModified = true;
                }
                if (rule.ValidateUserInput())
                    RulesModified = true;
            }
            for (int i = 0; i < SnapshotRules.Count; i++)
            {
                if (SnapshotRules[i].Enabled &&
                    ModRule.FileNameFlags.None != SnapshotRules[i].FileName &&
                    ModRule.FileNameFlags.None == (ModRule.FileNameFlags.UniqueID & SnapshotRules[i].FileName))
                {
                    for (int j = i + 1; j < SnapshotRules.Count; j++)
                    {
                        if (SnapshotRules[j].Enabled)
                        {
                            if (SnapshotRules[i].FileNamesCanOverlap(SnapshotRules[j]))
                            {
                                MWarn(I18n.Warning_RuleOverlap(SnapshotRules[i].Name, SnapshotRules[j].Name));
                            }
                        }
                    }
                }
                if (SnapshotRules[i].FileName != ModRule.FileNameFlags.None &&
                    ModRule.FileNameFlags.None ==
                        (SnapshotRules[i].FileName & ModRule.FileNameFlags.GameID) &&
                    ModRule.FileNameFlags.None ==
                        (SnapshotRules[i].FileName & ModRule.FileNameFlags.UniqueID))
                {
                    MWarn(I18n.Warning_SaveOverlap(SnapshotRules[i].Name));
                }
            }
        }
    }
}
