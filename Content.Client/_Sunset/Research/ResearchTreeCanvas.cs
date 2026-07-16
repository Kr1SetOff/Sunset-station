using System.Linq;
using Content.Shared.Research.Components;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._Sunset.Research;

/// <summary>
/// 🌇Sunset🌇 - draws prerequisite lines between the ResearchTreeNodeControl children placed on it,
/// turning the plain draggable canvas into a branching tech tree. Ported from Goob-Station's
/// ResearchesContainerPanel concept, adapted to draw over our own TechnologyPrototype data and to
/// color a line green once both ends are researched.
/// </summary>
public sealed partial class ResearchTreeCanvas : LayoutContainer
{
    protected override void Draw(DrawingHandleScreen handle)
    {
        var nodes = Children.OfType<ResearchTreeNodeControl>().ToList();

        foreach (var node in nodes)
        {
            if (node.Prototype.TechnologyPrerequisites.Count == 0)
                continue;

            foreach (var prereq in nodes)
            {
                if (!node.Prototype.TechnologyPrerequisites.Contains(prereq.Prototype.ID))
                    continue;

                var start = new System.Numerics.Vector2(prereq.PixelPosition.X + prereq.PixelWidth / 2f, prereq.PixelPosition.Y + prereq.PixelHeight / 2f);
                var end = new System.Numerics.Vector2(node.PixelPosition.X + node.PixelWidth / 2f, node.PixelPosition.Y + node.PixelHeight / 2f);

                var lineColor = prereq.Availability == ResearchAvailability.Researched && node.Availability != ResearchAvailability.Unavailable
                    ? Color.LimeGreen
                    : Color.Gray;

                if (System.MathF.Abs(start.Y - end.Y) > 0.5f)
                {
                    handle.DrawLine(start, new System.Numerics.Vector2(end.X, start.Y), lineColor);
                    handle.DrawLine(new System.Numerics.Vector2(end.X, start.Y), end, lineColor);
                }
                else
                {
                    handle.DrawLine(start, end, lineColor);
                }
            }
        }
    }
}
