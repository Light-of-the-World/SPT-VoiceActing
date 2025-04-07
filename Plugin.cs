using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceActing.Codebase;
using VoiceActing.Patches;

namespace VoiceActing
{
    [BepInPlugin("Light.VoiceActingMod", "VoiceActingMod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        public static bool areQuestVoicesOn;
        public static bool areTransactionsVoicesOn;
        public static bool areSillySoundsOn;

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            // save the Logger to variable so we can use it elsewhere in the project
            Log = Logger;
            Log.LogWarning("Loading audio files...");
            new PatchQuestSoundPlayer().Enable();
            new PatchForSillyness().Enable();
            new PatchForTransaction().Enable();
            new PatchForTraderId().Enable();
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "Plugins", "LightsVoiceActing", "config.json");

            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    JObject config = JObject.Parse(json);

                    areQuestVoicesOn = config.Value<bool?>("QuestVoiceLines") ?? true;
                    areTransactionsVoicesOn = config.Value<bool?>("TransactionVoiceLines") ?? true;
                    areSillySoundsOn = config.Value<bool?>("SillyMode") ?? false;

                    Plugin.Log.LogInfo($"[VoiceActingMod] Config loaded: QuestVoices={areQuestVoicesOn}, Transactions={areTransactionsVoicesOn}, SillyMode={areSillySoundsOn}");
                }
                catch (System.Exception ex)
                {
                    Plugin.Log.LogError($"[VoiceActingMod] Failed to read config.json: {ex.Message}");
                    // Fallback to defaults
                    areQuestVoicesOn = true;
                    areTransactionsVoicesOn = true;
                    areSillySoundsOn = false;
                }
            }
            else
            {
                Plugin.Log.LogWarning("[VoiceActingMod] config.json not found! Using default settings.");
                areQuestVoicesOn = true;
                areTransactionsVoicesOn = true;
                areSillySoundsOn = false;
            }
        }
    }   
}