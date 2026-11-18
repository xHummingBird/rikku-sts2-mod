using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Powers;
using Rikku.RikkuCode.Relics;

namespace Rikku.RikkuCode.Mechanics;

public static class StealHelper
{
    public static bool HasNinjaClaw(
        Creature owner) =>
        owner.Player?
            .Relics
            .Any(relic =>
                relic is NinjaClaw)
        ?? false;

    public static int GetMaxStealableGold(
        Creature owner,
        Creature target)
    {
        return target.MaxHp *
               40 /
               100;
    }

    public static int GetStolenGold(
        Creature target)
    {
        return (int)(
            target
                .GetPower<StolenGoldPower>()?
                .Amount
            ?? 0);
    }

    public static int GetRemainingGold(
        Creature owner,
        Creature target)
    {
        return Math.Max(
            0,
            GetMaxStealableGold(
                owner,
                target) -
            GetStolenGold(
                target));
    }

    public static int GetStealAmount(
        Creature owner,
        Creature target,
        int baseAmount = 3)
    {
        int requestedAmount =
            baseAmount +
            owner.GetPowerAmount<DaylightRobberyPower>() +
            (HasNinjaClaw(owner)
                ? 2
                : 0);

        return Math.Min(
            requestedAmount,
            GetRemainingGold(
                owner,
                target));
    }
}

[HarmonyPatch(
    typeof(ArtifactPower),
    nameof(ArtifactPower.TryModifyPowerAmountReceived))]
public static class StolenGoldArtifactFix
{
    [HarmonyPrefix]
    public static bool Prefix(
        PowerModel canonicalPower,
        decimal amount,
        ref decimal modifiedAmount,
        ref bool __result)
    {
        if (canonicalPower is not StolenGoldPower)
        {
            return true;
        }

        modifiedAmount =
            amount;

        __result =
            false;

        return false;
    }
}