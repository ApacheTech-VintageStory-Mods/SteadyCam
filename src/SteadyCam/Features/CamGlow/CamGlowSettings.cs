using Gantry.Services.FileSystem.Configuration.Abstractions;

namespace ApacheTech.VintageMods.SteadyCam.Features.CamGlow;

/// <summary>
///     Configuration settings for the camera glow effect.
/// </summary>
public class CamGlowSettings : FeatureSettings
{
    /// <summary>
    ///     The colour of the camera glow effect.
    /// </summary>
    public Color Colour { get; set; } = Color.White;

    /// <summary>
    ///     The brightness level of the camera glow effect.
    /// </summary>
    public float Brightness { get; set; } = 31f;

    /// <summary>
    ///     A value indicating whether the camera glow effect is enabled.
    /// </summary>
    public bool Enabled { get; set; }
}