using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class SneakAttack() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private bool _sneakAttackDiscountApplied;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            8m,
            ValueProp.Move)
    ];

    private bool HasPlayedCardThisTurn =>
        CombatManager.Instance
            .History
            .CardPlaysFinished
            .Any(entry =>
                entry.HappenedThisTurn(
                    CombatState) &&
                entry.CardPlay.Player ==
                    Owner);

    protected override bool ShouldGlowGoldInternal =>
        !HasPlayedCardThisTurn;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

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

    public override Task AfterCardEnteredCombat(
        CardModel card)
    {
        if (card != this)
            return Task.CompletedTask;

        if (HasPlayedCardThisTurn)
        {
            RemoveSneakAttackDiscount();
        }
        else
        {
            ApplySneakAttackDiscount();
        }

        return Task.CompletedTask;
    }

    public override Task AfterEnergyReset(
        Player player)
    {
        if (player != Owner)
            return Task.CompletedTask;

        /*
         * AddThisTurn modifiers are reset naturally,
         * so forget the previous turn's tracked discount.
         */
        _sneakAttackDiscountApplied =
            false;

        ApplySneakAttackDiscount();

        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        RemoveSneakAttackDiscount();

        return Task.CompletedTask;
    }

    private void ApplySneakAttackDiscount()
    {
        if (_sneakAttackDiscountApplied)
            return;

        EnergyCost.AddThisTurn(
            -1);

        _sneakAttackDiscountApplied =
            true;
    }

    private void RemoveSneakAttackDiscount()
    {
        if (!_sneakAttackDiscountApplied)
            return;

        EnergyCost.AddThisTurn(
            1);

        _sneakAttackDiscountApplied =
            false;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            2m);
    }
}