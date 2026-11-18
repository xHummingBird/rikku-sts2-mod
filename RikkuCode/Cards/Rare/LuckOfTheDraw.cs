using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Cards.Rare;

public class LuckOfTheDraw() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CardModel? drawnCard =
            (await CardPileCmd.Draw(
                choiceContext,
                DynamicVars.Cards.BaseValue,
                Owner))
            .FirstOrDefault();

        if (drawnCard == null ||
            drawnCard.Type != CardType.Skill)
        {
            return;
        }

        List<PotionModel> potionOptions =
            ModelDb.PotionPool<RikkuPotionPool>()
                .GetUnlockedPotions(
                    Owner.UnlockState)
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

        await PotionCmd.TryToProcure(
            potion,
            Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(
            -1);
    }
}