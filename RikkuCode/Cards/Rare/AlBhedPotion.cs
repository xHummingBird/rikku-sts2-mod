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

public class AlBhedPotion() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(
            4m)
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

        decimal healAmount =
            DynamicVars.Heal.BaseValue;

        if (discardedPotion != null)
        {
            healAmount *=
                2m;
        }

        await CreatureCmd.Heal(
            Owner.Creature,
            healAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(
            1m);
    }
}