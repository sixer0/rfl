#nullable enable
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Sim;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>CPU difficulty for Player vs CPU (PF-REQ-016).</summary>
    public sealed class DifficultyScreen : MenuScreen
    {
        MenuList _menu = null!;

        protected override VisualTreeAsset Template(MenuController app) => app.Difficulty;

        protected override void Build()
        {
            _menu = new MenuList(Q(Root, "desc"));
            _menu.Add(Root.Q("item-easy"), () => Pick(CpuLevel.Easy), "ui.difficulty.easy.desc");
            _menu.Add(
                Root.Q("item-normal"),
                () => Pick(CpuLevel.Normal),
                "ui.difficulty.normal.desc"
            );
            _menu.Add(Root.Q("item-hard"), () => Pick(CpuLevel.Hard), "ui.difficulty.hard.desc");
            _menu.Select((int)MatchRequest.LastCpu, sound: false);
        }

        void Pick(CpuLevel level)
        {
            MatchRequest.LastCpu = level;
            App.Push(new RobotSelectScreen(MatchMode.VersusCpu, level));
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
