using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Mechanics;
using Rikku.RikkuCode.Potions;
using Rikku.RikkuCode.Powers;
using Tidus.TidusCode.Extensions;

namespace Rikku.RikkuCode.Cards.Ancient;

public class Mix() : RikkuCard(
    0,
    CardType.Skill,
    CardRarity.Ancient,
    TargetType.AllEnemies)
{
    private const string Damage1 = "Damage1";
    private const string Damage1Level2 = "Damage1Level2";
    private const string Damage2 = "Damage2";
    private const string Damage2Level2 = "Damage2Level2";
    private const string Damage3 = "Damage3";
    private const string Damage3Level2 = "Damage3Level2";
    private const string DamageLevel2 = "DamageLevel2";
    private const string BlockLevel2 = "BlockLevel2";
    
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(28m, ValueProp.Move),
        new DynamicVar(DamageLevel2, 38m),
        new BlockVar(12m, ValueProp.Move),
        new DynamicVar(Damage1, 40m),
        new DynamicVar(Damage1Level2, 51m),
        new DynamicVar(Damage2, 35),
        new DynamicVar(Damage2Level2, 45m),
        new DynamicVar(Damage3, 38),
        new DynamicVar(Damage3Level2, 48m),
        new DynamicVar(BlockLevel2, 16m),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ingredients =
        (
            Primary: (PotionModel?)null,
            Secondary: (PotionModel?)null
        );

        if (Owner.Potions.Count() >= 2)
            ingredients =
                await SynchronizedPotionChoice.SelectMix(
                    true,
                    this,
                    choiceContext,
                    RikkuPotionMechanic.SelectMixIngredients);
        

        OverdriveManager.SetOverdrive(Owner, 0);
        await PowerCmd.Remove<OverdriveReadyPower>(Owner.Creature);
        if (ingredients.Primary == null || ingredients.Secondary == null)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayHazardousShell(choiceContext, play, isLevelTwo: false);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }
        RikkuPotionMechanic.ConsumeMixIngredients(ingredients.Primary, ingredients.Secondary);
        
        GD.Print($"Primary: {ingredients.Primary.Id.Entry}");

        GD.Print($"Secondary: {ingredients.Secondary.Id.Entry}");
        
        bool isLevelTwo = ingredients.Primary.Rarity ==
                          PotionRarity.Uncommon ||
                          ingredients.Primary.Id ==
                          ingredients.Secondary.Id;

        if (ingredients.Primary is FragGrenade or SupremeGem)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayChaosGrenade(choiceContext, play, isLevelTwo);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }

        if (ingredients.Primary is BombFragment or FireGem)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayBurningSoul(choiceContext, play, isLevelTwo);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }
        
        if (ingredients.Primary is IceGem or AntarcticWind)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayWinterStorm(choiceContext, play, isLevelTwo);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }

        if (ingredients.Primary is LightningGem or ElectroMarble)
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayLightningBolt(choiceContext, play, isLevelTwo);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }
        
        if (ingredients.Primary is WaterGem or FishScale)
            
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await PlayTidalWave(choiceContext, play, isLevelTwo);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }

        if (ingredients.Primary is StarCurtain or LightCurtain)
        {
            await PlayMightyWall(choiceContext, play, isLevelTwo);
            return;
        }
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        await PlayHazardousShell(choiceContext, play, isLevelTwo: true);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    private async Task PlayBurningSoul(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        decimal damage = DynamicVars[isLevelTwo ? Damage1Level2 : Damage1].BaseValue;

        decimal vulnerable = isLevelTwo ? 3m : 2m;

        List<Creature> targets = CombatState.HittableEnemies.ToList();

        if (targets.Count == 0)
            return;

        Creature middleEnemy = targets[(targets.Count - 1) / 2];

        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomFire();
            SfxCmd.Play("res://Rikku/sfx/mix_cast.mp3", 1.1f);
            rikku.PlayAnimation(ownerCreature, "throw_mix");
            await Task.Delay(467);
            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);
            await Task.Delay(33);
            rikku.PlayVfxOnTarget(
                middleEnemy,
                "res://Rikku/scenes/vfx.tscn",
                "burning_soul_2");
        }

        await DamageCmd.Attack(damage)
            .WithHitFx(null, "res://Rikku/sfx/from_tifa/meteostrike_1.wav")
            .FromCard(this, play)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .WithValueProp(ValueProp.Move)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        foreach (Creature enemy in targets)
        {
            if (enemy.IsDead)
                continue;
            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                enemy,
                vulnerable,
                Owner.Creature,
                this);
        }
    }
    
    private async Task PlayHazardousShell(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        decimal damage = isLevelTwo
                ? DynamicVars[DamageLevel2].BaseValue
                : DynamicVars.Damage.BaseValue;

        List<Creature> targets = CombatState.HittableEnemies.ToList();

        if (targets.Count == 0)
            return;

        Creature middleEnemy = targets[(targets.Count - 1) / 2];

        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomMix();

            SfxCmd.Play("res://Rikku/sfx/mix_cast.mp3", 1.1f);

            rikku.PlayAnimation(ownerCreature, "throw_mix");

            await Task.Delay(467);

            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);

            await Task.Delay(33);

            rikku.PlayVfxOnTarget(
                middleEnemy,
                "res://Rikku/scenes/vfx.tscn",
                "burning_soul");
        }

        await DamageCmd.Attack(damage)
            .WithHitFx(null, "res://Rikku/sfx/from_killia/fire_impact (1).wav")
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithValueProp(ValueProp.Move)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Red);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
    }
    
    private async Task PlayWinterStorm(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        decimal damage = DynamicVars[isLevelTwo ? Damage2Level2 : Damage2].BaseValue;

        decimal block =
            isLevelTwo
                ? DynamicVars[BlockLevel2].BaseValue
                : DynamicVars.Block.BaseValue;

        List<Creature> targets =
            CombatState.HittableEnemies
                .ToList();

        if (targets.Count == 0)
            return;

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomIce();

            SfxCmd.Play("res://Rikku/sfx/mix_cast.mp3", 1.1f);

            rikku.PlayAnimation(ownerCreature, "throw_mix");
            
            await Task.Delay(467);
            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);
            
            foreach (Creature enemy in targets)
            {
                rikku.PlayVfxOnTarget(enemy, "res://Rikku/scenes/vfx.tscn", "winter_storm");
            }
            
            SfxCmd.Play("res://Rikku/sfx/ice.wav");
            
            await Task.Delay(467);
        }

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Rikku/sfx/ice_2.wav")
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Blue);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);

        var finalBlock = await CreatureCmd.GainBlock(
            Owner.Creature,
            block,
            ValueProp.Move,
            play);

        await PowerCmd.Apply<BlockNextTurnPower>(
            choiceContext,
            Owner.Creature,
            finalBlock,
            Owner.Creature,
            this);
    }
    
    private async Task PlayTidalWave(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        decimal damage = DynamicVars[isLevelTwo ? Damage1Level2 : Damage1].BaseValue;
        decimal weak = isLevelTwo ? 3m : 2m;
        List<Creature> targets =
            CombatState.HittableEnemies
                .ToList();

        if (targets.Count == 0)
            return;

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomWater();

            SfxCmd.Play("res://Rikku/sfx/mix_cast.mp3", 1.1f);

            rikku.PlayAnimation(ownerCreature, "throw_mix");
            
            await Task.Delay(467);
            SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);
            
            foreach (Creature enemy in targets)
            {
                rikku.PlayVfxOnTarget(enemy, "res://Rikku/scenes/vfx.tscn", "tidal_wave");
            }
            
            SfxCmd.Play("res://Rikku/sfx/water_2.wav");
            
            await Task.Delay(33);
            SfxCmd.Play("res://Rikku/sfx/water.mp3");
            await Task.Delay(400);
        }

        await DamageCmd.Attack(damage)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState)
            .WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Rikku/sfx/water_2.wav")
            .Execute(choiceContext);

        foreach (Creature enemy in targets)
        {
            if (enemy.IsDead)
                continue;
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                enemy,
                weak,
                Owner.Creature,
                this);
        }
    }
    
    
    
    private async Task PlayLightningBolt(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        int hitCount =
            isLevelTwo
                ? 5
                : 4;

        var ownerCreature =
            Owner?.Creature;

        Character.Rikku? rikku =
            Owner?.Character as Character.Rikku;

        if (ownerCreature != null &&
            rikku != null)
        {
            AudioHelper.PlayRandomThunder();

            SfxCmd.Play(
                "res://Rikku/sfx/mix_cast.mp3",
                1.1f);

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
        }

        await DamageCmd.Attack(11m)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState)
            .WithValueProp(ValueProp.Move)
            .WithNoAttackerAnim()
            .WithWaitBeforeHit(0.1f, 0.1f)
            .WithHitVfxNode(target =>
            {
                rikku?.PlayVfxOnTarget(
                    target,
                    "res://Rikku/scenes/vfx.tscn",
                    "lightning_bolt");

                SfxCmd.Play(
                    "res://Rikku/sfx/from_killia/charged_thunder (2).wav");

                return null;
            })
            .Execute(
                choiceContext);

        await PowerCmd.Apply<LightningBoltPower>(
            choiceContext,
            Owner.Creature,
            isLevelTwo
                ? 3m
                : 2m,
            Owner.Creature,
            this);
    }
    
    private async Task PlayMightyWall(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        bool isLevelTwo)
    {
        decimal intangible =
            isLevelTwo
                ? 2m
                : 1m;

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Rikku rikku)
        {
            AudioHelper.PlayRandomMix();

            SfxCmd.Play(
                "res://Rikku/sfx/mix_cast.mp3",
                1.1f);

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
        }

        IEnumerable<Creature> players =
            CombatState
                .GetTeammatesOf(
                    Owner.Creature)
                .Where(creature =>
                    creature != null &&
                    creature.IsAlive &&
                    creature.IsPlayer);

        foreach (Creature player in players)
        {
            await PowerCmd.Apply<IntangiblePower>(
                choiceContext,
                player,
                intangible,
                Owner.Creature,
                this);
        }
    }
    
    private async Task PlayChaosGrenade(
    PlayerChoiceContext choiceContext,
    CardPlay play,
    bool isLevelTwo)
{
    decimal damage =
        DynamicVars[
            isLevelTwo
                ? Damage3Level2
                : Damage3]
            .BaseValue;

    List<Creature> targets =
        CombatState.HittableEnemies
            .ToList();

    if (targets.Count == 0)
        return;

    if (targets.Count == 1)
    {
        damage *= 1.5m;
    }

    Creature middleEnemy =
        targets[
            (targets.Count - 1) / 2];

    var ownerCreature =
        Owner?.Creature;

    if (ownerCreature != null &&
        Owner?.Character is Character.Rikku rikku)
    {
        AudioHelper.PlayRandomMix();

        SfxCmd.Play(
            "res://Rikku/sfx/mix_cast.mp3",
            1.1f);

        rikku.PlayAnimation(
            ownerCreature,
            "throw_mix");

        await Task.Delay(467);

        SfxCmd.Play("res://Rikku/sfx/from_tifa/punch_swing_3.wav", 0.8f);

        await Task.Delay(33);
        
        rikku.PlayVfxOnTarget(middleEnemy,
            "res://Rikku/scenes/vfx.tscn",
            "chaos_grenade");
        
        SfxCmd.Play(
            "res://Rikku/sfx/from_killia/fire_charge.wav");
        
        await Task.Delay(467);

        SfxCmd.Play(
            "res://Rikku/sfx/from_killia/charge_thunder (1).wav");

        SfxCmd.Play("res://Rikku/sfx/from_tifa/chi_sfx.wav");

        
        await Task.Delay(733);

        SfxCmd.Play(
            "res://Rikku/sfx/from_killia/charge_1.wav");

        // Hit: 1.533s
        // 1533 - 1200 = 333ms
        await Task.Delay(
            333);
    }

    await DamageCmd.Attack(damage)
        .WithHitFx(null, "res://Rikku/sfx/from_tifa/explode.wav")
        .FromCard(this, play)
        .WithValueProp(ValueProp.Move)
        .TargetingAllOpponents(CombatState)
        .Execute(choiceContext);
}
}