using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class GoldenOpportunity() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FreeSkillPower>(
            1m),

        new PowerVar<FreeAttackPower>(
            1m)
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
        await PowerCmd.Apply<FreeSkillPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars
                .Power<FreeSkillPower>()
                .BaseValue,
            Owner.Creature,
            this);

        if (!HasPlayedStealThisTurn)
            return;

        await PowerCmd.Apply<FreeAttackPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars
                .Power<FreeAttackPower>()
                .BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}