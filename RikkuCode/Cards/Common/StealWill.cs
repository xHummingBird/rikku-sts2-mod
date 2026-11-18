using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Common;

public class StealWill() : RikkuCard(
    0,
    CardType.Skill,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(
            1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RikkuEnums.Steal
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

        int amountToSteal =
            StealHelper.GetStealAmount(
                Owner.Creature,
                play.Target);

        if (amountToSteal > 0)
        {
            Owner.Gold +=
                amountToSteal;

            AudioHelper.PlayRandomSteal();

            await PowerCmd.Apply<StolenGoldPower>(
                choiceContext,
                play.Target,
                amountToSteal,
                Owner.Creature,
                this,
                false);
        }

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            play.Target,
            DynamicVars.Vulnerable.BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Vulnerable.UpgradeValueBy(
            1m);
    }
}