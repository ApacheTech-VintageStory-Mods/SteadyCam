﻿// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.SteadyCam.Features.CamTarget;

[HarmonySidedPatch(EnumAppSide.Client)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CamTarget : ClientModSystem, IClientServiceRegistrar
{
    private static CamTargetSettings _settings;
    private static SystemCinematicCamera _cinematicCamera;

    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<CamTargetSettings>();
    }

    private static string L(string path, params string[] args)
    {
        return LangEx.FeatureString("CamTarget", path, args);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _settings = IOC.Services.Resolve<CamTargetSettings>();
        _cinematicCamera = IOC.Services.Resolve<SystemCinematicCamera>();

        api.ChatCommands
             .Get("cam")
             .BeginSubCommand("target")
             .WithDescription(L("Description"))
             .WithArgs(api.ChatCommands.Parsers.WordRange("option", "set", "face", "tp", "clear"))
             .HandleWith(OnCmdCamTarget);
    }

    private TextCommandResult OnCmdCamTarget(TextCommandCallingArgs args)
    {
        var action = (string)args.Parsers[0].GetValue();
        if (action == "set")
        {
            SetTarget(ApiEx.ClientMain.EntityPlayer.Pos);
            return TextCommandResult.Success(L("TargetSet"));
        }

        if (action == "face")
        {
            if (!_settings.TargetSet)
            {
                return TextCommandResult.Error(L("TargetNotSet"));
            }
            FacePosition(_settings.TargetPos.XYZ);
            return TextCommandResult.Success(L("TargetFacing"));
        }

        if (action == "tp")
        {
            if (!_settings.TargetSet)
            {
                return TextCommandResult.Error(L("TargetNotSet"));
            }
            TeleportTo(_settings.TargetPos);
            return TextCommandResult.Success(L("TargetTeleport"));
        }

        if (action == "clear")
        {
            ClearTarget();
            return TextCommandResult.Success(L("TargetCleared"));
        }

        return TextCommandResult.Success();
    }

    public static void ClearTarget()
    {
        _settings.TargetPos = null;
        _settings.TargetSet = false;
    }

    public static void TeleportTo(EntityPos targetPos)
    {
        ApiEx.ClientMain.TeleportToPoint(targetPos);
    }

    public static void FacePosition(Vec3d targetPos)
    {
        var entityPos = ApiEx.ClientMain.EntityPlayer.Pos;
        var pos = entityPos.DirectlyFace(targetPos);
        ApiEx.ClientMain.TeleportToPoint(pos);
    }

    public static void SetTarget(EntityPos targetPos)
    {
        _settings.TargetPos = targetPos;
        _settings.TargetSet = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SystemCinematicCamera), "PrecalcCurrentPoint")]
    public static void Harmony_SystemCinematicCamera_PrecalcCurrentPoint_Postfix(
        ref CameraPoint[] ___points,
        ref double[] ___pointsPitch,
        ref double[] ___pointsYaw,
        ref double[] ___pointsRoll)
    {
        if (!_settings.TargetSet) return;
        if (_settings.TargetPos is null) return;

        for (int j = 0; j < 4; j++)
        {
            var cameraPosition = ___points[j].ToEntityPos();
            var pos = cameraPosition.DirectlyFace(_settings.TargetPos.XYZ);
            var previousYaw = ___pointsYaw[Math.Max(j - 1, 0)];
            var currentYaw = pos.Yaw;

            ___pointsPitch[j] = pos.Pitch;
            ___pointsYaw[j] = FixYaw(previousYaw, currentYaw);
            ___pointsRoll[j] = pos.Roll;
        }
    }

    private static double FixYaw(double previousYaw, double currentYaw)
    {
        var delta = currentYaw - previousYaw;
        return delta > GameMath.PI
            ? currentYaw - GameMath.TWOPI
            : delta < -GameMath.PI
                ? currentYaw + GameMath.TWOPI
                : currentYaw;
    }
}