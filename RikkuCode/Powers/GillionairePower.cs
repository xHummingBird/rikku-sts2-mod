using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Rikku.RikkuCode.Powers;

public class GillionairePower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override Task AfterCombatEnd(
        CombatRoom room)
    {
        room.AddExtraReward(
            Owner.Player,
            new GoldReward(
                Amount,
                Owner.Player));

        return Task.CompletedTask;
    }
}