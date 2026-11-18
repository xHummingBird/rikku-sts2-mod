using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Rikku.RikkuCode.Relics;

public class ItemPouch : RikkuRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Common;

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainMaxPotionCount(
            2,
            Owner);
    }
}