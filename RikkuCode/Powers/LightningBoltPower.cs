using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Powers;

public class LightningBoltPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player)
            return;

        List<Creature> enemies =
            Owner.CombatState?
                .HittableEnemies
                .ToList()
            ?? [];

        if (enemies.Count == 0)
            return;

        Creature target =
            Owner.Player
                .RunState
                .Rng
                .CombatCardSelection
                .NextItem(
                    enemies);

        Flash();

        if (Owner.Player.Character is Character.Rikku rikku)
        {
            rikku.PlayVfxOnTarget(
                target,
                "res://Rikku/scenes/vfx.tscn",
                "lightning_bolt");
        }

        SfxCmd.Play(
            "PUT_LIGHTNING_BOLT_SFX_HERE");

        await Task.Delay(
            100);

        await CreatureCmd.Damage(
            choiceContext,
            target,
            8m,
            ValueProp.Unpowered,
            Owner);

        await PowerCmd.Decrement(
            this);
    }
}