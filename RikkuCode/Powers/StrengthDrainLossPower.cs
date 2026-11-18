using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Rikku.RikkuCode.Cards.Rare;
using Rikku.RikkuCode.Extensions;

namespace Rikku.RikkuCode.Powers;

public class StrengthDrainLossPower :
    TemporaryStrengthPower,
    ICustomPower
{
    public override AbstractModel OriginModel =>
        ModelDb.Card<StrengthDrain>();

    protected override bool IsPositive =>
        false;

    public string? CustomPackedIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png"
            .BigPowerImagePath();

    public string? CustomBigIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png"
            .BigPowerImagePath();

    public string? CustomBigBetaIconPath =>
        null;
}