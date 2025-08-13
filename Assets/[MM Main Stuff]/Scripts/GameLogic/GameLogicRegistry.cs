using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameLogicRegistry {
    // Prefab references assigned by Initializer.cs
    public static GameObject dinoPrefab;
    public static GameObject dinoAvatarPrefab;
    public static GameObject tugPrefab;

    // [KC] Kernel Chaos
    public static GameObject kernelChaosPrefab;

    // Logic handler factory (constructor-based)
    private static readonly Dictionary<string, Func<IGameLogicHandler>> handlers = new() {
        { "dino_run",     () => new DinoRunLogic(dinoPrefab, dinoAvatarPrefab) },
        { "tug_of_war",   () => new TugOfWarLogic(tugPrefab) },
        { "kernel_chaos", () => new KernelChaosLogic(kernelChaosPrefab) }
    };

    /// <summary>
    /// Get the logic handler for a given gameType (e.g. "dino_run")
    /// </summary>
    public static IGameLogicHandler Get(string gameType) {
        return handlers.TryGetValue(gameType, out var creator) ? creator() : null;
    }
}
