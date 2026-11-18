using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Rare;

public class UnstableConcoction() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    private const string MinimumDamage =
        "MinimumDamage";
    
    protected override bool ShouldGlowGoldInternal => Owner.Potions.Count() >= 4;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            24m,
            ValueProp.Move),

        new DynamicVar(
            MinimumDamage,
            1m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int maximumDamage =
            DynamicVars.Damage.IntValue;

        int finalDamage =
            Owner.Potions.Count() >= 4
                ? maximumDamage
                : RollRandomDamage();

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomAttack();

            rikku.PlayAnimation(
                ownerCreature,
                "throw_mix");

            await Task.Delay(467);

            SfxCmd.Play(
                "res://Rikku/sfx/from_tifa/punch_swing_3.wav",
                0.8f);

            await Task.Delay(33);

            SfxCmd.Play(
                "res://Rikku/sfx/from_killia/fire_impact (2).wav");

            rikku.PlayVfxOnTarget(
                play.Target,
                "res://Rikku/scenes/vfx.tscn",
                "grenade_small");
        }
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(play.Target, VfxColor.Green));
        await DamageCmd.Attack(finalDamage).FromCard(this, play)
            .WithValueProp(ValueProp.Move)
            .Targeting(play.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            6m);
    }

    private int RollRandomDamage()
    {
        int minimumDamage =
            DynamicVars[MinimumDamage].IntValue;

        int maximumDamage =
            DynamicVars.Damage.IntValue;

        return Owner.RunState
            .Rng
            .CombatCardSelection
            .NextInt(
                minimumDamage,
                maximumDamage + 1);
    }
}