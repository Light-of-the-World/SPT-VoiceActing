using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoiceActing.Codebase;

namespace VoiceActing.Patches
{
    internal class PatchQuestSoundPlayer : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass3700), nameof(GClass3700.TryNotifyConditionalStatusChanged));
        }

    [PatchPostfix]
        private static void Postfix(GClass3700 __instance, QuestClass quest)
        {
            if (Plugin.areQuestVoicesOn == true)
            {
                Plugin.Log.LogInfo("Searching for id " + quest.Id);
                bool canRun = VoiceActingManager.EnsureConnections(out canRun);
                if (canRun) VoiceActingManager.RunVoiceActingCode(quest);
            }
        }
    }
}
