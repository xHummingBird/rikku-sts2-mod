using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Powers;

public class DoubleDosePower : RikkuPower
{
    private int _triggeredThisTurn;

    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == Owner.Player)
        {
            _triggeredThisTurn =
                0;
        }

        return Task.CompletedTask;
    }

    public override Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner != Owner.Player)
        {
            return Task.CompletedTask;
        }

        /*
         * Do not recursively repeat the synthetic second
         * activation.
         */
        if (RikkuPotionMechanic.IsDoubleDoseReplay(
                potion))
        {
            return Task.CompletedTask;
        }

        /*
         * The first Amount potions used each turn are doubled.
         *
         * The removal prefix temporarily preserved this potion
         * because DoubleDosePower existed. If all triggers have
         * already been used, consume that temporarily preserved
         * potion now.
         */
        if (_triggeredThisTurn >= Amount)
        {
            FinalizeOriginalPotionWithoutReplay(
                potion);

            return Task.CompletedTask;
        }

        Creature? replayTarget =
            GetReplayTarget(
                potion,
                target);

        /*
         * The first effect may have killed the final valid target.
         *
         * There is nothing to replay. Without Stash, consume the
         * temporarily retained potion. With Stash, leave it in the
         * belt.
         *
         * This does not spend a Double Dose trigger.
         */
        if (potion.TargetType == TargetType.AnyEnemy &&
            replayTarget == null)
        {
            if (RikkuPotionMechanic.IsPreservedByStash(
                    potion))
            {
                /*
                 * Stash already physically retained the potion.
                 * No replay will occur, so both temporary markers
                 * can now be cleared.
                 */
                RikkuPotionMechanic
                    .ClearDoubleDosePreservation(
                        potion);

                RikkuPotionMechanic
                    .ClearStashPreservation(
                        potion);
            }
            else
            {
                /*
                 * Double Dose was the only reason the potion remained.
                 * Remove it once combat has finished resolving.
                 */
                RikkuPotionMechanic
                    .MarkForDoubleDoseCombatEndRemoval(
                        potion);
            }

            potion.AfterUsageCanceled();

            return Task.CompletedTask;
        }

        if (potion.TargetType.IsSingleTarget() &&
            replayTarget == null)
        {
            FinalizeOriginalPotionWithoutReplay(
                potion);

            return Task.CompletedTask;
        }

        _triggeredThisTurn++;

        Flash();

        /*
         * Clear IsQueued from the original activation before
         * enqueueing the same PotionModel again.
         */
        potion.AfterUsageCanceled();

        RikkuPotionMechanic.QueueDoubleDoseReplay(
            potion);

        try
        {
            /*
             * The potion is still in the owner's potion list here,
             * so UsePotionAction's constructor validation passes.
             */
            AutomaticPotionFix.EnqueueForOwner(potion, replayTarget);
        }
        catch
        {
            RikkuPotionMechanic
                .CancelPendingDoubleDoseReplay(
                    potion);

            _triggeredThisTurn--;

            /*
             * If Stash did not preserve the potion, consume the
             * temporarily retained original.
             */
            FinalizeOriginalPotionWithoutReplay(
                potion);

            throw;
        }

        return Task.CompletedTask;
    }

    private static void FinalizeOriginalPotionWithoutReplay(
        PotionModel potion)
    {
        /*
         * Stash preserved this potion intentionally.
         *
         * Leave it in the belt. StashPower will clear IsQueued,
         * restore the holder UI, and decrement itself.
         */
        if (RikkuPotionMechanic.IsPreservedByStash(
                potion))
        {
            RikkuPotionMechanic
                .ClearDoubleDosePreservation(
                    potion);

            RikkuPotionMechanic
                .ClearStashPreservation(
                    potion);

            return;
        }

        /*
         * Double Dose temporarily kept this potion in the belt.
         * No replay will happen, so perform the original
         * consumption now.
         */
        if (RikkuPotionMechanic.IsPreservedForDoubleDose(
                potion))
        {
            RikkuPotionMechanic
                .ConsumePreservedPotion(
                    potion);
        }
    }

    private Creature? GetReplayTarget(
        PotionModel potion,
        Creature? originalTarget)
    {
        /*
         * Use the original target if it is still alive and valid.
         */
        if (originalTarget != null &&
            potion.IsValidTarget(
                originalTarget))
        {
            return originalTarget;
        }

        /*
         * If the original enemy died, randomly choose another
         * valid enemy.
         */
        if (potion.TargetType == TargetType.AnyEnemy)
        {
            List<Creature> enemies =
                Owner.CombatState?
                    .HittableEnemies
                    .Where(potion.IsValidTarget)
                    .ToList()
                ?? [];

            if (enemies.Count == 0)
            {
                return null;
            }

            return Owner.Player
                .RunState
                .Rng
                .CombatCardSelection
                .NextItem(
                    enemies);
        }

        /*
         * Self and owner-targetable AnyPlayer potions.
         */
        if (potion.IsValidTarget(
                Owner))
        {
            return Owner;
        }

        /*
         * AllEnemies and other non-single-target potions use a
         * null target intentionally.
         */
        if (!potion.TargetType.IsSingleTarget())
        {
            return null;
        }

        return null;
    }
    
    public override Task AfterCombatEnd(
        CombatRoom room)
    {
        PotionModel? potion =
            RikkuPotionMechanic
                .TakeDoubleDoseCombatEndRemoval();

        if (potion != null)
        {
            RikkuPotionMechanic
                .ConsumePreservedPotion(
                    potion);
        }

        RikkuPotionMechanic
            .ResetDoubleDoseState();

        return Task.CompletedTask;
    }
}