using System.Numerics;
using JetBrains.Annotations;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Sunset.SponsorTier;

/// <summary>
/// 🌇Sunset🌇 - Colors a single glyph of a sponsor OOC tag/name based on wall-clock time and the glyph's
/// index within the tag, so that re-drawing the same chat line (which OutputPanel/RichTextEntry does every
/// frame - see RichTextEntry.Draw) produces a genuinely shifting color instead of the old static per-letter
/// palette. ChatManager.Sunset.cs wraps every glyph of "[Bracket] Name" in its own [sunsetglow t=X i=Y] node
/// (X = sponsor tier 1-5, Y = glyph index) so this only has to pick one color per glyph per frame.
/// </summary>
[UsedImplicitly]
public sealed partial class SunsetSponsorGlowTag : IMarkupTagHandler
{
    [Dependency] private IGameTiming _timing = default!;

    // Radians/sec the wave travels at, and how far apart (in radians) each glyph's phase is offset.
    private const float WaveSpeed = 2.2f;
    private const float PhaseStep = 0.35f;

    public string Name => "sunsetglow";

    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        if (!node.Attributes.TryGetValue("t", out var tierParam) || !tierParam.TryGetLong(out var tier) ||
            !node.Attributes.TryGetValue("i", out var idxParam) || !idxParam.TryGetLong(out var idx))
        {
            context.Color.Push(ColorTag.DefaultColor);
            return;
        }

        var t = (float)_timing.RealTime.TotalSeconds;
        var wave = MathF.Sin(t * WaveSpeed + (float)idx.Value * PhaseStep); // -1..1

        context.Color.Push(GetColor((int)tier.Value, wave, t, (float)idx.Value));
    }

    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        context.Color.Pop();
    }

    private static Color GetColor(int tier, float wave, float t, float idx)
    {
        var pulse = wave * 0.5f + 0.5f; // 0..1

        switch (tier)
        {
            case 1: // Zombie: shimmering marsh green
                return Color.FromHsl(new Vector4(Frac((146f + wave * 18f) / 360f), 0.55f, 0.36f + pulse * 0.14f, 1f));

            case 2: // Syndicate Agent: shimmering scarlet
                return Color.FromHsl(new Vector4(Frac((350f + wave * 12f) / 360f), 0.75f, 0.40f + pulse * 0.16f, 1f));

            case 3: // Vampire: red/black pulse
                return Color.InterpolateBetween(Color.Black, Color.Crimson, pulse);

            case 4: // SunSetter: full-spectrum travelling rainbow
                return Color.FromHsl(new Vector4(Frac(t * 0.12f + idx * 0.06f), 0.85f, 0.55f, 1f));

            case 5: // Ghost: flickering silver-white
            default:
                return Color.FromHsl(new Vector4(220f / 360f, 0.15f, 0.82f + pulse * 0.1f, 0.7f + pulse * 0.25f));
        }
    }

    private static float Frac(float x) => x - MathF.Floor(x);
}
