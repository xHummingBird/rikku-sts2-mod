using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;
using Godot;

namespace Rikku.RikkuCode.Relics;

[Pool(typeof(RikkuRelicPool))]
public abstract class RikkuRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".BigRelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}