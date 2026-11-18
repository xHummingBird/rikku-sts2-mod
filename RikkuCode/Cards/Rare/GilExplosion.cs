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

public class GilExplosion() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AllEnemies)
{
    private const string GoldPercent =
        "GoldPercent";

    private const string Multiplier =
        "Multiplier";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            1m,
            ValueProp.Move),

        new DynamicVar(
            GoldPercent,
            10m),

        new DynamicVar(
            Multiplier,
            100m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        int goldSpent =
            (int)Math.Floor(
                Owner.Gold *
                DynamicVars[GoldPercent].BaseValue /
                100m);

        Owner.Gold -=
            goldSpent;

        decimal damage =
            goldSpent *
            DynamicVars[Multiplier].BaseValue /
            100m;

        var ownerCreature =
            Owner.Creature;

        if (Owner.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomFire();

            rikku.PlayAnimation(
                ownerCreature,
                "throw_mix");

            await Task.Delay(
                467);

            SfxCmd.Play(
                "res://Rikku/sfx/from_tifa/punch_swing_3.wav",
                0.8f);

            await Task.Delay(
                33);

            SfxCmd.Play(
                "res://Rikku/sfx/from_killia/fire_impact (2).wav");

            foreach (var enemy in CombatState.HittableEnemies)
            {
                rikku.PlayVfxOnTarget(
                    enemy,
                    "res://Rikku/scenes/vfx.tscn",
                    "grenade_small");
            }
        }

        await DamageCmd.Attack(
                damage)
            .FromCard(
                this,
                play)
            .WithValueProp(
                ValueProp.Move)
            .TargetingAllOpponents(
                CombatState)
            .BeforeDamage(async delegate
            {
                var targets =
                    CombatState.HittableEnemies;

                foreach (var target in targets)
                {
                    var vfx =
                        NGroundFireVfx.Create(
                            target);

                    if (vfx != null)
                    {
                        NCombatRoom.Instance
                            .CombatVfxContainer
                            .AddChildSafely(
                                vfx);
                    }
                }
            })
            .Execute(
                choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[Multiplier].UpgradeValueBy(
            30m);
    }
}