#nullable enable
using RFL.Game.Input;
using RFL.Sim;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>Demo main menu: Versus, Training, Settings, Quit. Back opens the quit confirm.</summary>
    public sealed class MainMenuScreen : MenuScreen
    {
        MenuList _menu = null!;

        protected override VisualTreeAsset Template(MenuController app) => app.MainMenu;

        protected override void Build()
        {
            _menu = new MenuList(Q(Root, "desc"));
            _menu.Add(
                Root.Q("item-versus"),
                () => App.Push(new VersusScreen()),
                "ui.menu.versus.desc"
            );
            _menu.Add(
                Root.Q("item-training"),
                () => App.Push(new RobotSelectScreen(MatchMode.Training, CpuLevel.Normal)),
                "ui.menu.training.desc"
            );
            _menu.Add(
                Root.Q("item-settings"),
                () => App.Push(new SettingsScreen()),
                "ui.menu.settings.desc"
            );
            _menu.Add(Root.Q("item-quit"), AskQuit, "ui.menu.quit.desc");
        }

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            if (merged.Cancel)
            {
                AskQuit();
                return;
            }
            _menu.Tick(merged);
        }

        void AskQuit() => App.ShowModal(new ConfirmDialog("ui.quit.message", QuitGame));

        static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
