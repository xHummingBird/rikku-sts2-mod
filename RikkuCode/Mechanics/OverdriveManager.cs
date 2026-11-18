using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Powers;
using Rikku.RikkuCode.Relics;

namespace Rikku.RikkuCode.Mechanics;

public static class OverdriveManager
{
    public const int DefaultMaxOverdrive = 100;

    // =====================================================================
    // READ
    // =====================================================================

    public static int GetOverdrive(
        Player? player)
    {
        StealRelicBase? relic =
            GetOverdriveRelic(player);

        return relic?.StoredOverdrive ?? 0;
    }

    public static int GetMaxOverdrive(
        Player? player)
    {
        StealRelicBase? relic =
            GetOverdriveRelic(player);

        return relic?.MaxOverdrive
               ?? DefaultMaxOverdrive;
    }

    public static bool IsFull(
        Player? player)
    {
        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return false;

        return relic.StoredOverdrive >=
               relic.MaxOverdrive;
    }

    public static bool HasOverdrive(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return true;

        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return false;

        return relic.StoredOverdrive >= amount;
    }

    // =====================================================================
    // SET
    // =====================================================================

    /// <summary>
    /// Sets Overdrive and returns the actual signed change.
    ///
    /// Examples:
    /// 40 to 60 returns 20.
    /// 60 to 40 returns -20.
    /// </summary>
    public static int SetOverdrive(
        Player? player,
        int amount)
    {
        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return 0;

        int previousAmount =
            relic.StoredOverdrive;

        int newAmount =
            Math.Clamp(
                amount,
                0,
                relic.MaxOverdrive);

        if (previousAmount == newAmount)
            return 0;

        relic.StoredOverdrive =
            newAmount;

        relic.NotifyOverdriveChanged(
            previousAmount,
            newAmount);

        return newAmount - previousAmount;
    }

    // =====================================================================
    // GAIN
    // =====================================================================

    /// <summary>
    /// Adds Overdrive and returns the amount actually gained.
    ///
    /// Adding 10 at 95/100 returns 5.
    /// </summary>
    public static int GainOverdrive(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return 0;

        int previousAmount =
            relic.StoredOverdrive;

        SetOverdrive(
            player,
            previousAmount + amount);

        return relic.StoredOverdrive -
               previousAmount;
    }

    // =====================================================================
    // SPEND
    // =====================================================================

    /// <summary>
    /// Attempts to spend the complete amount.
    /// Nothing is spent if there is not enough Overdrive.
    /// </summary>
    public static bool TrySpendOverdrive(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return true;

        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return false;

        if (relic.StoredOverdrive < amount)
            return false;

        SetOverdrive(
            player,
            relic.StoredOverdrive - amount);

        return true;
    }

    /// <summary>
    /// Spends up to the requested amount and returns the
    /// amount actually spent.
    /// </summary>
    public static int SpendOverdrive(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        StealRelicBase? relic =
            GetOverdriveRelic(player);

        if (relic == null)
            return 0;

        int amountSpent =
            Math.Min(
                relic.StoredOverdrive,
                amount);

        if (amountSpent <= 0)
            return 0;

        SetOverdrive(
            player,
            relic.StoredOverdrive - amountSpent);

        return amountSpent;
    }

    // =====================================================================
    // READY POWER
    // =====================================================================

    public static async Task CheckOverdriveReady(
        Creature owner,
        PlayerChoiceContext? choiceContext,
        Creature? source,
        CardModel? card)
    {
        int overdrive =
            GetOverdrive(owner.Player);

        int maxOverdrive =
            GetMaxOverdrive(owner.Player);

        bool isReady =
            overdrive >= maxOverdrive;

        bool alreadyReady =
            owner.HasPower<OverdriveReadyPower>();

        if (isReady && !alreadyReady)
        {
            await PowerCmd.Apply<OverdriveReadyPower>(
                choiceContext,
                owner,
                1m,
                source,
                card,
                false);
        }
        else if (!isReady && alreadyReady)
        {
            await PowerCmd.Remove<OverdriveReadyPower>(
                owner);
        }
    }

    // =====================================================================
    // RESET
    // =====================================================================

    public static void ResetOverdrive(
        Player? player)
    {
        SetOverdrive(
            player,
            0);
    }

    // =====================================================================
    // RELIC LOOKUP
    // =====================================================================

    public static StealRelicBase?
        GetOverdriveRelic(
            Player? player)
    {
        if (player == null)
            return null;

        return player.Relics
            .OfType<StealRelicBase>()
            .FirstOrDefault();
    }
}