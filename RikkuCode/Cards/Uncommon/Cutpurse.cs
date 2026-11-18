using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class Cutpurse() : RikkuCard(
    0,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const string BonusRepeats =
        "BonusRepeats";

    protected override bool HasEnergyCostX =>
        true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            6m,
            ValueProp.Move),

        new DynamicVar(
            BonusRepeats,
            0m)
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

        int repeatCount =
            ResolveEnergyXValue() +
            DynamicVars[BonusRepeats].IntValue;

        if (repeatCount <= 0)
            return;

        var ownerCreature =
            Owner?.Creature;
        
        for (int i = 0; i < repeatCount; i++)
        {
            int amountToSteal =
                StealHelper.GetStealAmount(
                    Owner.Creature,
                    play.Target);

            if (amountToSteal <= 0)
                continue;

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
        
        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomSteal();

            await rikku.DashTo(
                ownerCreature,
                play.Target,
                distance: 400f,
                durationSeconds: 0.15f);

            rikku.PlayAnimation(
                ownerCreature,
                "attack");

            SfxCmd.Play(
                "res://Rikku/sfx/from_tifa/punch_swing_1.wav");

            await Task.Delay(
                50);

            await CommonActions.CardAttack(
                    this,
                    play.Target,
                    hitCount: repeatCount)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);

            await rikku.Retreat(
                ownerCreature,
                duration: 0.2f);
        }
        else
        {
            await CommonActions.CardAttack(
                    this,
                    play.Target,
                    hitCount: repeatCount)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[BonusRepeats]
            .UpgradeValueBy(
                1m);
    }
}