using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rikku.RikkuCode.Relics;

public class ThiefBandana : RikkuRelic
{
    private bool _tookDamageThisCombat;

    public override RelicRarity Rarity =>
        RelicRarity.Shop;

    [SavedProperty]
    public bool TookDamageThisCombat
    {
        get =>
            _tookDamageThisCombat;

        set
        {
            AssertMutable();

            _tookDamageThisCombat =
                value;
        }
    }

    public override Task AfterRoomEntered(
        AbstractRoom room)
    {
        TookDamageThisCombat =
            false;

        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner.RunState.CurrentRoom
            is not CombatRoom)
        {
            return Task.CompletedTask;
        }

        if (target != Owner.Creature)
            return Task.CompletedTask;

        if (result.UnblockedDamage <= 0)
            return Task.CompletedTask;

        if (props.HasFlag(
                ValueProp.Unblockable))
        {
            return Task.CompletedTask;
        }

        TookDamageThisCombat =
            true;

        return Task.CompletedTask;
    }

    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner)
            return false;

        if (room == null)
            return false;

        if (!room.RoomType.IsCombatRoom())
            return false;

        if (TookDamageThisCombat)
            return false;

        Flash();

        rewards.Add(
            new PotionReward(
                player));

        return true;
    }
}