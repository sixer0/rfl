#nullable enable
using RFL.Game.Audio;
using RFL.Game.Core;
using RFL.Game.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>"Press any button": the first device pressed becomes P1.</summary>
    public sealed class TitleScreen : MenuScreen
    {
        LocLabel _press = null!;
        IVisualElementScheduledItem _blink = null!;

        protected override VisualTreeAsset Template(MenuController app) => app.TitleScreen;

        public override PromptSet Prompts => PromptSet.None;

        protected override void Build()
        {
            _press = Q(Root, "press");
            Q(Root, "version").SetArgs(Application.version);
            _blink = Root.schedule.Execute(() => _press.ToggleInClassList("is-dim")).Every(600);
            Root.RegisterCallback<ClickEvent>(_ => Continue(Keyboard.current));
        }

        public override void Exit() => _blink?.Pause();

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            foreach (var f in input.Frames)
            {
                if (f.HasAction || f.Any)
                {
                    Continue(f.Device);
                    return;
                }
            }
        }

        void Continue(InputDevice device)
        {
            MatchRequest.P1Device = device;
            AudioService.Play(UiCues.NavConfirm);
            App.Replace(new MainMenuScreen());
        }
    }
}
