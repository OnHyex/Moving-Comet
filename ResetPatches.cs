using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Moving_Comet
{
    internal class ResetPatches
    {
        [HarmonyPatch(typeof(PLUIEscapeMenu), "OnClick_Disconnect")]
        internal class DisconnectResetPatch
        {
            static void Postfix()
            {
                Patches.CometInitial = null;
                Patches.numJumps = 0;
            }
        }
        [HarmonyPatch(typeof(PLUIMainMenu), "ClickPlay")]
        internal class PlayResetPatch
        {
            static void Postfix()
            {
                Patches.CometInitial = null;
                Patches.numJumps = 0;
            }
        }
        [HarmonyPatch(typeof(PLUIMainMenu), "ClickLoadGame")]
        internal class MainMenuLoadResetPatch
        {
            static void Postfix()
            {
                Patches.CometInitial = null;
                Patches.numJumps = 0;
            }
        }
        [HarmonyPatch(typeof(PLGameOverScreen), "ClickRetry")]
        internal class RetryResetPatch
        {
            static void Postfix()
            {
                Patches.CometInitial = null;
                Patches.numJumps = 0;
            }
        }
        [HarmonyPatch(typeof(PLGameOverScreen), "ClickLoad")]
        internal class LoadResetPatch
        {
            static void Postfix()
            {
                Patches.CometInitial = null;
                Patches.numJumps = 0;
            }
        }
    }
}
