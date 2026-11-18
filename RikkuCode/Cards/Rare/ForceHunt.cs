using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Rare;

public class ForceHunt() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    private const string StealPercent =
        "StealPercent";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            8m,
            ValueProp.Move),

        new DynamicVar(
            StealPercent,
            50m)
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

        var ownerCreature =
            Owner?.Creature;

        StolenGoldPower? stolenGoldPower =
            play.Target.GetPower<StolenGoldPower>();

        int remainingGold =
            stolenGoldPower?.RemainingGold
            ?? StealHelper.GetMaxStealableGold(
                Owner.Creature,
                play.Target);

        int amountToSteal =
            remainingGold *
            DynamicVars[StealPercent].IntValue /
            100;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomAttack();

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

            await Task.Delay(50);

            await CommonActions.CardAttack(
                    this,
                    play.Target)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);

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

            await rikku.Retreat(
                ownerCreature,
                duration: 0.2f);
        }
        else
        {
            await CommonActions.CardAttack(
                    this,
                    play.Target)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);

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
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            3m);

        DynamicVars[StealPercent].UpgradeValueBy(
            16m);
    }
}