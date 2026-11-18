using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Rikku.RikkuCode.Relics;

public class ThiefsGloves : RikkuRelic
{
    private const string GoldIncrease =
        "GoldIncrease";

    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(
            GoldIncrease,
            1.5m)
    ];

    public override decimal ModifyGoldGained(
        Player player,
        decimal amount)
    {
        if (player != Owner)
            return amount;

        return amount *
               DynamicVars[GoldIncrease].BaseValue;
    }

    public override Task AfterModifyingGoldGained(
        Player player,
        decimal amount)
    {
        if (player == Owner)
        {
            Flash();
        }

        return Task.CompletedTask;
    }
}