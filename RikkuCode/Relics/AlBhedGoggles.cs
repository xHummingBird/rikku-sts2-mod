using MegaCrit.Sts2.Core.Entities.Relics;

namespace Rikku.RikkuCode.Relics;

public class AlBhedGoggles : RikkuRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    public const decimal DamageBonus =
        25m;
}