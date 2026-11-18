using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Rikku.RikkuCode.Extensions;

public static class RikkuStaticHoverTips
{
    private static string PotionIcon(
        string potionName)
    {
        return
            $"[img]res://Rikku/images/charui/{potionName}.png[/img]";
    }

    private static IHoverTip CreateMixHoverTip(
        string locKey,
        params (string Key, string FileName)[] icons)
    {
        LocString description =
            new LocString(
                "static_hover_tips",
                $"{locKey}.description");

        foreach (var icon in icons)
        {
            description.Add(
                icon.Key,
                PotionIcon(
                    icon.FileName));
        }

        return new HoverTip(
            new LocString(
                "static_hover_tips",
                $"{locKey}.title"),
            description);
    }

    public static readonly IHoverTip Overdrive =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RIKKU_OVERDRIVE.title"),
            new LocString(
                "static_hover_tips",
                "RIKKU_OVERDRIVE.description"));

    public static readonly IHoverTip Overkill =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RIKKU_OVERKILL.title"),
            new LocString(
                "static_hover_tips",
                "RIKKU_OVERKILL.description"));

    public static readonly IHoverTip Mix =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RIKKU_MIX.title"),
            new LocString(
                "static_hover_tips",
                "RIKKU_MIX.description"));

    public static readonly IHoverTip DiscardPotion =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RIKKU_DISCARD_POTION.title"),
            new LocString(
                "static_hover_tips",
                "RIKKU_DISCARD_POTION.description"));

    public static readonly IHoverTip BurningSoul =
        CreateMixHoverTip(
            "RIKKU_BURNING_SOUL",
            (
                "BombFragmentIcon",
                "bomb_fragment"
            ),
            (
                "FireGemIcon",
                "fire_gem"
            ));

    public static readonly IHoverTip WinterStorm =
        CreateMixHoverTip(
            "RIKKU_WINTER_STORM",
            (
                "AntarcticWindIcon",
                "antarctic_wind"
            ),
            (
                "IceGemIcon",
                "ice_gem"
            ));

    public static readonly IHoverTip LightningBolt =
        CreateMixHoverTip(
            "RIKKU_LIGHTNING_BOLT",
            (
                "ElectroMarbleIcon",
                "electro_marble"
            ),
            (
                "LightningGemIcon",
                "lightning_gem"
            ));

    public static readonly IHoverTip TidalWave =
        CreateMixHoverTip(
            "RIKKU_TIDAL_WAVE",
            (
                "FishScaleIcon",
                "fish_scale"
            ),
            (
                "WaterGemIcon",
                "water_gem"
            ));

    public static readonly IHoverTip ChaosGrenade =
        CreateMixHoverTip(
            "RIKKU_CHAOS_GRENADE",
            (
                "FragGrenadeIcon",
                "frag_grenade"
            ),
            (
                "SupremeGemIcon",
                "supreme_gem"
            ));

    public static readonly IHoverTip MightyWall =
        CreateMixHoverTip(
            "RIKKU_MIGHTY_WALL",
            (
                "LightCurtainIcon",
                "light_curtain"
            ),
            (
                "StarCurtainIcon",
                "star_curtain"
            ));

    public static readonly IHoverTip HazardousShell =
        CreateMixHoverTip(
            "RIKKU_HAZARDOUS_SHELL");
}