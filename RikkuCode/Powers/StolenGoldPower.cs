using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Relics;

namespace Rikku.RikkuCode.Powers;

public class StolenGoldPower : RikkuPower
{
    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(RikkuEnums.Steal)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StolenGold", Amount)
    ];

    public bool HasGodhand =>
        Owner.CombatState != null &&
        Owner.CombatState.Players.Any(player =>
            player.Relics.Any(relic =>
                relic is Godhand));

    public int MaxStealableGold
    {
        get
        {
            int threshold =
                HasGodhand
                    ? 40
                    : 30;

            return Owner.MaxHp * threshold / 100;
        }
    }

    public int RemainingGold =>
        Math.Max(
            0,
            MaxStealableGold - (int)Amount);

    public int GetStealAmount(
        int requestedAmount)
    {
        return Math.Min(
            requestedAmount,
            RemainingGold);
    }
}