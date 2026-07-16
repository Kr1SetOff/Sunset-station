using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Shared.Dataset;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._Sunset.RoundEnd;

/// <summary>
/// Full-screen movie-style "credits roll" shown before the round-end summary window: sponsors by
/// tier, then departments, then antagonists by type, then ghosts, then admins, then the project logo,
/// all laid out as one continuous document that scrolls upward at a constant speed, then fades out
/// to reveal the game again.
/// </summary>
public sealed class RoundEndCreditsControl : Control
{
    private readonly IEntityManager _entityManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly IRobustRandom _random;

    private const float ScrollSpeed = 65f; // pixels/second
    private const float FinalFadeOutDuration = 1.5f;

    private readonly LayoutContainer _viewport;
    private readonly BoxContainer _creditsContent;

    private float _elapsed;
    private bool _finishing;
    private float _finishingElapsed;

    private readonly VectorFont _headerFont;
    private readonly VectorFont _nameFont;
    private readonly VectorFont _jokeFont;

    /// <summary>
    /// Raised once the whole sequence (including the final fade-out) has finished.
    /// </summary>
    public event Action? Finished;

    public RoundEndCreditsControl(RoundEndMessageEvent message, IEntityManager entityManager,
        IPrototypeManager prototypeManager, IResourceCache resourceCache, IRobustRandom random)
    {
        _entityManager = entityManager;
        _prototypeManager = prototypeManager;
        _random = random;

        _headerFont = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/PlayfairDisplay/PlayfairDisplay-Bold.ttf"), 34);
        _nameFont = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 14);
        _jokeFont = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Italic.ttf"), 10);

        MouseFilter = MouseFilterMode.Stop;
        LayoutContainer.SetAnchorPreset(this, LayoutContainer.LayoutPreset.Wide);

        AddChild(new PanelContainer
        {
            PanelOverride = new StyleBoxFlat { BackgroundColor = new Color(0f, 0f, 0f, 0.96f) },
        }.WithWide());

        _viewport = new LayoutContainer { RectClipContent = true };
        LayoutContainer.SetAnchorPreset(_viewport, LayoutContainer.LayoutPreset.Wide);
        AddChild(_viewport);

