using Gantry.Services.FileSystem.Configuration.Abstractions;

namespace ApacheTech.VintageMods.SteadyCam.Features.CamTarget;

/// <summary>
///     Configuration settings for the camera target.
/// </summary>
public class CamTargetSettings : FeatureSettings<CamTargetSettings>
{
    /// <summary>
    ///     A value indicating whether a target position has been set.
    /// </summary>
    public bool TargetSet { get; set; }

    /// <summary>
    ///     The position of the camera target.
    /// </summary>
    public EntityPos TargetPos { get; set; }
}