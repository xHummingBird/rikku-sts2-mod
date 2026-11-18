using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Relics;

public class Claw : StealRelicBase
{
    private bool _triggeredThisCombat;

    public override RelicRarity Rarity =>
        RelicRarity.Starter;

    public override Task BeforeCombatStart()
    {
        _triggeredThisCombat = false;

        return base.BeforeCombatStart();
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RikkuStaticHoverTips.Overkill
    ];
        
    
    public override async Task AfterObtained()
    {
        await PlayerCmd.GainMaxPotionCount(3, base.Owner);
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

        CardModel? card =
            cardPlay.Card;

        if (card == null)
            return;

        if (card.Owner != Owner)
            return;

        if (!card.CanonicalKeywords.Contains(
                RikkuEnums.Steal))
        {
            return;
        }

        List<PotionModel> potions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
                .ToList();

        if (potions.Count == 0)
            return;

        PotionModel potion =
            Owner.RunState.Rng.CombatPotionGeneration
                .NextItem(potions)
                .ToMutable();

        PotionProcureResult result =
            await PotionCmd.TryToProcure(
                potion,
                Owner);

        if (!result.success)
            return;

        _triggeredThisCombat = true;

        Flash();
    }
}