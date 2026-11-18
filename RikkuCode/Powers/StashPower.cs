using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Powers;

public class StashPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner.Creature != Owner)
        {
            return;
        }

        /*
         * Double Dose's second activation is not a second potion
         * consumption.
         *
         * Do not spend another Stash stack and do not restore the
         * holder UI twice.
         */
        if (RikkuPotionMechanic.IsDoubleDoseReplay(
                potion))
        {
            return;
        }

        potion.AfterUsageCanceled();

        if (LocalContext.IsMe(
                potion.Owner))
        {
            NRun.Instance?
                .GlobalUi
                .TopBar
                .PotionContainer
                .OnPotionUseOrDiscardCanceled(
                    potion);
        }

        await PowerCmd.Decrement(
            this);

        /*
         * If Double Dose is not present, no replay will need the
         * Stash preservation marker.
         *
         * When Double Dose is present, DoubleDosePower either
         * transfers this information to the queued replay or
         * clears it when no replay is possible.
         */
        if (!Owner.HasPower<DoubleDosePower>())
        {
            RikkuPotionMechanic
                .ClearStashPreservation(
                    potion);
        }
    }
}