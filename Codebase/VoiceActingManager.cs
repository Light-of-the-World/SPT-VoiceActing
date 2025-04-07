using System.IO;
using UnityEngine;
using EFT.UI;
using UnityEngine.Networking;
using BepInEx.Logging;
using Comfort.Common;
using System.Collections.Generic;
using EFT;
using System.Collections;
using static EFT.Interactive.BetterPropagationGroups;
using Bsg.GameSettings;

namespace VoiceActing.Codebase
{
    internal class VoiceActingManager
    {
        private static readonly string questSoundDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "Plugins", "LightsVoiceActing", "SoundFiles", "QuestVoicelines");
        private static readonly string transactionSoundDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "Plugins", "LightsVoiceActing", "SoundFiles", "BuySellVoicelines");
        private static readonly string sillySoundDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "Plugins", "LightsVoiceActing", "SoundFiles", "SillyMode");
        private static ManualLogSource Log => Plugin.Log;
        private static bool isPlayingVoiceLine = false;
        private static Queue<(AudioClip clip, float volume)> voiceLineQueue = new Queue<(AudioClip clip, float volume)>();
        private static GUISounds _sounds;
        private static SharedGameSettingsClass _volume;
        private static float volume;
        public static string traderId = "54cb50c76803fa8b248b4571";

        public static bool EnsureConnections(out bool canContinue)
        {
            canContinue = false;
            if (_sounds != null && _volume != null)
            {
                Log.LogInfo("All connections good.");
                volume = (_volume.Sound.Settings.OverallVolume) / 100f;
                return true;
            }
            if (_sounds == null)
            {
                Log.LogWarning("_sounds was null, assigning now.");
                _sounds = Singleton<GUISounds>.Instance;
                if (_sounds == null)
                {
                    Log.LogWarning("_sounds IS STILL NULL! Double check your code.");
                    return false;
                }
            }

            if (_volume == null)
            {
                Log.LogWarning("_volume was null, assigning now.");
                _volume = Singleton<SharedGameSettingsClass>.Instance;
                volume = (_volume.Sound.Settings.OverallVolume) / 100f;
                if (_volume == null)
                {
                    Log.LogWarning("_volume IS STILL NULL! Double check your code.");
                    return false;
                }
            }
            return true;
        }

        public static void RunVoiceActingCode(QuestClass quest)
        {
            if (quest.QuestStatus == EFT.Quests.EQuestStatus.Started) PlayAcceptSound(quest);
            else if (quest.QuestStatus == EFT.Quests.EQuestStatus.Success) PlayCompleteSound(quest);
            else Log.LogInfo(quest + " is not being accepted or completed, skipping code");
        }

        public static async void Silly()
        {
            if (!Directory.Exists(sillySoundDirectory))
            {
                Log.LogWarning($"[VoiceActingMod] Silly sound directory does not exist: {sillySoundDirectory}");
                return;
            }

            string[] sillyFiles = Directory.GetFiles(sillySoundDirectory, "*.wav");
            if (sillyFiles.Length == 0)
            {
                Log.LogWarning("[VoiceActingMod] No silly sound files found.");
                return;
            }

            string filePath = sillyFiles[UnityEngine.Random.Range(0, sillyFiles.Length)];
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
            {
                var request = www.SendWebRequest();
                while (!request.isDone)
                    await System.Threading.Tasks.Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Log.LogError($"[VoiceActingMod] Failed to load silly audio clip: {www.error}");
                    return;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null || clip.length == 0)
                {
                    Log.LogWarning("[VoiceActingMod] Loaded silly clip was empty or null.");
                    return;
                }

                Log.LogInfo($"[VoiceActingMod] Playing silly clip: {Path.GetFileName(filePath)}");

                _sounds.PlaySound(clip, false, false, volume);
            }
        }

        public static void PlayAcceptSound(QuestClass quest)
        {
            LoadAndQueueVoiceLine(quest, "AcceptVoice");
        }

        public static void PlayCompleteSound(QuestClass quest)
        {
            LoadAndQueueVoiceLine(quest, "CompleteVoice");
        }

        private static async void LoadAndQueueVoiceLine(QuestClass quest, string suffix)
        {
            string fileName = $"{quest.Id}_{suffix}.wav";
            string filePath = Path.Combine(questSoundDirectory, fileName);

            if (!File.Exists(filePath))
            {
                Log.LogWarning($"[VoiceActingMod] File not found for quest {quest.Id} with suffix {suffix}");
                return;
            }

            Log.LogInfo($"[VoiceActingMod] Attempting to load and queue audio for quest {quest.Id}, file: {filePath}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
            {
                var request = www.SendWebRequest();
                while (!request.isDone)
                    await System.Threading.Tasks.Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Log.LogError($"[VoiceActingMod] Failed to load audio clip: {www.error}");
                    return;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null || clip.length == 0)
                {
                    Log.LogWarning($"[VoiceActingMod] Loaded clip was empty or null for quest {quest.Id}");
                    return;
                }

                Log.LogInfo($"[VoiceActingMod] Successfully loaded audio clip for quest {quest.Id}, queuing now. Length: {clip.length}s");

                EnqueueVoiceLine(clip, volume);
            }
        }

        private static void EnqueueVoiceLine(AudioClip clip, float volume)
        {
            if (isPlayingVoiceLine)
            {
                voiceLineQueue.Enqueue((clip, volume));
                Log.LogInfo("[VoiceActingMod] Voice line is currently playing. Queued new clip.");
            }
            else
            {
                StaticManager.BeginCoroutine(PlayVoiceLineRoutine(clip, volume));
            }
        }

        private static IEnumerator PlayVoiceLineRoutine(AudioClip clip, float volume)
        {
            isPlayingVoiceLine = true;
            _sounds.PlaySound(clip, false, false, volume);
            Log.LogInfo($"[VoiceActingMod] Playing queued clip. Duration: {clip.length}s");
            yield return new WaitForSeconds(clip.length);
            isPlayingVoiceLine = false;

            if (voiceLineQueue.Count > 0)
            {
                var (nextClip, nextVolume) = voiceLineQueue.Dequeue();
                StaticManager.BeginCoroutine(PlayVoiceLineRoutine(nextClip, nextVolume));
            }
        }

        public static void PlayBuySound()
        {
            PlayRandomTraderVoice(traderId, "BuyVoice");
        }

        public static void PlaySellSound()
        {
            PlayRandomTraderVoice(traderId, "SellVoice");
        }

        private static async void PlayRandomTraderVoice(string traderId, string suffix)
        {
            // Search for all matching files like "traderId_BuyVoice1.wav", "traderId_BuyVoice2.wav", etc.
            string[] matchingFiles = Directory.GetFiles(transactionSoundDirectory, $"{traderId}_{suffix}*.wav");
            if (matchingFiles.Length == 0)
            {
                Log.LogWarning($"[VoiceActingMod] No {suffix} clips found for trader {traderId}");
                return;
            }

            // Pick a random file
            string filePath = matchingFiles[UnityEngine.Random.Range(0, matchingFiles.Length)];
            Log.LogInfo($"[VoiceActingMod] Attempting to play {suffix} for trader {traderId}: {Path.GetFileName(filePath)}");

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.WAV))
            {
                var request = www.SendWebRequest();
                while (!request.isDone)
                    await System.Threading.Tasks.Task.Yield();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Log.LogError($"[VoiceActingMod] Failed to load trader audio clip: {www.error}");
                    return;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null || clip.length == 0)
                {
                    Log.LogWarning($"[VoiceActingMod] Loaded trader clip was empty or null for {traderId}");
                    return;
                }

                Log.LogInfo($"[VoiceActingMod] Successfully loaded and playing trader clip. Length: {clip.length}s");
                _sounds.PlaySound(clip, false, false, volume);
            }
        }
    }
}
