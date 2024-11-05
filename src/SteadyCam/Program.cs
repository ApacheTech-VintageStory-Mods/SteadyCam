using ApacheTech.Common.DependencyInjection.Abstractions;
using Gantry.Core.Hosting;
using Gantry.Services.HarmonyPatches.Hosting;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace ApacheTech.VintageMods.SteadyCam;

/// <summary>
///     Entry-point for the mod. This class will configure and build the IOC Container, and Service list for the rest of the mod.
///     
///     Registrations performed within this class should be global scope; by convention, features should aim to be as stand-alone as they can be.
/// </summary>
/// <remarks>
///     Only one derived instance of this class should be added to any single mod within
///     the VintageMods domain. This class will enable Dependency Injection, and add all
///     the domain services. Derived instances should only have minimal functionality, 
///     instantiating, and adding Application specific services to the IOC Container.
/// </remarks>
/// <seealso cref="ModHost" />
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed class Program() : ModHost(
#if DEBUG
    debugMode: true
#else
    debugMode: false
#endif
)
{
    protected override void ConfigureUniversalModServices(IServiceCollection services, ICoreAPI api)
    {
        services.AddHarmonyPatchingService(api, o => o.AutoPatchModAssembly = true);
    }
}