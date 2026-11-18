using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Common;

public class PilferGil() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HitCountVar(
            2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        RikkuEnums.Steal
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

        bool stoleAnyGold =
            false;

        for (int i = 0;
             i < DynamicVars.HitCount.IntValue;
             i++)
        {
            int amountToSteal =
                StealHelper.GetStealAmount(
                    Owner.Creature,
                    play.Target);

            if (amountToSteal <= 0)
                continue;

            Owner.Gold +=
                amountToSteal;

            stoleAnyGold =
                true;

            await PowerCmd.Apply<StolenGoldPower>(
                choiceContext,
                play.Target,
                amountToSteal,
                Owner.Creature,
                this,
                false);
        }

        if (stoleAnyGold)
        {
            AudioHelper.PlayRandomSteal();
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(
            -1);
    }
}