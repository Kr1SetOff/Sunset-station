using System;
using Content.Client._Starlight.Managers;
using Content.Client.Administration.Managers;
using Content.Client.Changelog;
using Content.Client.UserInterface.Systems.EscapeMenu;
using Content.Client.UserInterface.Systems.Guidebook;
using Content.Shared.CCVar;
using Robust.Client.Console;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Collections;
using Robust.Shared.Configuration;
using Robust.Shared.Localization;

namespace Content.Client.Info
{
    public sealed class LinkBanner : BoxContainer
    {
        private readonly IConfigurationManager _cfg;

        private ValueList<(CVarDef<string> cVar, Button button)> _infoLinks;

        public LinkBanner()
        {
            var buttons = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal
            };
            AddChild(buttons);

            var uriOpener = IoCManager.Resolve<IUriOpener>();
            _cfg = IoCManager.Resolve<IConfigurationManager>();
            var consoleHost = IoCManager.Resolve<IClientConsoleHost>();
            var rulesButton = new Button() {Text = Loc.GetString("server-info-rules-button")};
            rulesButton.OnPressed += args => new RulesAndInfoWindow().Open();
            buttons.AddChild(rulesButton);

            AddInfoButton("server-info-discord-button", CCVars.InfoLinksDiscord);
            AddInfoButton("server-info-website-button", CCVars.InfoLinksWebsite);
            AddInfoButton("server-info-wiki-button", CCVars.InfoLinksWiki);
            AddInfoButton("server-info-forum-button", CCVars.InfoLinksForum);
            AddInfoButton("server-info-telegram-button", CCVars.InfoLinksTelegram);

            // 🌇Sunset🌇 - was NullLink's _playerRoles.GetDiscordLink() (broken unless the external NullLink
            // cluster is configured); now opens our own self-contained Discord link window/OAuth flow instead.
            var button = new Button { Text = Loc.GetString("server-info-connect-discord-button") };
            button.OnPressed += _ => consoleHost.RemoteExecuteCommand(null, "linkdiscord");
            buttons.AddChild(button);

            var guidebookController = UserInterfaceManager.GetUIController<GuidebookUIController>();
            var guidebookButton = new Button() { Text = Loc.GetString("server-info-guidebook-button") };
            guidebookButton.OnPressed += _ =>
            {
                guidebookController.ToggleGuidebook();
            };
            buttons.AddChild(guidebookButton);

            var changelogButton = new ChangelogButton();
            changelogButton.OnPressed += args => UserInterfaceManager.GetUIController<ChangelogUIController>().ToggleWindow();
            buttons.AddChild(changelogButton);

            void AddInfoButton(string loc, CVarDef<string> cVar)
            {
                var button = new Button { Text = Loc.GetString(loc) };
                // 🌇Sunset🌇 - guard against a misconfigured (non-URI) CVar value crashing the client.
                button.OnPressed += _ =>
                {
                    try
                    {
                        uriOpener.OpenUri(_cfg.GetCVar(cVar));
                    }
                    catch (ArgumentException)
                    {
                    }
                };
                buttons.AddChild(button);
                _infoLinks.Add((cVar, button));
            }
        }

        protected override void EnteredTree()
        {
            // LinkBanner is constructed before the client even connects to the server due to UI refactor stuff.
            // We need to update these buttons when the UI is shown.

            base.EnteredTree();

            foreach (var (cVar, link) in _infoLinks)
            {
                link.Visible = _cfg.GetCVar(cVar) != "";
            }
        }
    }
}
