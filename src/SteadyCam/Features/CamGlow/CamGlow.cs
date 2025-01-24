// ReSharper disable InconsistentNaming

namespace ApacheTech.VintageMods.SteadyCam.Features.CamGlow;

[HarmonyClientSidePatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CamGlow : ClientModSystem, IClientServiceRegistrar
{
    private static CamGlowSettings _settings;

    public void ConfigureClientModServices(IServiceCollection services, ICoreClientAPI capi)
    {
        services.AddFeatureWorldSettings<CamGlowSettings>();
    }

    private static string L(string path, params string[] args)
    {
        return LangEx.FeatureString("CamGlow", path, args);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _settings = IOC.Services.Resolve<CamGlowSettings>();

        var parsers = api.ChatCommands.Parsers;
        var camCommand = api.ChatCommands.Get("cam");

        camCommand.BeginSubCommand("glow").WithDescription(L("Enabled")).HandleWith(_ =>
        {
            _settings.Enabled = !_settings.Enabled;
            return TextCommandResult.Success();
        });

        camCommand.BeginSubCommand("glow-brightness").WithDescription(L("Brightness")).WithArgs(parsers.FloatRange("brightness", 0f, 31f)).HandleWith(args =>
        {
            var brightness = (float)args.Parsers[0].GetValue();
            _settings.Brightness = brightness;
            return TextCommandResult.Success();
        });

        camCommand.BeginSubCommand("glow-colour").WithDescription(L("Colour")).WithArgs(parsers.Color("colour")).HandleWith(args =>
        {
            var colour = (Color)args.Parsers[0].GetValue();
            _settings.Colour = colour;
            return TextCommandResult.Success();
        });
    }

    public override void Dispose()
    {
        _settings.Enabled = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityPlayer), "LightHsv", MethodType.Getter)]
    public static bool Patch_Block_LightHsv_Prefix(EntityPlayer __instance, ref byte[] __result)
    {
        if (!_settings.Enabled) return true;
        var hsv = _settings.Colour.ToHsvByteArray();

        var lightLevel = __instance.GetLightLevelAtPlayerPosition();

        var array = new byte[]
        {
            (byte)(hsv[0] / 4),
            (byte)(hsv[1] / 32),
            (byte)(_settings.Brightness * (1f - lightLevel / 32f))
        };

        if (__result == null)
        {
            __result = array;
            return false;
        }

        var num = (float)(array[2] + __result[2]);
        var num2 = __result[2] / num;
        array[0] = (byte)(__result[0] * num2 + array[0] * (1f - num2));
        array[1] = (byte)(__result[1] * num2 + array[1] * (1f - num2));
        array[2] = Math.Max(__result[2], array[2]);

        __result = array;
        return false;
    }
}