using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Cards.Uncommon;

public class Throw() : RikkuCard(
    1,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const string DamagePerPotion =
        "DamagePerPotion";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(
            DamagePerPotion,
            6m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (play.Target == null)
            return;

        List<PotionModel> potions =
            Owner.Potions.ToList();

        int potionCount =
            potions.Count;

        foreach (PotionModel potion in potions)
        {
            await PotionCmd.Discard(
                potion);
        }

        int damage =
            DynamicVars[DamagePerPotion].IntValue *
            potionCount;

        if (potionCount > 3)
        {
            damage *= 2;
        }

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomFire();

            rikku.PlayAnimation(ownerCreature, "throw_mix");

            await Task.Delay(467);

            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);

            await Task.Delay(33);

            SfxCmd.Play("res://Rikku/sfx/from_killia/fire_impact (2).wav");

            rikku.PlayVfxOnTarget(
                play.Target,
                "res://Rikku/scenes/vfx.tscn",
                "grenade_small");
        }
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(play.Target));
        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithValueProp(ValueProp.Move)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[DamagePerPotion]
            .UpgradeValueBy(
                2m);
    }
}