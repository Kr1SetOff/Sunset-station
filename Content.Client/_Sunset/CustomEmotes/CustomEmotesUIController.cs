using System.Collections.Generic;
using System.IO;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using JetBrains.Annotations;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.ContentPack;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Client._Sunset.CustomEmotes;

// 🌇Sunset🌇 Lets players save their own custom "me"-style emotes locally and fire them with one click.
// Storage is purely client-side (a YAML file in user data) - nothing is synced to the server or other clients,
// the saved text is just typed into the existing "me" emote command when fired, same as if the player typed it.
// Uses Robust's own DataNode/YamlStream serialization rather than System.Text.Json, which the client
// content sandbox does not allow (JsonSerializer/JsonSerializerOptions are not on the type allowlist).
[UsedImplicitly]
public sealed partial class CustomEmotesUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private IResourceManager _resourceManager = default!;
    [Dependency] private IClientConsoleHost _consoleHost = default!;

    private static readonly ResPath SaveFile = new("/sunset_custom_emotes.yml");

    private MenuButton? CustomEmotesButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.CustomEmotesButton;

    private CustomEmotesWindow _window = default!;

    public List<CustomEmoteEntry> Entries { get; private set; } = new();

    public void OnStateEntered(GameplayState state)
    {
        LoadEntries();

        _window = UIManager.CreateWindow<CustomEmotesWindow>();
        _window.SetController(this);

        _window.OnClose += () =>
        {
            if (CustomEmotesButton != null)
                CustomEmotesButton.Pressed = false;
        };
        _window.OnOpen += () =>
        {
            if (CustomEmotesButton != null)
                CustomEmotesButton.Pressed = true;
        };
    }

    public void OnStateExited(GameplayState state)
    {
        _window.Dispose();
    }

    public void LoadButton()
        => CustomEmotesButton?.OnPressed += ButtonToggleWindow;

    public void UnloadButton()
        => CustomEmotesButton?.OnPressed -= ButtonToggleWindow;

    private void ButtonToggleWindow(BaseButton.ButtonEventArgs args)
        => ToggleWindow();

    private void ToggleWindow()
    {
        if (_window.IsOpen)
            _window.Close();
        else
            _window.OpenCentered();
    }

    public void AddEntry(string name, string text)
    {
        Entries.Add(new CustomEmoteEntry { Name = name, Text = text });
        SaveEntries();
    }

    public void UpdateEntry(int index, string name, string text)
    {
        if (index < 0 || index >= Entries.Count)
            return;

        Entries[index].Name = name;
        Entries[index].Text = text;
        SaveEntries();
    }

    public void RemoveEntry(int index)
    {
        if (index < 0 || index >= Entries.Count)
            return;

        Entries.RemoveAt(index);
        SaveEntries();
    }

    public void FireEntry(CustomEmoteEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Text))
            return;

        _consoleHost.ExecuteCommand($"me \"{CommandParsing.Escape(entry.Text)}\"");
    }

    private void LoadEntries()
    {
        Entries = new List<CustomEmoteEntry>();

        if (!_resourceManager.UserData.Exists(SaveFile))
            return;

        try
        {
            using var stream = _resourceManager.UserData.Open(SaveFile, FileMode.Open);
            using var reader = new StreamReader(stream);

            var yamlStream = new YamlStream();
            yamlStream.Load(reader);

            if (yamlStream.Documents.Count == 0)
                return;

            if (yamlStream.Documents[0].RootNode.ToDataNode() is not SequenceDataNode sequence)
                return;

            foreach (var node in sequence.Sequence)
            {
                if (node is not MappingDataNode map)
                    continue;

                var name = map.TryGet<ValueDataNode>("name", out var nameNode) ? nameNode.Value : string.Empty;
                var text = map.TryGet<ValueDataNode>("text", out var textNode) ? textNode.Value : string.Empty;

                if (name.Length == 0 && text.Length == 0)
                    continue;

                Entries.Add(new CustomEmoteEntry { Name = name, Text = text });
            }
        }
        catch
        {
            Entries = new List<CustomEmoteEntry>();
        }
    }

    private void SaveEntries()
    {
        var sequence = new SequenceDataNode();
        foreach (var entry in Entries)
        {
            var map = new MappingDataNode();
            map.Add("name", new ValueDataNode(entry.Name));
            map.Add("text", new ValueDataNode(entry.Text));
            sequence.Add(map);
        }

        using var stream = _resourceManager.UserData.Open(SaveFile, FileMode.Create);
        using var writer = new StreamWriter(stream);
        sequence.Write(writer);
    }
}
