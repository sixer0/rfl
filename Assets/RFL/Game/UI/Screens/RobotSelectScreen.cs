#nullable enable
using System.Collections.Generic;
using RFL.Data;
using RFL.Game.Audio;
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Sim;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>
    /// Pick a starter type for each side. Each player moves only with their own device (PF-REQ-002).
    /// Versus Player: P2 joins by pressing confirm on another device. Versus CPU and Training:
    /// P1 picks their own robot, then the CPU's or the dummy's.
    /// </summary>
    public sealed class RobotSelectScreen : MenuScreen
    {
        public const string FightScene = "GreyBox";
        const float StartDelay = 0.6f;

        sealed class Slot
        {
            public InputDevice? Device;
            public bool Joined;
            public bool Ready;
            public int Index;
            public bool Computer;
            public VisualElement Panel = null!;
            public LocLabel Head = null!;
            public Label DeviceName = null!;
            public LocLabel State = null!;
            public LocLabel ReadyBadge = null!;
        }

        readonly MatchMode _mode;
        readonly CpuLevel _cpu;
        readonly Slot _p1 = new Slot();
        readonly Slot _p2 = new Slot();
        readonly List<VisualElement> _cards = new List<VisualElement>();
        LocLabel _title = null!;
        float _startAt = -1f;

        public RobotSelectScreen(MatchMode mode, CpuLevel cpu)
        {
            _mode = mode;
            _cpu = cpu;
        }

        public MatchMode Mode => _mode;
        public bool P1Ready => _p1.Ready;
        public bool P2Joined => _p2.Joined;
        public bool P2Ready => _p2.Ready;

        protected override VisualTreeAsset Template(MenuController app) => app.RobotSelect;

        public override PromptSet Prompts => PromptSet.Change | PromptSet.Confirm | PromptSet.Back;

        protected override void Build()
        {
            _title = Q(Root, "title");
            BindSlot(_p1, Root.Q("slot-p1"));
            BindSlot(_p2, Root.Q("slot-p2"));
            var host = Root.Q("cards");
            for (int i = 0; i < StarterTypes.All.Length; i++)
            {
                var card = BuildCard(StarterTypes.All[i]);
                int index = i;
                card.RegisterCallback<PointerEnterEvent>(_ => PointTo(index));
                card.RegisterCallback<ClickEvent>(_ => ClickCard(index));
                host.Add(card);
                _cards.Add(card);
            }

            _p1.Device = MatchRequest.P1Device;
            _p1.Joined = true;
            _p1.Index = Mathf.Clamp(MatchRequest.LastP1Index, 0, _cards.Count - 1); // 0 = STANDARD, the default
            _p2.Index = Mathf.Clamp(MatchRequest.LastP2Index, 0, _cards.Count - 1);
            _p2.Computer = _mode != MatchMode.VersusPlayer;
            _p2.Joined = _p2.Computer;
            if (
                !_p2.Computer
                && MatchRequest.P2Device != null
                && MatchRequest.P2Device.added
                && MatchRequest.P2Device != _p1.Device
            )
            {
                _p2.Device = MatchRequest.P2Device;
                _p2.Joined = true;
            }
            Refresh();
        }

        static void BindSlot(Slot s, VisualElement panel)
        {
            s.Panel = panel;
            s.Head = panel.Q<LocLabel>("head");
            s.DeviceName = panel.Q<Label>("device");
            s.State = panel.Q<LocLabel>("state");
            s.ReadyBadge = panel.Q<LocLabel>("ready");
        }

        static VisualElement BuildCard(StarterTypeDef def)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            card.Add(new LocLabel(def.NameKey, "card-name"));
            card.Add(new LocLabel(StarterTypes.ClassKey(def.Class), "card-class"));
            card.Add(NumberRow("ui.stat.hp", def.Hp));
            card.Add(NumberRow("ui.stat.stamina", def.Stamina));
            card.Add(PipRow("ui.stat.agility", def.Agility));
            card.Add(PipRow("ui.stat.power", def.Power));
            card.Add(PipRow("ui.stat.overload", def.OverloadResist));
            card.Add(new LocLabel("ui.stat.tech", "tech-title"));
            foreach (var move in def.TechMoveKeys)
                card.Add(new LocLabel(move, "tech-move"));
            return card;
        }

        static VisualElement NumberRow(string labelKey, int value)
        {
            var row = new VisualElement();
            row.AddToClassList("stat-row");
            row.Add(new LocLabel(labelKey, "stat-label"));
            var v = new LocLabel("ui.fmt.int", "stat-value");
            v.SetArgs(value);
            row.Add(v);
            return row;
        }

        static VisualElement PipRow(string labelKey, int value)
        {
            var row = new VisualElement();
            row.AddToClassList("stat-row");
            row.Add(new LocLabel(labelKey, "stat-label"));
            var pips = new VisualElement();
            pips.AddToClassList("pips");
            for (int i = 0; i < StarterTypes.StatPipMax; i++)
            {
                var pip = new VisualElement();
                pip.AddToClassList("pip");
                if (i < value)
                    pip.AddToClassList("pip-on");
                pips.Add(pip);
            }
            row.Add(pips);
            return row;
        }

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            if (_startAt > 0f)
            {
                if (Time.unscaledTime >= _startAt)
                    StartMatch();
                return;
            }
            foreach (var f in input.Frames)
            {
                if (_startAt > 0f || App.TopScreen != this)
                    return;
                HandleFrame(f);
            }
        }

        void HandleFrame(NavFrame f)
        {
            if (_p1.Device == null && (f.HasAction || f.Any))
                _p1.Device = f.Device;

            Slot slot;
            if (f.Device == _p1.Device || f.Device == null)
            {
                slot = _p2.Computer && _p1.Ready ? _p2 : _p1;
            }
            else if (_mode == MatchMode.VersusPlayer)
            {
                if (!_p2.Joined)
                {
                    if (f.Submit)
                    {
                        _p2.Device = f.Device;
                        _p2.Joined = true;
                        _p2.Ready = false;
                        AudioService.Play(UiCues.NavConfirm);
                        Refresh();
                    }
                    return;
                }
                if (f.Device != _p2.Device)
                    return;
                slot = _p2;
            }
            else
            {
                return; // a second device has nothing to do against the CPU
            }

            if (f.X != 0 && !slot.Ready)
            {
                slot.Index = (slot.Index + f.X + _cards.Count) % _cards.Count;
                AudioService.Play(UiCues.NavMove);
            }
            if (f.Submit && !slot.Ready)
            {
                slot.Ready = true;
                AudioService.Play(UiCues.NavConfirm);
            }
            else if (f.Cancel)
            {
                Cancel(slot);
                if (App.TopScreen != this)
                    return;
            }
            Refresh();
            CheckStart();
        }

        void Cancel(Slot slot)
        {
            AudioService.Play(UiCues.NavBack);
            if (slot.Ready)
            {
                slot.Ready = false;
                return;
            }
            if (slot == _p2 && _p2.Computer)
            {
                _p1.Ready = false;
                return;
            }
            if (slot == _p2)
            {
                _p2.Joined = false;
                _p2.Device = null;
                return;
            }
            App.Pop();
        }

        void PointTo(int index)
        {
            var slot = _p2.Computer && _p1.Ready ? _p2 : _p1;
            if (slot.Ready || slot.Index == index)
                return;
            slot.Index = index;
            AudioService.Play(UiCues.NavMove);
            Refresh();
        }

        void ClickCard(int index)
        {
            var slot = _p2.Computer && _p1.Ready ? _p2 : _p1;
            if (slot.Ready || _startAt > 0f)
                return;
            slot.Index = index;
            slot.Ready = true;
            AudioService.Play(UiCues.NavConfirm);
            Refresh();
            CheckStart();
        }

        void CheckStart()
        {
            if (_p1.Ready && _p2.Joined && _p2.Ready && _startAt < 0f)
                _startAt = Time.unscaledTime + StartDelay;
        }

        void StartMatch()
        {
            MatchRequest.Config = new MatchConfig
            {
                Mode = _mode,
                P1 = (StarterType)_p1.Index,
                P2 = (StarterType)_p2.Index,
                Cpu = _cpu,
                Seed = (uint)System.Environment.TickCount,
            };
            MatchRequest.P1Device = _p1.Device;
            MatchRequest.P2Device = _p2.Computer ? null : _p2.Device;
            MatchRequest.LastP1Index = _p1.Index;
            MatchRequest.LastP2Index = _p2.Index;
            SceneManager.LoadScene(FightScene);
        }

        void Refresh()
        {
            bool choosingForComputer = _p2.Computer && _p1.Ready;
            _title.Key =
                !choosingForComputer ? "ui.select.title"
                : _mode == MatchMode.Training ? "ui.select.choose_for_challenger"
                : "ui.select.choose_for_cpu";

            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].EnableInClassList("p1-on", _p1.Index == i);
                _cards[i]
                    .EnableInClassList(
                        "p2-on",
                        _p2.Joined && (!_p2.Computer || _p1.Ready || _p2.Ready) && _p2.Index == i
                    );
            }

            var kind =
                MenuInput.Instance != null ? MenuInput.Instance.LastKind : DeviceKind.Keyboard;
            _p1.Head.Key = "ui.select.p1";
            _p1.DeviceName.text = _p1.Device != null ? _p1.Device.displayName : string.Empty;
            SetState(_p1, kind);

            _p2.Head.Key =
                !_p2.Computer ? "ui.select.p2"
                : _mode == MatchMode.Training ? "ui.select.challenger"
                : "ui.select.cpu";
            _p2.DeviceName.text =
                _mode == MatchMode.Training ? Loc.Get(GreyBox.FightHud.DummyKey(MatchRequest.Dummy))
                : _p2.Computer ? Loc.Get(CpuKey(_cpu))
                : _p2.Device != null ? _p2.Device.displayName
                : string.Empty;
            if (!_p2.Joined)
            {
                _p2.State.Key = "ui.select.join";
                _p2.State.SetArgs(
                    Glyphs.Confirm(DeviceKind.Xbox) + " / " + Glyphs.Confirm(DeviceKind.Keyboard)
                );
            }
            else
            {
                SetState(_p2, kind);
            }
            _p1.Panel.EnableInClassList("is-ready", _p1.Ready);
            _p2.Panel.EnableInClassList("is-ready", _p2.Ready);
            _p2.Panel.EnableInClassList("is-waiting", !_p2.Joined);
        }

        static void SetState(Slot s, DeviceKind kind)
        {
            s.State.Key = StarterTypes.All[s.Index].NameKey;
            s.State.SetArgs();
        }

        public static string CpuKey(CpuLevel level) =>
            level switch
            {
                CpuLevel.Easy => "ui.difficulty.easy",
                CpuLevel.Hard => "ui.difficulty.hard",
                _ => "ui.difficulty.normal",
            };
    }
}
