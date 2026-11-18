using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Cards.Common;

public class MercyStroke() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    private const string GoldScaling =
        "GoldScaling";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            8m,
            ValueProp.Move),

        new DynamicVar(
            GoldScaling,
            40m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =
            Owner?.Creature;

        int stolenGold = play.Target.GetPowerAmount<StolenGoldPower>();

        int goldScaling =
            DynamicVars[GoldScaling].IntValue;

        int bonusDamage =
            (int)Math.Floor(
                stolenGold *
                goldScaling /
                100m);

        int finalDamage =
            DynamicVars.Damage.IntValue +
            bonusDamage;

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

            await DamageCmd.Attack(
                    finalDamage)
                .FromCard(
                    this,
                    play)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .WithValueProp(
                    ValueProp.Move)
                .Targeting(
                    play.Target)
                .Execute(
                    choiceContext);

            await rikku.Retreat(
                ownerCreature,
                duration: 0.2f);
        }
        else
        {
            await DamageCmd.Attack(
                    finalDamage)
                .FromCard(
                    this,
                    play)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .WithValueProp(
                    ValueProp.Move)
                .Targeting(
                    play.Target)
                .Execute(
                    choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            2m);

        DynamicVars[GoldScaling].UpgradeValueBy(
            10m);
    }
}