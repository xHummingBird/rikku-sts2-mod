using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Relics;

public class GoldenHand : RikkuRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override async Task BeforeCombatStartLate()
    {
        if (!Owner.HasOpenPotionSlots)
            return;

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
                .Where(potion =>
                    potion.CanBeGeneratedInCombat)
                .ToList();

        if (potionOptions.Count == 0)
            return;

        PotionModel potion =
            Owner.RunState
                .Rng
                .CombatPotionGeneration
                .NextItem(
                    potionOptions)
                .ToMutable();

        PotionProcureResult result =
            await PotionCmd.TryToProcure(
                potion,
                Owner);

        if (result.success)
        {
            Flash();
        }
    }
}