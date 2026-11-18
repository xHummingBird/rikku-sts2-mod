using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Powers;

public class SleightOfHandPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RikkuStaticHoverTips.Mix
    ];

    public override async Task AfterPotionUsed(
        PotionModel potion,
        Creature? target)
    {
        if (potion.Owner.Creature != Owner)
            return;

        /*
         * A normally consumed Potion has already been removed
         * from the player's Potion collection.
         *
         * Stash prevents RemoveBeforeUse(), so the Potion is
         * still present and Sleight of Hand must not trigger.
         */
        if (potion.Owner.Potions.Contains(
                potion))
        {
            return;
        }

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    potion.Owner.UnlockState)
                .ToList();

        if (potionOptions.Count == 0)
            return;

        PotionModel newPotion =
            potion.Owner.RunState
                .Rng
                .CombatPotionGeneration
                .NextItem(
                    potionOptions)
                .ToMutable();

        Flash();

        await PotionCmd.TryToProcure(
            newPotion,
            potion.Owner);

        await PowerCmd.Decrement(this);
    }
}