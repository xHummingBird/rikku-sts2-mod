using MegaCrit.Sts2.Core.Entities.Relics;
using Rikku.RikkuCode.Mechanics;

namespace Rikku.RikkuCode.Relics;

public class Survivor : RikkuRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Uncommon;

    public override Task BeforeCombatStartLate()
    {
        Flash();

        OverdriveManager.GainOverdrive(
            Owner,
            20);

        return Task.CompletedTask;
    }
}