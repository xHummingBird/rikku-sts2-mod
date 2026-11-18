using MegaCrit.Sts2.Core.Entities.Powers;

namespace Rikku.RikkuCode.Powers;

public class DaylightRobberyPower : RikkuPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}