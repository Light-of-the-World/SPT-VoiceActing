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
    internal class PatchForSillyness : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TweenAnimatedButton), nameof(TweenAnimatedButton.OnPointerClick));
        }

        [PatchPostfix]
        private static void Postfix(TweenAnimatedButton __instance)
        {
            if (Plugin.areSillySoundsOn == true)
            {
                Plugin.Log.LogWarning("A Tween button was clicked while silly mode was enabled. Commencing MetalPipe.Wav");
                bool canRun = VoiceActingManager.EnsureConnections(out canRun);
                if (canRun) VoiceActingManager.Silly();
            }
        }
    }
}
