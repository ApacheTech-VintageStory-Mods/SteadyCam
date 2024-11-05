using Gantry.Core.Extensions;
using Gantry.Core.ModSystems;
using Gantry.Services.HarmonyPatches.Annotations;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.SteadyCam.Features.TemporalStabilityFix;

[HarmonySidedPatch(EnumAppSide.Universal)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class TemporalStabilityFixUniversal : UniversalModSystem
{

#if DEBUG
    public override void StartServerSide(ICoreServerAPI sapi)
    {
        sapi.ChatCommands
            .Create("stability")
            .RequiresPrivilege(Privilege.controlserver)
            .WithDescription("[DEBUG] Sets the player's temporal stability.")
            .IgnoreAdditionalArgs()
            .HandleWith(args =>
            {
                SetTemporalStability(args.Caller.Player, args.RawArgs.PopDouble().GetValueOrDefault(1.0));
                return TextCommandResult.Success();
            });
    }

    private static void SetTemporalStability(IPlayer player, double value)
    {
        var entityPlayer = player.Entity;
        if (!entityPlayer.HasBehavior<EntityBehaviorTemporalStabilityAffected>()) return;
        var behaviour = entityPlayer.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
        behaviour.OwnStability = value;
        entityPlayer.Api.Logger.Audit($"Set Temporal Stability for {player.PlayerName}: {value}");
    }
#endif

    /// <summary>
    ///     Fix spectator mode accounts when a player has their game mode changed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntityBehaviorTemporalStabilityAffected), nameof(EntityBehaviorTemporalStabilityAffected.OwnStability), MethodType.Getter)]
    public static void Harmony_EntityBehaviorTemporalStabilityAffected_get_OwnStability_Postfix(Entity ___entity, ref double __result)
    {
        if (!___entity.IsSpectator()) return;
        __result = 1.0;
    }

    /// <summary>
    ///     Fix spectator mode accounts when a player has their game mode changed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntityBehaviorTemporalStabilityAffected), nameof(EntityBehaviorTemporalStabilityAffected.OwnStability), MethodType.Setter)]
    public static void Harmony_EntityBehaviorTemporalStabilityAffected_set_OwnStability_Postfix(Entity ___entity)
    {
        if (!___entity.IsSpectator()) return;
        ___entity.WatchedAttributes.SetDouble("temporalStability", 1.0);
    }

    /// <summary>
    ///     Fix spectator mode accounts when a player has their game mode changed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntityBehaviorTemporalStabilityAffected), nameof(EntityBehaviorTemporalStabilityAffected.TempStabChangeVelocity), MethodType.Getter)]
    public static void Harmony_EntityBehaviorTemporalStabilityAffected_get_TempStabChangeVelocity_Postfix(Entity ___entity, ref double __result)
    {
        if (!___entity.IsSpectator()) return;
        __result = 0d;
    }

    /// <summary>
    ///     Fix spectator mode accounts when a player has their game mode changed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EntityBehaviorTemporalStabilityAffected), nameof(EntityBehaviorTemporalStabilityAffected.TempStabChangeVelocity), MethodType.Setter)]
    public static void Harmony_EntityBehaviorTemporalStabilityAffected_set_TempStabChangeVelocity_Postfix(Entity ___entity)
    {
        if (!___entity.IsSpectator()) return;
        ___entity.Attributes.SetDouble("tempStabChangeVelocity", 0d);
    }
}