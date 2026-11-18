using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using Rikku.RikkuCode.Powers;

namespace Rikku.RikkuCode.Mechanics;

public partial class RikkuMixInputBlocker : Control
{
    public override void _Input(
        InputEvent inputEvent)
    {
        if (!RikkuPotionMechanic
                .IsSelectingMixIngredient)
        {
            return;
        }

        if (!inputEvent.IsActionPressed(
                MegaInput.cancel))
        {
            return;
        }

        RikkuPotionMechanic
            .CancelMixSelection();

        GetViewport()?
            .SetInputAsHandled();
    }
}

public static class RikkuPotionMechanic
{
    private static RikkuMixInputBlocker?
        _inputBlocker;
    
    private static bool _isSelectingSecondaryIngredient;
    
    private static TaskCompletionSource<PotionModel?>?
        _ingredientSelectionSource;
    
    private static Control? _cancelButtonLayer;
    
    private static bool _disabledHandForMix;

    private static PotionModel? _excludedIngredient;
    
    private static bool _canCancelMixSelection;
    
    private static NPotionHolder? _excludedHolder;

    private static Control? _selectModeBackstop;

    private static MegaRichTextLabel? _selectionHeader;
    
    public static bool IsSelectingMix
    {
        get;
        private set;
    }

    public static bool IsSelectingDiscard
    {
        get;
        private set;
    }

    public static bool IsSelectingMixIngredient =>
        _ingredientSelectionSource != null;

    public static bool IsConsumingMixIngredient
    {
        get;
        private set;
    }
    
    private const string CancelButtonPath =
        "res://scenes/ui/choice_selection_skip_button.tscn";

    private static NChoiceSelectionSkipButton? _cancelButton;
    
    // =====================================================================
    // DOUBLE DOSE
    // =====================================================================

    // =====================================================================
    // MIX SELECTION
    // =====================================================================
    
    
    private static void CreateInputBlocker()
    {
        if (_inputBlocker != null &&
            GodotObject.IsInstanceValid(
                _inputBlocker))
        {
            return;
        }

        NCombatRoom? combatRoom =
            NCombatRoom.Instance;

        if (combatRoom == null)
            return;

        _inputBlocker =
            new RikkuMixInputBlocker
            {
                Name = "RikkuMixInputBlocker",
                MouseFilter =
                    Control.MouseFilterEnum.Ignore
            };

        combatRoom.Ui.AddChildSafely(
            _inputBlocker);

        _inputBlocker.SetAnchorsPreset(
            Control.LayoutPreset.FullRect);

        _inputBlocker.OffsetLeft = 0f;
        _inputBlocker.OffsetTop = 0f;
        _inputBlocker.OffsetRight = 0f;
        _inputBlocker.OffsetBottom = 0f;
    }
    
    
    public static async Task<(
            PotionModel? Primary,
            PotionModel? Secondary)>
        SelectMixIngredients(
            bool canCancel = true)
    {
        IsSelectingMix =
            true;

        _isSelectingSecondaryIngredient =
            false;

        BeginMixSelectionUi(
            "Choose a Primary Ingredient");

        SetMixCancelable(
            canCancel);

        try
        {
            while (true)
            {
                _isSelectingSecondaryIngredient =
                    false;

                UpdateMixSelectionPrompt(
                    "Choose a Primary Ingredient");

                UpdateCancelButtonState();

                MixDisplay.Instance?
                    .HighlightAvailableRecipes(
                        canCancel);

                PotionModel? primaryIngredient =
                    await SelectMixIngredient();

                if (primaryIngredient == null)
                {
                    return (
                        null,
                        null);
                }

                MixDisplay.Instance?
                    .HighlightRecipeForPrimary(
                        primaryIngredient);

                _isSelectingSecondaryIngredient =
                    true;

                UpdateMixSelectionPrompt(
                    "Choose a Secondary Ingredient");

                UpdateCancelButtonState();

                PotionModel? secondaryIngredient =
                    await SelectMixIngredient(
                        primaryIngredient);

                if (secondaryIngredient == null)
                {
                    continue;
                }

                return (
                    primaryIngredient,
                    secondaryIngredient);
            }
        }
        finally
        {
            _isSelectingSecondaryIngredient =
                false;

            MixDisplay.Instance?
                .ResetRecipeHighlights();

            SetMixCancelable(
                false);

            EndMixSelectionUi();

            IsSelectingMix =
                false;
        }
    }

