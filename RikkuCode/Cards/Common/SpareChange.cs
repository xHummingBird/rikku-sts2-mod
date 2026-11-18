using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Common;

public class SpareChange() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    private const string GoldCost =
        "GoldCost";
    
    protected override bool IsPlayable => Owner.Gold >= DynamicVars[GoldCost].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            13m,
            ValueProp.Move),

        new DynamicVar(
            GoldCost,
            15m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        Owner.Gold -=
            DynamicVars[GoldCost].IntValue;

        var ownerCreature =
            Owner?.Creature;

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
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            2m);

        DynamicVars[GoldCost].UpgradeValueBy(
            -5m);
    }
}