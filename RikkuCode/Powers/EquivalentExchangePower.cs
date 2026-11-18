using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Rikku.RikkuCode.Powers;

public class EquivalentExchangePower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

    public override async Task AfterCombatEnd(
        CombatRoom room)
    {
        if (Owner.Player == null)
            return;

        List<PotionModel> potions =
            Owner.Player
                .Potions
                .ToList();

        foreach (PotionModel potion in potions)
        {
            await PotionCmd.Discard(
                potion);
        }
    }
}