    private static async Task<PotionModel?> SelectMixIngredient(
        PotionModel? excludedIngredient = null)
    {
        if (_ingredientSelectionSource != null)
        {
            throw new InvalidOperationException(
                "A Mix ingredient selection is already active.");
        }

        _excludedIngredient =
            excludedIngredient;

        SetExcludedIngredientVisual(
            excludedIngredient);

        _ingredientSelectionSource =
            new TaskCompletionSource<PotionModel?>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        FocusPotionBelt();

        try
        {
            return await _ingredientSelectionSource.Task;
        }
        finally
        {
            ClearExcludedIngredientVisual();
            
            _ingredientSelectionSource = null;
            _excludedIngredient = null;
        }
    }

    public static void CancelMixSelection()
    {
        bool canGoBack =
            IsSelectingMix &&
            _isSelectingSecondaryIngredient;

        if (!_canCancelMixSelection &&
            !canGoBack)
        {
            return;
        }

        _ingredientSelectionSource?
            .TrySetResult(
                null);
    }

    public static bool TrySelectIngredient(
        PotionModel potion)
    {
        if (_ingredientSelectionSource == null)
            return false;

        if (ReferenceEquals(
                potion,
                _excludedIngredient))
        {
            return false;
        }

        SfxCmd.Play(
            "event:/sfx/ui/clicks/ui_click");

        _ingredientSelectionSource.TrySetResult(
            potion);

        return true;
    }

    // =====================================================================
    // MIX SELECTION UI
    // =====================================================================

    private static void BeginMixSelectionUi(
        string prompt)
    {
        NPlayerHand? hand =
            NPlayerHand.Instance;

        if (hand == null)
            return;

        DisableHandForMix();
        CreateInputBlocker();

        _selectModeBackstop =
            AccessTools.Field(
                    typeof(NPlayerHand),
                    "_selectModeBackstop")
                .GetValue(hand) as Control;

        _selectionHeader =
            AccessTools.Field(
                    typeof(NPlayerHand),
                    "_selectionHeader")
                .GetValue(hand) as MegaRichTextLabel;

        if (_selectModeBackstop != null)
        {
            _selectModeBackstop.Visible = true;
            _selectModeBackstop.MouseFilter =
                Control.MouseFilterEnum.Stop;

            Color selfModulate =
                _selectModeBackstop.SelfModulate;

            selfModulate.A = 0f;

            _selectModeBackstop.SelfModulate =
                selfModulate;

            Tween tween =
                _selectModeBackstop.CreateTween();

            tween.TweenProperty(
                _selectModeBackstop,
                "self_modulate:a",
                1f,
                0.2f);
        }

        UpdateMixSelectionPrompt(
            prompt);

        FocusPotionBelt();
    }


    private static void UpdateMixSelectionPrompt(
        string prompt)
    {
        if (_selectionHeader == null)
            return;

        _selectionHeader.Visible = true;
        _selectionHeader.Text =
            $"[center]{prompt}[/center]";
    }

