using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using Rikku.RikkuCode.Character;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Powers;
using Rikku.RikkuCode.Relics;

namespace Rikku.RikkuCode.Potions;

[Pool(typeof(RikkuPotionPool))]
public abstract class RikkuPotion : CustomPotionModel

{
    protected string PotionFileName =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    protected string PotionOutlineFileName =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png";

    public override string? CustomPackedImagePath =>
        PotionFileName.PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        PotionOutlineFileName.PotionImagePath();
    
    public static decimal GetMixablePotionDamage(
        Player owner,
        decimal baseDamage)
    {
        decimal damageBonus =
            owner.Creature
                .GetPowerAmount<ElementalistPower>();

        if (owner.Relics.Any(relic =>
                relic is AlBhedGoggles))
        {
            damageBonus +=
                AlBhedGoggles.DamageBonus;
        }

        return baseDamage *
               (1m + damageBonus / 100m);
    }
}

