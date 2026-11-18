using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Common;

public class Reversal() : RikkuCard(
    0,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    private const string HpThreshold =
        "HpThreshold";

    private const string AdditionalHits =
        "AdditionalHits";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            5m,
            ValueProp.Move),

        new DynamicVar(
            HpThreshold,
            50m),

        new DynamicVar(
            AdditionalHits,
            2m)
    ];

    private bool IsReversalActive
    {
        get
        {
            var ownerCreature =
                Owner?.Creature;

            if (ownerCreature == null)
                return false;

            return ownerCreature.CurrentHp * 100 <=
                ownerCreature.MaxHp *
                DynamicVars[HpThreshold].IntValue;
        }
    }

    protected override bool ShouldGlowGoldInternal =>
        IsReversalActive;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

        var ownerCreature =
            Owner?.Creature;

        int hitCount =
            IsReversalActive
                ? 1 +
                  DynamicVars[AdditionalHits].IntValue
                : 1;

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
                    play.Target,
                    hitCount: hitCount)
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
                    hitCount: hitCount)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars[HpThreshold].UpgradeValueBy(
            16m);
    }
}