    private static void EndMixSelectionUi()
    {
        Control? backstop =
            _selectModeBackstop;

        MegaRichTextLabel? header =
            _selectionHeader;

        _selectModeBackstop = null;
        _selectionHeader = null;

        if (header != null &&
            GodotObject.IsInstanceValid(header))
        {
            header.Visible = false;
        }

        if (backstop != null &&
            GodotObject.IsInstanceValid(backstop))
        {
            backstop.MouseFilter =
                Control.MouseFilterEnum.Ignore;

            Tween tween =
                backstop.CreateTween();

            tween.TweenProperty(
                backstop,
                "self_modulate:a",
                0f,
                0.2f);

            tween.TweenCallback(
                Callable.From(() =>
                {
                    if (GodotObject.IsInstanceValid(
                            backstop))
                    {
                        backstop.Visible = false;
                    }
                }));
        }

        EnableHandAfterMix();

        NCombatRoom.Instance?
            .EnableControllerNavigation();
    }

    private static List<NPotionHolder>
        GetOccupiedPotionHolders()
    {
        NPotionContainer? potionContainer =
            NRun.Instance?
                .GlobalUi
                .TopBar
                .PotionContainer;

        if (potionContainer == null)
            return [];

        List<NPotionHolder> holders =
            AccessTools.Field(
                    typeof(NPotionContainer),
                    "_holders")
                .GetValue(potionContainer)
                as List<NPotionHolder>
            ?? [];

        return holders
            .Where(holder =>
                holder.HasPotion &&
                holder.Potion != null)
            .ToList();
    }

    private static void FocusPotionBelt()
    {
        List<NPotionHolder> holders =
            GetOccupiedPotionHolders();

        if (holders.Count == 0)
            return;

        List<Control> controls =
            holders
                .Where(holder =>
                    holder.Potion?.Model !=
                    _excludedIngredient)
                .Cast<Control>()
                .ToList();

        if ((_canCancelMixSelection ||
             _isSelectingSecondaryIngredient) &&
            _cancelButton != null &&
            GodotObject.IsInstanceValid(
                _cancelButton) &&
            _cancelButton.Visible)
        {
            controls.Add(
                _cancelButton);
        }

        NCombatRoom.Instance?
            .RestrictControllerNavigation(
                controls);

        NPotionHolder? firstHolder =
            holders.FirstOrDefault(holder =>
                holder.Potion?.Model !=
                _excludedIngredient);

        firstHolder?.TryGrabFocus();
    }

    // =====================================================================
    // MIX CONSUMPTION
    // =====================================================================

    public static void ConsumeMixIngredients(
        PotionModel primaryIngredient,
        PotionModel secondaryIngredient,
        bool consumeSecondary = false)
    {
        if (ReferenceEquals(
                primaryIngredient,
                secondaryIngredient))
        {
            throw new InvalidOperationException(
                "The same Potion cannot be used as both Mix ingredients.");
        }

        IsConsumingMixIngredient = true;

        try
        {
            primaryIngredient.RemoveBeforeUse();

            if (consumeSecondary)
            {
                secondaryIngredient.RemoveBeforeUse();
            }
        }
        finally
        {
            IsConsumingMixIngredient = false;
        }
    }
    
    private static void SetExcludedIngredientVisual(
        PotionModel? potion)
    {
        ClearExcludedIngredientVisual();

        if (potion == null)
            return;

        _excludedHolder =
            GetOccupiedPotionHolders()
                .FirstOrDefault(holder =>
                    holder.Potion?.Model == potion);

        if (_excludedHolder == null)
            return;

        _excludedHolder.Modulate =
            StsColors.gray;

        _excludedHolder.FocusMode =
            Control.FocusModeEnum.None;
    }

    private static void ClearExcludedIngredientVisual()
    {
        if (_excludedHolder == null)
            return;

        if (GodotObject.IsInstanceValid(
                _excludedHolder))
        {
            _excludedHolder.Modulate =
                Colors.White;

            _excludedHolder.FocusMode =
                Control.FocusModeEnum.All;
        }

        _excludedHolder = null;
    }
    
