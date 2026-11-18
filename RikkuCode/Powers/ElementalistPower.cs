using MegaCrit.Sts2.Core.Entities.Powers;

namespace Rikku.RikkuCode.Powers;

public class ElementalistPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public decimal ModifyPotionDamage(
        decimal damage)
    {
        return damage *
               (1m + Amount / 100m);
    }
}