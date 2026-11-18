using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Rare;

public class DoubleDown() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            8m,
            ValueProp.Move)
    ];

    private bool HasPlayedStealThisTurn =>
        CombatManager.Instance
            .History
            .CardPlaysFinished
            .Any(entry =>
                entry.HappenedThisTurn(
                    CombatState) &&
                entry.CardPlay.Card.Owner ==
                Owner &&
                entry.CardPlay.Card.Keywords.Contains(
                    RikkuEnums.Steal));

    protected override bool ShouldGlowGoldInternal =>
        HasPlayedStealThisTurn;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        bool gainBlockTwice =
            HasPlayedStealThisTurn;

        await CommonActions.CardBlock(
            this,
            play);

        if (gainBlockTwice)
        {
            await CommonActions.CardBlock(
                this,
                play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(
            3m);
    }
}