    private static void CreateCancelButton()
{
    if (_cancelButton != null &&
        GodotObject.IsInstanceValid(
            _cancelButton))
    {
        return;
    }

    NCombatRoom? combatRoom =
        NCombatRoom.Instance;

    if (combatRoom == null)
        return;

    PackedScene? scene =
        GD.Load<PackedScene>(
            CancelButtonPath);

    if (scene == null)
        return;

    _cancelButtonLayer =
        new Control
        {
            Name =
                "RikkuMixCancelButtonLayer",

            MouseFilter =
                Control.MouseFilterEnum.Ignore
        };

    combatRoom.Ui.AddChildSafely(
        _cancelButtonLayer);

    _cancelButtonLayer.SetAnchorsPreset(
        Control.LayoutPreset.FullRect);

    _cancelButtonLayer.OffsetLeft = 0f;
    _cancelButtonLayer.OffsetTop = 0f;
    _cancelButtonLayer.OffsetRight = 0f;
    _cancelButtonLayer.OffsetBottom = 0f;

    _cancelButtonLayer.Position =
        new Vector2(
            0f,
            -180f);

    _cancelButton =
        scene.Instantiate<NChoiceSelectionSkipButton>(
            PackedScene.GenEditState.Disabled);

    _cancelButton.Name =
        "RikkuMixCancelButton";

    // AccessTools.Field(
    //         typeof(NChoiceSelectionSkipButton),
    //         "_optionName")
    //     .SetValue(
    //         _cancelButton,
    //         "Don't Mix");

    _cancelButtonLayer.AddChildSafely(
        _cancelButton);

    _cancelButton.SetAnchorsPreset(
        Control.LayoutPreset.TopLeft);
    
    _cancelButton.GetNode<MegaLabel>("Label").SetTextAutoSize("Don't Mix");

    _cancelButton.ResetSize();

    _cancelButtonLayer.Resized +=
        CenterCancelButton;

    Callable.From(
            CenterCancelButton)
        .CallDeferred();

    _cancelButton.Connect(
        NClickableControl.SignalName.Focused,
        Callable.From<NClickableControl>(_ =>
        {
            SfxCmd.Play(
                "event:/sfx/ui/clicks/ui_hover");
        }));

    _cancelButton.Connect(
        NClickableControl.SignalName.Released,
        Callable.From<NButton>(_ =>
        {
            SfxCmd.Play(
                "event:/sfx/ui/clicks/ui_click");

            CancelMixSelection();
        }));

    _cancelButton.Visible =
        false;
}
    
    private static void CenterCancelButton()
    {
        if (_cancelButtonLayer == null ||
            !GodotObject.IsInstanceValid(
                _cancelButtonLayer))
        {
            return;
        }

        if (_cancelButton == null ||
            !GodotObject.IsInstanceValid(
                _cancelButton))
        {
            return;
        }

        Vector2 layerSize =
            _cancelButtonLayer.Size;

        Vector2 buttonSize =
            _cancelButton.Size;

        _cancelButton.Position =
            new Vector2(
                (layerSize.X - buttonSize.X) * 0.5f,
                layerSize.Y - buttonSize.Y - 200f);
    }
    
    
    private static void SetMixCancelable(
        bool canCancel)
    {
        _canCancelMixSelection =
            canCancel;

        UpdateCancelButtonState();

        if ((_canCancelMixSelection ||
             _isSelectingSecondaryIngredient) &&
            _cancelButton != null &&
            GodotObject.IsInstanceValid(
                _cancelButton))
        {
            _cancelButton.AnimateIn();
        }
    }

    private static void DisableHandForMix()
    {
        NPlayerHand? hand =
            NPlayerHand.Instance;
    
        if (hand == null)
            return;
    
        bool wasDisabled =
            (bool)(AccessTools.Field(
                           typeof(NPlayerHand),
                           "_isDisabled")
                       .GetValue(hand)
                   ?? false);
    
        _disabledHandForMix =
            !wasDisabled;
    
        foreach (NHandCardHolder holder
                 in hand.ActiveHolders)
        {
            holder.SetClickable(false);
    
            holder.FocusMode =
                Control.FocusModeEnum.None;
    
            holder.MouseFilter =
                Control.MouseFilterEnum.Ignore;
    
            holder.Hitbox.SetEnabled(false);
    
            holder.Hitbox.MouseFilter =
                Control.MouseFilterEnum.Ignore;
    
            holder.Modulate =
                StsColors.gray;
        }
    
        if (_disabledHandForMix)
        {
            AccessTools.Method(
                    typeof(NPlayerHand),
                    "AnimDisable")
                .Invoke(
                    hand,
                    null);
        }
    }

