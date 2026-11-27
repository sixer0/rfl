#nullable enable
using RFL.Game.Input;
using RFL.Sim;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    public sealed class VersusScreen : MenuScreen
    {
        MenuList _menu = null!;

        protected override VisualTreeAsset Template(MenuController app) => app.Versus;

        protected override void Build()
        {
            _menu = new MenuList(Q(Root, "desc"));
            _menu.Add(
                Root.Q("item-pvp"),
                () => App.Push(new RobotSelectScreen(MatchMode.VersusPlayer, CpuLevel.Normal)),
                "ui.versus.pvp.desc"
            );
            _menu.Add(
                Root.Q("item-cpu"),
                () => App.Push(new DifficultyScreen()),
                "ui.versus.cpu.desc"
            );
        }

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            if (merged.Cancel)
            {
                Back();
                return;
            }
            _menu.Tick(merged);
        }
    }
}
