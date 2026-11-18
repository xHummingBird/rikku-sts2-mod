using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Cards.Ancient;
using Rikku.RikkuCode.Cards.Rare;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Relics;

public class Godhand : StealRelicBase
{
    private bool _triggeredThisCombat;

    public override RelicRarity Rarity =>
        RelicRarity.Ancient;

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainMaxPotionCount(
            2,
            Owner);
    }

    public override Task BeforeCombatStart()
    {
        _triggeredThisCombat =
            false;

        return base.BeforeCombatStart();
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await base.AfterCardPlayed(
            choiceContext,
            cardPlay);
        
        if (_triggeredThisCombat)
            return;

        if (cardPlay.Card.Owner != Owner)
            return;

        if (!cardPlay.Card.Keywords.Contains(
                RikkuEnums.Steal))
        {
            return;
        }

        _triggeredThisCombat =
            true;

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
                .Where(potion =>
                    potion.CanBeGeneratedInCombat)
                .ToList();

        if (potionOptions.Count == 0)
            return;

        Flash();

        for (int i = 0; i < 2; i++)
        {
            if (!Owner.HasOpenPotionSlots)
                break;

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

            if (!result.success)
                break;
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal damage,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (cardSource is not Mix &&
            cardSource is not QuickMix)
        {
            return 1m;
        }

        if (cardSource.Owner != Owner)
            return 1m;

        return 1.25m;
    }
}