    private static void EnableHandAfterMix()
    {
        NPlayerHand? hand =
            NPlayerHand.Instance;
    
        if (hand == null)
            return;
    
        foreach (NHandCardHolder holder
                 in hand.ActiveHolders)
        {
            holder.SetClickable(true);
    
            holder.FocusMode =
                Control.FocusModeEnum.All;
    
            holder.MouseFilter =
                Control.MouseFilterEnum.Stop;
    
            holder.Hitbox.SetEnabled(true);
    
            holder.Hitbox.MouseFilter =
                Control.MouseFilterEnum.Stop;
    
            holder.Modulate =
                Colors.White;
        }
    
        if (_disabledHandForMix)
        {
            AccessTools.Method(
                    typeof(NPlayerHand),
                    "AnimEnable")
                .Invoke(
                    hand,
                    null);
        }
    
        _disabledHandForMix = false;
    }
    
    public static async Task<PotionModel?> SelectDiscardPotionLocally(
        bool canCancel = true)
    {
        IsSelectingDiscard = true;

        BeginMixSelectionUi(
            "Choose a Potion to Discard");

        SetMixCancelable(
            canCancel);

        SetCancelButtonText(
            "Don't Discard");

        PotionModel? selectedPotion =
            null;

        try
        {
            selectedPotion =
                await SelectMixIngredient();
        }
        finally
        {
            SetMixCancelable(
                false);

            EndMixSelectionUi();

            IsSelectingDiscard =
                false;
        }

        if (selectedPotion == null)
            return null;

        return selectedPotion;
    }
    
    private static void SetCancelButtonText(
        string text)
    {
        if (_cancelButton == null ||
            !GodotObject.IsInstanceValid(
                _cancelButton))
        {
            CreateCancelButton();
        }

        if (_cancelButton == null ||
            !GodotObject.IsInstanceValid(
                _cancelButton))
        {
            return;
        }

        // AccessTools.Field(
        //         typeof(NChoiceSelectionSkipButton),
        //         "_optionName")
        //     .SetValue(
        //         _cancelButton,
        //         text);
        //
        // UpdateCancelButtonLabel(
        //     _cancelButton,
        //     text);
        
        _cancelButton.GetNode<MegaLabel>("Label").SetTextAutoSize(text);

        _cancelButton.ResetSize();

        Callable.From(
                CenterCancelButton)
            .CallDeferred();
    }
    
