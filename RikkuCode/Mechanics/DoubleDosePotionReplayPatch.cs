using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Patches;

[HarmonyPatch(
    typeof(PotionModel),
    nameof(PotionModel.OnUseWrapper))]
public static class DoubleDosePotionReplayPatch
{
    [HarmonyPrefix]
    public static void Prefix(
        PotionModel __instance,
        out bool __state)
    {
        /*
         * The pending replay becomes active immediately before
         * its own OnUseWrapper execution.
         */
        __state =
            RikkuPotionMechanic
                .TryBeginDoubleDoseReplay(
                    __instance);
    }

    [HarmonyPostfix]
    public static void Postfix(
        PotionModel __instance,
        bool __state,
        ref Task __result)
    {
        if (!__state)
        {
            return;
        }

        __result =
            CompleteReplayAfterTask(
                __instance,
                __result);
    }

    private static async Task CompleteReplayAfterTask(
        PotionModel potion,
        Task replayTask)
    {
        try
        {
            /*
             * Keep the active replay marker through the complete
             * potion pipeline, including AfterPotionUsed hooks.
             */
            await replayTask;
        }
        finally
        {
            RikkuPotionMechanic
                .CompleteDoubleDoseReplay(
                    potion);

            /*
             * Relevant when Stash retained the potion.
             *
             * If the replay consumed the potion normally, clearing
             * IsQueued on the removed instance is harmless.
             */
            potion.AfterUsageCanceled();
        }
    }
}