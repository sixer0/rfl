#nullable enable
using RFL.Data;
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Game.UI;
using RFL.Sim;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFL.Game.GreyBox
{
    /// <summary>
    /// Fight HUD (reads <see cref="MatchState"/> only): HP, round-win pips, timer, stamina and OVERLOAD gauges,
    /// the 5-part figure, status and COUNTER callouts, combo counter, round banners, frame data and controls.
    /// </summary>
    public sealed class FightHud
    {
        static readonly string[] PartClass =
        {
            "part-head",
            "part-torso",
            "part-arm-l",
            "part-arm-r",
            "part-legs",
        };

        sealed class SideView
        {
            public VisualElement HpFill = null!,
                StaminaFill = null!,
                OverloadFill = null!;
            public Label Hp = null!;
            public LocLabel Combo = null!,
                Status = null!,
                Counter = null!;
            public readonly VisualElement[] Pips = new VisualElement[Simulation.WinsNeeded];
            public readonly VisualElement[] Parts = new VisualElement[(int)Part.Count];
            public readonly VisualElement[] PartFills = new VisualElement[(int)Part.Count];
            public float ComboUntil,
                CounterUntil;
            public FighterDef Body;
        }

        readonly SideView _s1 = new SideView();
        readonly SideView _s2 = new SideView();
        readonly Label _timer;
        readonly LocLabel _banner,
            _subBanner,
            _frames,
            _dummy,
            _controls;
        readonly VisualElement _bottom;
        readonly MatchMode _mode;
        readonly string _p2HeadKey;
        readonly StarterTypeDef _def1,
            _def2;
        float _subUntil;
        string _bannerState = "";
        DeviceKind _lastKind = (DeviceKind)(-1);
        MoveId _lastMove;
        int _lastFrame;

        public FightHud(
            VisualElement root,
            MatchConfig cfg,
            StarterTypeDef def1,
            StarterTypeDef def2,
            Simulation sim
        )
        {
            _mode = cfg.Mode;
            _def1 = def1;
            _def2 = def2;
            _p2HeadKey = P2HeadKey(cfg.Mode);
            root.Q<LocLabel>("mode").Key = GreyBoxController.ModeKey(cfg.Mode);
            root.Q<LocLabel>("p1-head").Key = "ui.select.p1";
            root.Q<LocLabel>("p2-head").Key = _p2HeadKey;
            root.Q<LocLabel>("p1-robot").Key = def1.NameKey;
            root.Q<LocLabel>("p2-robot").Key = def2.NameKey;
            Bind(root, "p1", _s1, sim.Fighter(1));
            Bind(root, "p2", _s2, sim.Fighter(2));
            _timer = root.Q<Label>("timer");
            _banner = root.Q<LocLabel>("banner");
            _subBanner = root.Q<LocLabel>("sub-banner");
            _frames = root.Q<LocLabel>("frames");
            _dummy = root.Q<LocLabel>("dummy");
            _controls = root.Q<LocLabel>("controls");
            _bottom = root.Q("hud-bottom");
            _dummy.style.display =
                _mode == MatchMode.Training ? DisplayStyle.Flex : DisplayStyle.None;
            _banner.style.visibility = Visibility.Hidden;
            _subBanner.style.visibility = Visibility.Hidden;
        }

        public static string P2HeadKey(MatchMode m) =>
            m switch
            {
                MatchMode.VersusPlayer => "ui.select.p2",
                MatchMode.Training => "ui.select.challenger",
                _ => "ui.select.cpu",
            };

        public static string PartKey(Part p) =>
            p switch
            {
                Part.Head => "ui.part.head",
                Part.Torso => "ui.part.torso",
                Part.ArmL => "ui.part.arm_l",
                Part.ArmR => "ui.part.arm_r",
                _ => "ui.part.legs",
            };

        public static string StateKey(PartState s) =>
            s switch
            {
                PartState.Broken => "ui.part.broken",
                PartState.Damaged => "ui.part.damaged",
                _ => "ui.part.ok",
            };

        /// <summary>"P1 STANDARD" or "PENANTANG POWER".</summary>
        public string SideName(int side) =>
            side == 1
                ? Loc.Format("ui.fight.side_name", Loc.Get("ui.select.p1"), Loc.Get(_def1.NameKey))
                : Loc.Format("ui.fight.side_name", Loc.Get(_p2HeadKey), Loc.Get(_def2.NameKey));

        static void Bind(VisualElement root, string p, SideView v, FighterDef body)
        {
            v.Body = body;
            v.HpFill = root.Q($"{p}-hp-fill");
            v.StaminaFill = root.Q($"{p}-stamina-fill");
            v.OverloadFill = root.Q($"{p}-overload-fill");
            v.Hp = root.Q<Label>($"{p}-hp");
            v.Combo = root.Q<LocLabel>($"{p}-combo");
            v.Status = root.Q<LocLabel>($"{p}-status");
            v.Counter = root.Q<LocLabel>($"{p}-counter");
            v.Combo.style.visibility = Visibility.Hidden;
            v.Counter.style.visibility = Visibility.Hidden;
            v.Status.style.visibility = Visibility.Hidden;
            var pips = root.Q($"{p}-pips");
            for (int i = 0; i < v.Pips.Length; i++)
            {
                v.Pips[i] = new VisualElement();
                v.Pips[i].AddToClassList("round-pip");
                pips.Add(v.Pips[i]);
            }
            var figure = root.Q($"{p}-parts");
            for (int i = 0; i < v.Parts.Length; i++)
            {
                var part = new VisualElement();
                part.AddToClassList("part");
                part.AddToClassList(PartClass[i]);
                var fill = new VisualElement();
                fill.AddToClassList("part-fill");
                part.Add(fill);
                figure.Add(part);
                v.Parts[i] = part;
                v.PartFills[i] = fill;
            }
        }

        public void ShowCombo(byte side, int hits, int damage, bool counter)
        {
            var v = side == 1 ? _s1 : _s2;
            v.Combo.Key = hits == 1 ? "ui.fight.combo_one" : "ui.fight.combo";
            v.Combo.SetArgs(hits, damage);
            v.Combo.style.visibility = Visibility.Visible;
            v.ComboUntil = Time.unscaledTime + 1.6f;
            if (counter)
            {
                v.Counter.style.visibility = Visibility.Visible;
                v.CounterUntil = Time.unscaledTime + 1.2f;
            }
        }

        public void ShowSub(string key, float seconds, params object[] args)
        {
            _subBanner.Key = key;
            _subBanner.SetArgs(args);
            _subBanner.style.visibility = Visibility.Visible;
            _subUntil = Time.unscaledTime + seconds;
        }

        public void SetBottomVisible(bool visible) =>
            _bottom.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        public void Update(
            in MatchState s,
            Simulation sim,
            DummyAction dummyAction,
            bool showFrames,
            DeviceKind kind,
            bool showBanner = true
        )
        {
            UpdateSide(_s1, s.P1, s.WinsP1);
            UpdateSide(_s2, s.P2, s.WinsP2);
            _timer.text = s.TimerSeconds.ToString(
                System.Globalization.CultureInfo.InvariantCulture
            );
            float now = Time.unscaledTime;
            if (now > _subUntil)
                _subBanner.style.visibility = Visibility.Hidden;
            UpdateBanner(s, showBanner);

            if (kind != _lastKind)
            {
                _lastKind = kind;
                _controls.Key = "ui.fight.controls";
                _controls.SetArgs(
                    Glyphs.LeftPunch(kind),
                    Glyphs.RightPunch(kind),
                    Glyphs.Walk(kind),
                    Glyphs.LeftKick(kind),
                    Glyphs.RightKick(kind)
                );
            }
            if (_mode == MatchMode.Training)
            {
                _dummy.Key = "ui.fight.dummy";
                _dummy.SetArgs(Loc.Get(DummyKey(dummyAction)));
            }

            _frames.style.display = showFrames ? DisplayStyle.Flex : DisplayStyle.None;
            if (s.P1.Mode == RobotMode.Attack)
            {
                _lastMove = s.P1.Move;
                _lastFrame = s.P1.MoveFrame;
            }
            if (showFrames && _lastMove != MoveId.None)
            {
                var m = sim.Move(_lastMove);
                _frames.Key = "ui.fight.frames";
                _frames.SetArgs(
                    Loc.Get(m.NameKey),
                    _lastFrame,
                    m.Total,
                    m.Startup + 1,
                    m.Active,
                    m.Recovery
                );
            }
        }

        public static string DummyKey(DummyAction a) =>
            a switch
            {
                DummyAction.Block => "ui.dummy.block",
                DummyAction.CrouchBlock => "ui.dummy.crouch_block",
                _ => "ui.dummy.stand",
            };

        void UpdateSide(SideView v, in RobotState r, int wins)
        {
            var b = v.Body;
            v.HpFill.style.width = Length.Percent(100f * r.Hp / b.MaxHp);
            v.Hp.text = Loc.Number(r.Hp);
            v.StaminaFill.style.width = Length.Percent(
                100f * r.Stamina / Mathf.Max(1, b.MaxStamina)
            );
            v.OverloadFill.style.width = Length.Percent(
                r.Mode == RobotMode.Overload ? 100f : r.Overload / 10f
            );
            v.OverloadFill.EnableInClassList("is-full", r.Mode == RobotMode.Overload);
            for (int i = 0; i < v.Pips.Length; i++)
                v.Pips[i].EnableInClassList("is-won", i < wins);
            for (int i = 0; i < v.Parts.Length; i++)
            {
                int hp = r.GetPart((Part)i);
                var st = StatusRules.StateOf(hp, b.PartMax);
                v.PartFills[i].style.height = Length.Percent(
                    100f * Mathf.Max(0, hp) / Mathf.Max(1, b.PartMax)
                );
                v.Parts[i].EnableInClassList("is-damaged", st == PartState.Damaged);
                v.Parts[i].EnableInClassList("is-broken", st == PartState.Broken);
            }
            float now = Time.unscaledTime;
            if (now > v.ComboUntil)
                v.Combo.style.visibility = Visibility.Hidden;
            if (now > v.CounterUntil)
                v.Counter.style.visibility = Visibility.Hidden;
            string? status =
                r.Mode == RobotMode.Groggy ? "ui.fight.groggy"
                : r.Mode == RobotMode.Overload ? "ui.fight.overload"
                : null;
            v.Status.style.visibility = status != null ? Visibility.Visible : Visibility.Hidden;
            if (status != null && v.Status.Key != status)
                v.Status.Key = status;
            v.Status.EnableInClassList("is-groggy", r.Mode == RobotMode.Groggy);
        }

        void UpdateBanner(in MatchState s, bool show)
        {
            string? key = null;
            object[] args = System.Array.Empty<object>();
            switch (show ? s.Phase : MatchPhase.CornerBreak)
            {
                case MatchPhase.Intro:
                    if (s.PhaseFrames > Simulation.FightBannerFrames)
                    {
                        key = "ui.fight.round";
                        args = new object[] { s.Round };
                    }
                    else
                    {
                        key = "ui.fight.fight";
                    }
                    break;
                case MatchPhase.RoundOver:
                    if (Simulation.RoundOverFrames - s.PhaseFrames < 60)
                        key = s.RoundEndReason == RoundEnd.Time ? "ui.fight.time" : "ui.fight.ko";
                    else if (s.RoundWinner == 3)
                        key = "ui.fight.round_draw";
                    else
                    {
                        key = "ui.fight.round_win";
                        args = new object[] { SideName(s.RoundWinner), s.Round };
                    }
                    break;
            }
            string state = key == null ? "" : key + string.Join("|", args);
            if (state == _bannerState)
                return;
            _bannerState = state;
            _banner.style.visibility = key != null ? Visibility.Visible : Visibility.Hidden;
            if (key == null)
                return;
            _banner.EnableInClassList(
                "is-long",
                key == "ui.fight.round_win" || key == "ui.fight.round_draw"
            );
            _banner.Key = key;
            _banner.SetArgs(args);
        }
    }
}