    private static void UpdateCancelButtonLabel(
        Node node,
        string text)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is MegaRichTextLabel richTextLabel)
            {
                richTextLabel.Text =
                    text;
            }
            else if (child is Label label)
            {
                label.Text =
                    text;
            }

            UpdateCancelButtonLabel(
                child,
                text);
        }
    }
    
    // =====================================================================
    // DOUBLE DOSE
    // =====================================================================

    private static PotionModel?
        _preservedForDoubleDose;

    private static PotionModel?
        _preservedByStash;

    private static PotionModel?
        _pendingDoubleDoseReplay;

    private static PotionModel?
        _activeDoubleDoseReplay;

    private static bool
        _pendingReplayRetainsPotion;

    private static bool
        _activeReplayRetainsPotion;

    public static bool IsPreservedForDoubleDose(
        PotionModel potion)
    {
        return ReferenceEquals(
            _preservedForDoubleDose,
            potion);
    }

    public static void PreserveForDoubleDose(
        PotionModel potion)
    {
        _preservedForDoubleDose =
            potion;
    }

    public static void ClearDoubleDosePreservation(
        PotionModel potion)
    {
        if (!ReferenceEquals(
                _preservedForDoubleDose,
                potion))
        {
            return;
        }

        _preservedForDoubleDose =
            null;
    }

    public static bool IsPreservedByStash(
        PotionModel potion)
    {
        return ReferenceEquals(
            _preservedByStash,
            potion);
    }

    public static void PreserveByStash(
        PotionModel potion)
    {
        _preservedByStash =
            potion;
    }

    public static void ClearStashPreservation(
        PotionModel potion)
    {
        if (!ReferenceEquals(
                _preservedByStash,
                potion))
        {
            return;
        }

        _preservedByStash =
            null;
    }

    public static bool IsDoubleDoseReplay(
        PotionModel potion)
    {
        return ReferenceEquals(
            _activeDoubleDoseReplay,
            potion);
    }

    public static bool ShouldRetainDoubleDoseReplay(
        PotionModel potion)
    {
        return ReferenceEquals(
                   _activeDoubleDoseReplay,
                   potion) &&
               _activeReplayRetainsPotion;
    }

    public static void QueueDoubleDoseReplay(
        PotionModel potion)
    {
        if (_pendingDoubleDoseReplay != null ||
            _activeDoubleDoseReplay != null)
        {
            throw new InvalidOperationException(
                "A Double Dose replay is already pending or active.");
        }

        _pendingDoubleDoseReplay =
            potion;

        /*
         * Capture whether Stash preserved the original use.
         *
         * Stash may be decremented before the replay starts, so the
         * replay cannot rely on checking whether StashPower still
         * exists later.
         */
        _pendingReplayRetainsPotion =
            IsPreservedByStash(
                potion);

        /*
         * The replay now owns this information.
         */
        ClearStashPreservation(
            potion);
    }

    public static bool TryBeginDoubleDoseReplay(
        PotionModel potion)
    {
        if (!ReferenceEquals(
                _pendingDoubleDoseReplay,
                potion))
        {
            return false;
        }

        _pendingDoubleDoseReplay =
            null;

        _activeDoubleDoseReplay =
            potion;

        _activeReplayRetainsPotion =
            _pendingReplayRetainsPotion;

        _pendingReplayRetainsPotion =
            false;

        return true;
    }

    public static void CancelPendingDoubleDoseReplay(
        PotionModel potion)
    {
        if (!ReferenceEquals(
                _pendingDoubleDoseReplay,
                potion))
        {
            return;
        }

        _pendingDoubleDoseReplay =
            null;

        _pendingReplayRetainsPotion =
            false;
    }

    public static void CompleteDoubleDoseReplay(
        PotionModel potion)
    {
        if (!ReferenceEquals(
                _activeDoubleDoseReplay,
                potion))
        {
            return;
        }

        _activeDoubleDoseReplay =
            null;

        _activeReplayRetainsPotion =
            false;

        ClearDoubleDosePreservation(
            potion);

        ClearStashPreservation(
            potion);
    }

    public static void ConsumePreservedPotion(
        PotionModel potion)
    {
        ClearDoubleDosePreservation(
            potion);

        ClearStashPreservation(
            potion);

        /*
         * Reuse the existing explicit-consumption bypass.
         *
         * PotionRemoveBeforeUsePatch allows removal whenever
         * IsConsumingMixIngredient is true, so Double Dose and
         * Stash cannot preserve this potion again.
         */
        IsConsumingMixIngredient =
            true;

        try
        {
            potion.RemoveBeforeUse();
        }
        finally
        {
            IsConsumingMixIngredient =
                false;
        }

        potion.AfterUsageCanceled();
    }

    public static void ResetDoubleDoseState()
    {
        _preservedForDoubleDose =
            null;

        _preservedByStash =
            null;

        _pendingDoubleDoseReplay =
            null;
        
        _activeDoubleDoseReplay = 
            null;

        _pendingReplayRetainsPotion =
            false;

        _activeReplayRetainsPotion =
            false;

        _doubleDoseCombatEndRemoval =
            null;
    }
    
    private static PotionModel?
        _doubleDoseCombatEndRemoval;

    public static void MarkForDoubleDoseCombatEndRemoval(
        PotionModel potion)
    {
        _doubleDoseCombatEndRemoval =
            potion;
    }

    public static PotionModel?
        TakeDoubleDoseCombatEndRemoval()
    {
        PotionModel? potion =
            _doubleDoseCombatEndRemoval;

        _doubleDoseCombatEndRemoval =
            null;

        return potion;
    }
    
    private static void UpdateCancelButtonState()
    {
        if (_cancelButton == null ||
            !GodotObject.IsInstanceValid(
                _cancelButton))
        {
            CreateCancelButton();
        }

        if (_cancelButton == null ||
            !GodotObject.IsInstanceValid(
                _cancelButton))
        {
            return;
        }

        bool shouldShow =
            _canCancelMixSelection ||
            _isSelectingSecondaryIngredient;

        if (!shouldShow)
        {
            _cancelButton.Disable();

            _cancelButton.Visible =
                false;

            FocusPotionBelt();

            return;
        }

        string buttonText;

        if (IsSelectingDiscard)
        {
            buttonText =
                "Don't Discard";
        }
        else if (_isSelectingSecondaryIngredient)
        {
            buttonText =
                "Back";
        }
        else
        {
            buttonText =
                "Don't Mix";
        }

        SetCancelButtonText(
            buttonText);

        _cancelButton.Visible =
            true;

        _cancelButton.Enable();

        Callable.From(
                CenterCancelButton)
            .CallDeferred();

        FocusPotionBelt();
    }
    
}

