using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Ancient;

public class HandOfMidas() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Ancient,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<StashPower>(
            choiceContext,
            Owner.Creature,
            3,
            Owner.Creature,
            this,
            false);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}