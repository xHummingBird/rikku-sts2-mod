using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Cards.Rare;

public class UltraPotion() : RikkuCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    private const string Energy =
        "Energy";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),

        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RikkuStaticHoverTips.DiscardPotion
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        PotionModel? discardedPotion =
            null;

        if (Owner.Potions.Any())
        {
            discardedPotion =
                await SynchronizedPotionChoice.SelectAndDiscard(
                    true,
                    this,
                    choiceContext,
                    RikkuPotionMechanic.SelectDiscardPotionLocally);
        }

        int energy =
            DynamicVars.Energy.IntValue;

        if (discardedPotion != null)
        {
            energy += 2;
        }

        await PlayerCmd.GainEnergy(energy, Owner);

        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars.Cards.BaseValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}