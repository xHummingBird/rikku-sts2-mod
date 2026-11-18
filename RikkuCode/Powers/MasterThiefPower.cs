using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Rikku.RikkuCode.Powers;

public class MasterThiefPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner.Player)
            return false;

        if (room == null)
            return false;

        if (!room.RoomType.IsCombatRoom())
            return false;

        for (int i = 0;
             i < Amount;
             i++)
        {
            rewards.Add(
                new PotionReward(
                    player));
        }

        return true;
    }
}