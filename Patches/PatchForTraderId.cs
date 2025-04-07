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
    internal class PatchForTraderId : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderScreensGroup), nameof(TraderScreensGroup.method_6));
        }

        [PatchPostfix]
        private static void Postfix(TraderScreensGroup __instance)
        {
            VoiceActingManager.traderId = __instance.TraderClass.Id;
            Plugin.Log.LogInfo("Set current traderId to " +  VoiceActingManager.traderId);
        }
    }
}
