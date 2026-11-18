using MegaCrit.Sts2.Core.Entities.Powers;

namespace Rikku.RikkuCode.Powers;

public class SoulSwipePower : RikkuPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;
}