        _creditsContent = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
        };
        _viewport.AddChild(_creditsContent);
        LayoutContainer.SetAnchorPreset(_creditsContent, LayoutContainer.LayoutPreset.TopWide);

        BuildContent(message, resourceCache);
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_finishing)
        {
            _finishingElapsed += args.DeltaSeconds;
            Modulate = new Color(1, 1, 1, Math.Clamp(1f - _finishingElapsed / FinalFadeOutDuration, 0f, 1f));
            if (_finishingElapsed >= FinalFadeOutDuration)
                Finished?.Invoke();
            return;
        }

        _elapsed += args.DeltaSeconds;

        var viewportHeight = Height > 0 ? Height : 720f;
        var topY = viewportHeight - ScrollSpeed * _elapsed;
        LayoutContainer.SetMarginTop(_creditsContent, topY);

        // Once the whole document (whatever its height turns out to be) has scrolled past the top
        // of the screen, start the final fade. Height is re-measured every frame so this doesn't
        // depend on layout having settled by the time the animation starts.
        var contentHeight = _creditsContent.Height;
        if (contentHeight > 0 && topY + contentHeight < 0)
        {
            _finishing = true;
            _finishingElapsed = 0;
        }
    }

    private void BuildContent(RoundEndMessageEvent message, IResourceCache resourceCache)
    {
        // 1. Sponsors, tier 5 down to tier 1. Tiers with nobody in them contribute nothing -
        // no header, no wasted scroll time.
        for (var tier = 5; tier >= 1; tier--)
        {
            var atTier = message.AllPlayersEndInfo.Where(p => p.SponsorTier == tier).ToArray();
            if (atTier.Length == 0)
                continue;

            AddSection(
                Loc.GetString("round-end-credits-sponsor-tier", ("tier", tier)),
                atTier.Select(MakePlayerCard).ToList());
        }

        // 2. Departments, in UI weight order. Jobs are matched to their primary department first,
        // falling back to any department that lists the job, so e.g. a CE shows under Engineering.
        var departments = _prototypeManager.EnumeratePrototypes<DepartmentPrototype>().ToList();
        departments.Sort(DepartmentUIComparer.Instance);

        var jobToDept = new Dictionary<string, DepartmentPrototype>();
        foreach (var dept in departments.Where(d => d.Primary))
            foreach (var role in dept.Roles)
                jobToDept.TryAdd(role.Id, dept);
        foreach (var dept in departments.Where(d => !d.Primary))
            foreach (var role in dept.Roles)
                jobToDept.TryAdd(role.Id, dept);

        var deptGroups = new Dictionary<string, List<RoundEndMessageEvent.RoundEndPlayerInfo>>();
        foreach (var p in message.AllPlayersEndInfo)
        {
            if (p.Antag || p.Observer)
                continue;

            var job = p.JobPrototypes.FirstOrDefault();
            if (job == null || !jobToDept.TryGetValue(job, out var dept))
                continue;

            if (!deptGroups.TryGetValue(dept.ID, out var list))
                deptGroups[dept.ID] = list = new List<RoundEndMessageEvent.RoundEndPlayerInfo>();
            list.Add(p);
        }

        foreach (var dept in departments)
        {
            if (!deptGroups.TryGetValue(dept.ID, out var members) || members.Count == 0)
                continue;

            AddSection(Loc.GetString(dept.Name), members.Select(MakePlayerCard).ToList(), dept.Color);
        }

        // 3. Antagonists, grouped by their antagonist role prototype (falls back to the localized
        // role name for anything that somehow has no prototype id attached).
        var antagGroups = message.AllPlayersEndInfo
            .Where(p => p.Antag)
            .GroupBy(p => p.AntagPrototypes.FirstOrDefault() ?? p.Role)
            .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase);

        foreach (var group in antagGroups)
        {
            var header = _prototypeManager.TryIndex<AntagPrototype>(group.Key, out var antagProto)
                ? Loc.GetString(antagProto.Name)
                : Loc.GetString(group.First().Role);
            AddSection(header, group.Select(MakePlayerCard).ToList());
        }

        // 4. Ghosts/observers - just names, no portraits.
        var ghosts = message.AllPlayersEndInfo.Where(p => p.Observer).ToArray();
        if (ghosts.Length > 0)
            AddNameListSection(Loc.GetString("round-end-credits-ghosts"), ghosts.Select(p => p.PlayerOOCName));

        // 5. Admins.
        if (message.AllAdminsEndInfo.Length > 0)
            AddSection(Loc.GetString("round-end-credits-admins"), message.AllAdminsEndInfo.Select(MakeAdminCard).ToList());

        // 6. Project logo, always last.
        AddLogoSection(resourceCache);
    }

    private void AddSection(string header, List<Control> cards, Color? headerColor = null)
    {
        var vbox = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 48, 0, 0),
        };

        vbox.AddChild(new Label
        {
            Text = header,
            FontOverride = _headerFont,
            FontColorOverride = headerColor ?? Color.White,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        });

        var columns = Math.Clamp((int) Math.Ceiling(Math.Sqrt(cards.Count)), 3, 6);
        var grid = new GridContainer
        {
            Columns = columns,
            HorizontalAlignment = HAlignment.Center,
        };
        foreach (var card in cards)
            grid.AddChild(card);

        vbox.AddChild(grid);
        _creditsContent.AddChild(vbox);
    }

    private void AddNameListSection(string header, IEnumerable<string> names)
    {
        var vbox = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 48, 0, 0),
        };

        vbox.AddChild(new Label
        {
            Text = header,
            FontOverride = _headerFont,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16),
        });

        vbox.AddChild(new Label
        {
            Text = string.Join(", ", names),
            FontOverride = _nameFont,
            HorizontalAlignment = HAlignment.Center,
            Align = Label.AlignMode.Center,
            MaxWidth = 900,
        });

        _creditsContent.AddChild(vbox);
    }

    private void AddLogoSection(IResourceCache resourceCache)
    {
        var vbox = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 64, 0, 96),
        };

        var logo = resourceCache.GetResource<TextureResource>("/Textures/Logo/logo.png");
        vbox.AddChild(new TextureRect
        {
            Texture = logo,
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            SetSize = new Vector2(256, 256),
            HorizontalAlignment = HAlignment.Center,
        });

        _creditsContent.AddChild(vbox);
    }

    private Control MakePlayerCard(RoundEndMessageEvent.RoundEndPlayerInfo info)
    {
        var box = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(14, 10),
        };

        if (info.PlayerNetEntity != null)
        {
            box.AddChild(new SpriteView(info.PlayerNetEntity.Value, _entityManager)
            {
                OverrideDirection = Direction.South,
                SetSize = new Vector2(64, 64),
                HorizontalAlignment = HAlignment.Center,
            });
        }

        box.AddChild(new Label
        {
            Text = info.PlayerOOCName,
            FontOverride = _nameFont,
            HorizontalAlignment = HAlignment.Center,
        });

        var joke = PickJoke();
        if (joke != null)
        {
            box.AddChild(new Label
            {
                Text = joke,
                FontOverride = _jokeFont,
                FontColorOverride = Color.Gray,
                HorizontalAlignment = HAlignment.Center,
                Align = Label.AlignMode.Center,
                MaxWidth = 140,
            });
        }

        return box;
    }

    private Control MakeAdminCard(RoundEndMessageEvent.RoundEndAdminInfo info)
    {
        var box = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(14, 10),
        };

        if (info.Rank != null)
        {
            box.AddChild(new Label
            {
                Text = $"({info.Rank})",
                FontOverride = _jokeFont,
                FontColorOverride = Color.Gold,
                HorizontalAlignment = HAlignment.Center,
            });
        }

        if (info.PlayerNetEntity != null)
        {
            box.AddChild(new SpriteView(info.PlayerNetEntity.Value, _entityManager)
            {
                OverrideDirection = Direction.South,
                SetSize = new Vector2(64, 64),
                HorizontalAlignment = HAlignment.Center,
            });
        }

        box.AddChild(new Label
        {
            Text = info.Ckey,
            FontOverride = _nameFont,
            HorizontalAlignment = HAlignment.Center,
        });

        return box;
    }

    private string? PickJoke()
    {
        if (!_prototypeManager.TryIndex<LocalizedDatasetPrototype>("RoundEndCreditsJokePhrases", out var dataset)
            || dataset.Values.Count == 0)
            return null;

        return Loc.GetString(_random.Pick(dataset.Values));
    }
}

file static class PanelContainerExt
{
    public static PanelContainer WithWide(this PanelContainer panel)
    {
        LayoutContainer.SetAnchorPreset(panel, LayoutContainer.LayoutPreset.Wide);
        return panel;
    }
}
