using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Rikku.RikkuCode.Cards.Ancient;
using Rikku.RikkuCode.Extensions;
using Rikku.RikkuCode.Potions;

namespace Rikku.RikkuCode.Mechanics;

public partial class MixDisplay : Control
{
    public static MixDisplay? Instance
    {
        get;
        private set;
    }
    
    private Tween? _displayTween;

    private bool _lastVisibleState;
    
    private const int DefaultRecipeZIndex =
        0;

    private const int HighlightedRecipeZIndex =
        1;

    private Control? _display;

    private TextureRect? _mix1;
    private TextureRect? _mix2;
    private TextureRect? _mix3;
    private TextureRect? _mix4;
    private TextureRect? _mix5;
    private TextureRect? _mix6;
    private TextureRect? _mix7;

    private RichTextLabel? _mixLabel1;
    private RichTextLabel? _mixLabel2;
    private RichTextLabel? _mixLabel3;
    private RichTextLabel? _mixLabel4;
    private RichTextLabel? _mixLabel5;
    private RichTextLabel? _mixLabel6;
    private RichTextLabel? _mixLabel7;

    private Player? _player;

    public override void _Ready()
    {
        Instance =
            this;

        Name =
            "MixDisplay";

        MouseFilter =
            MouseFilterEnum.Pass;

        CallDeferred(
            nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        for (int i = 0; i < 60; i++)
        {
            var state =
                CombatManager.Instance?
                    .DebugOnlyGetState();

            var player =
                state?.Players.FirstOrDefault(
                    p => LocalContext.IsMe(p));

            if (player != null)
            {
                if (player.Character is not Character.Rikku)
                {
                    QueueFree();
                    return;
                }

                _player =
                    player;

                break;
            }

            SceneTree? tree =
                GetTree();

            if (tree == null)
                return;

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        PackedScene? scene =
            GD.Load<PackedScene>(
                "res://Rikku/scenes/mix_display.tscn");

        if (scene == null)
        {
            GD.PushError(
                "[Rikku Mix] Failed to load mix_display.tscn.");

            QueueFree();
            return;
        }

        _display =
            scene.Instantiate<Control>();

        AddChild(
            _display);

        _display.MouseFilter =
            MouseFilterEnum.Pass;

        /*
         * Your scene is already anchored BottomLeft,
         * so only use Position here to fine-tune it.
         */
        _display.Position =
            new Vector2(
                -125f,
                -140f);

        GetNodes();

        if (!ValidateNodes())
        {
            GD.PushError(
                "[Rikku Mix] mix_display.tscn is missing one or more Mix nodes.");

            QueueFree();
            return;
        }

        SetupLabels();
        SetupHoverTips();
        ShowAllMixes();

        _display.Visible =
            false;

        _display.Modulate =
            Colors.White;

        _display.Scale =
            Vector2.One;

        _lastVisibleState =
            false;

        UpdateVisibility();
    }

    private void GetNodes()
    {
        if (_display == null)
            return;

        _mix1 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix1");

        _mix2 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix2");

        _mix3 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix3");

        _mix4 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix4");

        _mix5 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix5");

        _mix6 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix6");

        _mix7 =
            _display.GetNodeOrNull<TextureRect>(
                "Mix7");

        _mixLabel1 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix1/MixLabel1");

        _mixLabel2 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix2/MixLabel2");

        _mixLabel3 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix3/MixLabel3");

        _mixLabel4 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix4/MixLabel4");

        _mixLabel5 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix5/MixLabel5");

        _mixLabel6 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix6/MixLabel6");

        _mixLabel7 =
            _display.GetNodeOrNull<RichTextLabel>(
                "Mix7/MixLabel7");
    }

    private bool ValidateNodes()
    {
        return
            _mix1 != null &&
            _mix2 != null &&
            _mix3 != null &&
            _mix4 != null &&
            _mix5 != null &&
            _mix6 != null &&
            _mix7 != null &&
            _mixLabel1 != null &&
            _mixLabel2 != null &&
            _mixLabel3 != null &&
            _mixLabel4 != null &&
            _mixLabel5 != null &&
            _mixLabel6 != null &&
            _mixLabel7 != null;
    }
    
    public override void _Process(
        double delta)
    {
        UpdateVisibility();
    }
    
    private void UpdateVisibility()
    {
        if (_display == null ||
            _player == null)
        {
            return;
        }

        bool hasMixCard =
            PileType.Hand
                .GetPile(_player)
                .Cards
                .Any(card =>
                    card is Mix ||
                    card is QuickMix);

        bool shouldShow =
            hasMixCard ||
            RikkuPotionMechanic.IsSelectingMix;

        if (shouldShow ==
            _lastVisibleState)
        {
            return;
        }

        _lastVisibleState =
            shouldShow;

        if (shouldShow)
        {
            PlayAppearAnimation();
        }
        else
        {
            PlayDisappearAnimation();
        }
    }
    
    private void PlayAppearAnimation()
    {
        if (_display == null)
            return;

        _displayTween?.Kill();
        _displayTween = null;

        _display.Visible =
            true;

        _display.PivotOffset =
            _display.Size / 2f;

        _display.Modulate =
            Colors.White;

        _display.Scale =
            new Vector2(
                1.3f,
                1.3f);

        SfxCmd.Play(
            "res://Rikku/sfx/mix_commands.wav");

        _displayTween =
            CreateTween();

        _displayTween.TweenProperty(
                _display,
                "scale",
                Vector2.One,
                0.30f)
            .SetTrans(
                Tween.TransitionType.Back)
            .SetEase(
                Tween.EaseType.Out);
    }
    
    private void PlayDisappearAnimation()
    {
        if (_display == null)
            return;

        _displayTween?.Kill();
        _displayTween = null;

        _display.PivotOffset =
            _display.Size / 2f;

        _display.Scale =
            Vector2.One;

        _display.Modulate =
            Colors.White;

        _displayTween =
            CreateTween();

        _displayTween.Parallel()
            .TweenProperty(
                _display,
                "scale",
                new Vector2(
                    1.15f,
                    1.15f),
                0.25f)
            .SetTrans(
                Tween.TransitionType.Back)
            .SetEase(
                Tween.EaseType.Out);

        _displayTween.Parallel()
            .TweenProperty(
                _display,
                "modulate:a",
                0f,
                0.25f);

        _displayTween.TweenCallback(
            Callable.From(() =>
            {
                if (_display == null ||
                    !GodotObject.IsInstanceValid(
                        _display))
                {
                    return;
                }

                _display.Visible =
                    false;

                _display.Scale =
                    Vector2.One;

                _display.Modulate =
                    Colors.White;
            }));
    }

    private void SetupLabels()
    {
        ConfigureLabel(
            _mixLabel1,
            "Burning Soul");

        ConfigureLabel(
            _mixLabel2,
            "Winter Storm");

        ConfigureLabel(
            _mixLabel3,
            "Lightning Bolt");

        ConfigureLabel(
            _mixLabel4,
            "Tidal Wave");

        ConfigureLabel(
            _mixLabel5,
            "Chaos Grenade");

        ConfigureLabel(
            _mixLabel6,
            "Mighty Wall");

        ConfigureLabel(
            _mixLabel7,
            "Hazardous Shell");
    }

    private static void ConfigureLabel(
        RichTextLabel? label,
        string text)
    {
        if (label == null)
            return;

        label.Text =
            text;

        label.Visible =
            true;

        /*
         * Let the parent TextureRect receive hovering.
         */
        label.MouseFilter =
            MouseFilterEnum.Ignore;

        Font? font =
            GD.Load<Font>(
                "res://themes/kreon_regular_shared.tres");

        if (font != null)
        {
            label.AddThemeFontOverride(
                "normal_font",
                font);
        }

        label.AddThemeFontSizeOverride(
            "normal_font_size",
            24);

        label.AddThemeColorOverride(
            "default_color",
            Colors.White);

        label.AddThemeColorOverride(
            "font_outline_color",
            Colors.Black);

        label.AddThemeConstantOverride(
            "outline_size",
            6);
        label.Position = new Vector2(
            70,
            -2);
    }

    private void SetupHoverTips()
    {
        RegisterHoverTip(
            _mix1!,
            RikkuStaticHoverTips.BurningSoul);

        RegisterHoverTip(
            _mix2!,
            RikkuStaticHoverTips.WinterStorm);

        RegisterHoverTip(
            _mix3!,
            RikkuStaticHoverTips.LightningBolt);

        RegisterHoverTip(
            _mix4!,
            RikkuStaticHoverTips.TidalWave);

        RegisterHoverTip(
            _mix5!,
            RikkuStaticHoverTips.ChaosGrenade);

        RegisterHoverTip(
            _mix6!,
            RikkuStaticHoverTips.MightyWall);

        RegisterHoverTip(
            _mix7!,
            RikkuStaticHoverTips.HazardousShell);
    }

    private void RegisterHoverTip(
        TextureRect icon,
        IHoverTip hoverTip)
    {
        icon.MouseFilter =
            MouseFilterEnum.Stop;

        icon.MouseEntered +=
            () =>
                OnMixHovered(
                    icon,
                    hoverTip);

        icon.MouseExited +=
            () =>
                OnMixUnhovered(
                    icon);
    }

    private void OnMixHovered(
        Control icon,
        IHoverTip hoverTip)
    {
        NHoverTipSet.Clear();

        var tip =
            NHoverTipSet.CreateAndShow(
                icon,
                hoverTip);

        if (tip == null)
            return;

        tip.MouseFilter =
            MouseFilterEnum.Ignore;

        /*
         * Panel is on the left, so push the recipe
         * tooltip to its right.
         */
        tip.GlobalPosition =
            icon.GlobalPosition +
            new Vector2(
                300f,
                -200f);
    }

    private static void OnMixUnhovered(
        Control icon)
    {
        NHoverTipSet.Remove(
            icon);
    }

    private void ShowAllMixes()
    {
        if (_display != null)
        {
            _display.Visible =
                true;
        }

        SetVisible(
            _mix1);

        SetVisible(
            _mix2);

        SetVisible(
            _mix3);

        SetVisible(
            _mix4);

        SetVisible(
            _mix5);

        SetVisible(
            _mix6);

        SetVisible(
            _mix7);
        
        ResetRecipeHighlights();
    }
    
    public void HighlightAvailableRecipes(
    bool canCancel)
{
    if (_player == null)
    {
        ResetRecipeHighlights();
        return;
    }

    List<PotionModel> potions =
        _player.Potions
            .ToList();

    ResetRecipeHighlights();

    SetRecipeHighlighted(
        _mix1,
        potions.Any(potion =>
            potion is BombFragment or FireGem));

    SetRecipeHighlighted(
        _mix2,
        potions.Any(potion =>
            potion is AntarcticWind or IceGem));

    SetRecipeHighlighted(
        _mix3,
        potions.Any(potion =>
            potion is ElectroMarble or LightningGem));

    SetRecipeHighlighted(
        _mix4,
        potions.Any(potion =>
            potion is FishScale or WaterGem));

    SetRecipeHighlighted(
        _mix5,
        potions.Any(potion =>
            potion is FragGrenade or SupremeGem));

    SetRecipeHighlighted(
        _mix6,
        potions.Any(potion =>
            potion is LightCurtain or StarCurtain));

    bool hasNonMixablePotion =
        potions.Any(potion =>
            potion is not RikkuPotion);

    /*
     * Normal Mix:
     * Hazardous Shell is available through cancellation.
     *
     * Quick Mix:
     * Cancellation is unavailable, so Hazardous Shell is
     * only possible if a non-Mixable Potion can be chosen
     * as the Primary ingredient.
     */
    bool hazardousShellAvailable =
        canCancel ||
        hasNonMixablePotion;

    SetRecipeHighlighted(
        _mix7,
        hazardousShellAvailable);
}

public void HighlightRecipeForPrimary(
    PotionModel primaryIngredient)
{
    ResetRecipeHighlights();

    TextureRect? recipe =
        GetRecipeForPrimary(
            primaryIngredient);

    SetRecipeHighlighted(
        recipe,
        true);
}

public void ResetRecipeHighlights()
{
    SetRecipeHighlighted(
        _mix1,
        false);

    SetRecipeHighlighted(
        _mix2,
        false);

    SetRecipeHighlighted(
        _mix3,
        false);

    SetRecipeHighlighted(
        _mix4,
        false);

    SetRecipeHighlighted(
        _mix5,
        false);

    SetRecipeHighlighted(
        _mix6,
        false);

    SetRecipeHighlighted(
        _mix7,
        false);
}

private TextureRect? GetRecipeForPrimary(
    PotionModel primaryIngredient)
{
    return primaryIngredient switch
    {
        BombFragment or FireGem =>
            _mix1,

        AntarcticWind or IceGem =>
            _mix2,

        ElectroMarble or LightningGem =>
            _mix3,

        FishScale or WaterGem =>
            _mix4,

        FragGrenade or SupremeGem =>
            _mix5,

        LightCurtain or StarCurtain =>
            _mix6,

        _ =>
            _mix7
    };
}

private static void SetRecipeHighlighted(
    TextureRect? recipe,
    bool highlighted)
{
    if (recipe == null ||
        !GodotObject.IsInstanceValid(
            recipe))
    {
        return;
    }

    recipe.ZIndex =
        highlighted
            ? HighlightedRecipeZIndex
            : DefaultRecipeZIndex;
}

    private static void SetVisible(
        TextureRect? mix)
    {
        if (mix == null)
            return;

        mix.Visible =
            true;

        mix.Modulate =
            Colors.White;
    }

    public override void _ExitTree()
    {
        _displayTween?.Kill();
        _displayTween = null;
        
        if (Instance == this)
        {
            Instance =
                null;
        }
        
        ResetRecipeHighlights();

        RemoveHoverTip(
            _mix1);

        RemoveHoverTip(
            _mix2);

        RemoveHoverTip(
            _mix3);

        RemoveHoverTip(
            _mix4);

        RemoveHoverTip(
            _mix5);

        RemoveHoverTip(
            _mix6);

        RemoveHoverTip(
            _mix7);

        _mix1 = null;
        _mix2 = null;
        _mix3 = null;
        _mix4 = null;
        _mix5 = null;
        _mix6 = null;
        _mix7 = null;

        _mixLabel1 = null;
        _mixLabel2 = null;
        _mixLabel3 = null;
        _mixLabel4 = null;
        _mixLabel5 = null;
        _mixLabel6 = null;
        _mixLabel7 = null;

        _display = null;
        _player = null;
    }

    private static void RemoveHoverTip(
        TextureRect? icon)
    {
        if (icon == null)
            return;

        if (!GodotObject.IsInstanceValid(
                icon))
        {
            return;
        }

        NHoverTipSet.Remove(
            icon);
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready))]
public static class MixDisplayPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(
                __instance))
        {
            return;
        }

        var state =
            CombatManager.Instance?
                .DebugOnlyGetState();

        var player =
            state?.Players.FirstOrDefault(
                p => LocalContext.IsMe(p));

        if (player?.Character
            is not Character.Rikku)
        {
            return;
        }

        if (__instance
                .GetNodeOrNull<MixDisplay>(
                    "RikkuMixDisplay") != null)
        {
            return;
        }

        __instance.AddChild(
            new MixDisplay
            {
                Name =
                    "RikkuMixDisplay"
            });
    }
}