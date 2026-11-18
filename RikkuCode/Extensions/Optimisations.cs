using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Rikku.RikkuCode.Extensions;

public static class RikkuAssets
{
    private static PackedScene? _rikkuScene;
    private static PackedScene? _vfxScene;

    private const string RikkuScenePath = "res://Rikku/scenes/rikku.tscn";
    private const string VfxPath = "res://Rikku/scenes/vfx.tscn";
    
    
    public static bool IsRikkuInRun(IRunState runState)
    {
        return runState?.Players?.Any(
            p => p?.Character is Rikku.RikkuCode.Character.Rikku
        ) ?? false;
    }

    public static PackedScene? RikkuScene
    {
        get
        {
            _rikkuScene = LoadOrReload(_rikkuScene, RikkuScenePath, "Rikku scene");
            return _rikkuScene;
        }
    }

    public static PackedScene? VfxScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "VFX");
            return _vfxScene;
        }
    }
    
    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"RikkuAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"RikkuAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"RikkuAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = RikkuScene;
        _ = VfxScene;

        GD.Print("RikkuAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class RikkuAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Rikku)
        //     return;
        if (!RikkuAssets.IsRikkuInRun(runState))
            return;
        
        GD.Print("AfterActEntered: Rikku detected → preloading");

        RikkuAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class RikkuAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Rikku)
        //     return;
        if (!RikkuAssets.IsRikkuInRun(runState))
            return;
        
        GD.Print($"AfterRoomEntered: Rikku detected → preloading. Room = {room.GetType().Name}");

        RikkuAssets.EnsurePreloaded();
    }
}
