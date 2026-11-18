using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Rare;

public class StrengthDrain() : RikkuCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthDrainGainPower>(
            3m)
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

        int amountToSteal =
            StealHelper.GetStealAmount(
                Owner.Creature,
                play.Target);
        
        AudioHelper.PlayRandomSteal();
        
        if (amountToSteal > 0)
        {
            Owner.Gold +=
                amountToSteal;
            
            await PowerCmd.Apply<StolenGoldPower>(
                choiceContext,
                play.Target,
                amountToSteal,
                Owner.Creature,
                this,
                false);
        }

        decimal strength =
            DynamicVars["StrengthDrainGainPower"].BaseValue;

        await PowerCmd.Apply<StrengthDrainLossPower>(
            choiceContext,
            play.Target,
            strength,
            Owner.Creature,
            this);

        await PowerCmd.Apply<StrengthDrainGainPower>(
            choiceContext,
            Owner.Creature,
            strength,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthDrainGainPower"].UpgradeValueBy(1m);
    }
}