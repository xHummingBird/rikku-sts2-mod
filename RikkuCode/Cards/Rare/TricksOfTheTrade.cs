using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Cards.Rare;

public class TricksOfTheTrade() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    private bool _procuredThisCombat;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new HitCountVar(2)
    ];

    public override Task BeforeCombatStart()
    {
        _procuredThisCombat = false;

        return Task.CompletedTask;
    }

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
                    play.Target,
                    hitCount: 2)
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
            await CommonActions.CardAttack(this, play.Target, hitCount: DynamicVars.HitCount.IntValue)
                .WithHitFx(
                    "vfx/vfx_attack_slash",
                    "res://Rikku/sfx/rikku_hit.wav")
                .Execute(
                    choiceContext);
        }

        if (_procuredThisCombat)
            return;

        int potionAmount =
            Owner.Potions.Count() == 0
                ? 2
                : 1;

        bool procuredAnyPotion =
            false;

        for (int i = 0; i < potionAmount; i++)
        {
            List<PotionModel> potionOptions =
                ModelDb.PotionPool<RikkuPotionPool>()
                    .GetUnlockedPotions(
                        Owner.UnlockState)
                    .ToList();

            if (potionOptions.Count == 0)
                break;

            PotionModel potion =
                Owner.RunState
                    .Rng
                    .CombatPotionGeneration
                    .NextItem(
                        potionOptions)
                    .ToMutable();

            PotionProcureResult result =
                await PotionCmd.TryToProcure(
                    potion,
                    Owner);

            if (!result.success)
                break;

            procuredAnyPotion = true;
        }

        if (procuredAnyPotion)
        {
            _procuredThisCombat = true;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            2m);
    }
}