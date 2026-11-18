using BaseLib.Abstracts;
using BaseLib.Extensions;
using Rikku.RikkuCode.Extensions;
using Godot;

namespace Rikku.RikkuCode.Powers;

public abstract class RikkuPower : CustomPowerModel
{
    //Loads from Rikku/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}