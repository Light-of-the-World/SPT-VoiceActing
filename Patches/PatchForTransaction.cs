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
    internal class PatchForTransaction : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderDealScreen), nameof(TraderDealScreen.method_0));
        }

        [PatchPostfix]
        private static void Postfix(TraderDealScreen __instance)
        {
            if (Plugin.areTransactionsVoicesOn == true)
            {
                Plugin.Log.LogWarning("Trader deal was made. Type of deal is currently unknown, will be added later. Assuming Buy for now. Trader Id is currently unknown, will be added later. Assuming Prapor for now.");
                bool canRun = VoiceActingManager.EnsureConnections(out canRun);
                if (canRun)
                {
                    if (__instance.ETradeMode_0 == ETradeMode.Purchase) { VoiceActingManager.PlayBuySound(); Plugin.Log.LogInfo("Purchase was just made"); }//DON'T FORGET TO CHANGE THIS ONCE YOU GET THE TRADER FIGURED OUT
                    else if (__instance.ETradeMode_0 == ETradeMode.Sale) { VoiceActingManager.PlaySellSound(); Plugin.Log.LogInfo("Sale was just made"); } //DON'T FORGET TO CHANGE THIS ONCE YOU GET THE TRADER FIGURED OUT
                    else Plugin.Log.LogWarning("Transaction is neither a buy nor a sell. Your code is wrong.");
                }
            }
        }
    }
}