[HarmonyPatch(
    typeof(PotionModel),
    nameof(PotionModel.RemoveBeforeUse))]
public static class PotionRemoveBeforeUsePatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        PotionModel __instance)
    {
        /*
         * Mix explicitly consumes its ingredients.
         *
         * Stash and Double Dose do not interfere with that
         * explicit consumption.
         */
        if (RikkuPotionMechanic
            .IsConsumingMixIngredient)
        {
            return true;
        }

        /*
         * This is the second Double Dose activation.
         *
         * Without Stash, allow the replay to consume the potion.
         *
         * With Stash, prevent removal so the potion remains in
         * the belt after both activations.
         */
        if (RikkuPotionMechanic
            .IsDoubleDoseReplay(
                __instance))
        {
            return !RikkuPotionMechanic
                .ShouldRetainDoubleDoseReplay(
                    __instance);
        }

        Creature? owner =
            __instance.Owner?.Creature;

        if (owner == null)
        {
            return true;
        }

        /*
         * Stash has priority over temporary Double Dose
         * preservation.
         */
        if (owner.HasPower<StashPower>())
        {
            RikkuPotionMechanic
                .PreserveByStash(
                    __instance);

            return false;
        }

        /*
         * Double Dose needs the PotionModel to remain in the
         * owner's potion list until UsePotionAction for the replay
         * has been constructed.
         *
         * The power itself decides after the first activation
         * whether this potion is actually eligible for a replay.
         */
        if (owner.HasPower<DoubleDosePower>())
        {
            RikkuPotionMechanic
                .PreserveForDoubleDose(
                    __instance);

            return false;
        }

        return true;
    }
    
    
}

[HarmonyPatch(
    typeof(NPotionHolder),
    "OnRelease")]
public static class MixPotionSelectionPatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        NPotionHolder __instance)
    {
        if (!RikkuPotionMechanic.IsSelectingMixIngredient)
            return true;

        if (!__instance.HasPotion ||
            __instance.Potion == null)
        {
            return false;
        }

        RikkuPotionMechanic.TrySelectIngredient(
            __instance.Potion.Model);

        return false;
    }
}



