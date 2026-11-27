#nullable enable
using System.Collections.Generic;
using RFL.Data;
using RFL.Game.Audio;
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Game.UI;
using RFL.Sim;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RFL.Game.GreyBox
{
    /// <summary>
    /// The 1v1 fight scene: VS intro (challenger declaration) → rounds run by <see cref="Simulation"/> at 60 ticks
    /// a second → corner break between rounds → results. P2 is a human (Versus) or an idle robot that never attacks
    /// (Training, Versus CPU until the CPU exists). The view only reads <see cref="MatchState"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GreyBoxController : MonoBehaviour
    {
        public enum Stage
        {
            VsIntro,
            Fight,
            CornerBreak,
            Results,
        }

        public const string MenuScene = "Menu";
        public const string FightScene = "GreyBox";
        public const float VsIntroSeconds = 3.4f;
        public const float CornerBreakSeconds = 10f;
        const float TickSeconds = 1f / Simulation.TicksPerSecond;

        [Header("Overlays (UXML)")]
        public VisualTreeAsset PauseTemplate = null!;
        public VisualTreeAsset IntroTemplate = null!;
        public VisualTreeAsset CornerTemplate = null!;
        public VisualTreeAsset ResultsTemplate = null!;

        [Header("Robots")]
        public Transform P1Robot = null!;
        public Transform P2Robot = null!;

        [Tooltip(
            "Art prefab per starter type (STANDARD, POWER, SPEED); empty slots use the capsule."
        )]
        public GameObject[] RobotPrefabs = new GameObject[3];

        Simulation _sim = null!;
        MatchState _state;
        FightInput _in1 = null!;
        FightInput? _in2;
        float _accumulator;
        readonly Queue<InputFrame> _scriptedP1 = new Queue<InputFrame>();

        StarterTypeDef _def1 = null!,
            _def2 = null!;
        RobotView _v1 = null!,
            _v2 = null!;
        FightHud _hud = null!;
        VisualElement _overlayHost = null!;
        VisualElement? _overlay;
        MenuList _overlayMenu = null!;
        PromptBar _prompts = null!;
        Camera _cam = null!;
        Vector3 _camHome;
        Quaternion _camHomeRot;
        float _stageTime;
        float _cornerDeadline;
        LocLabel _cornerCountdown = null!;
        Part _p2Repair;
        readonly List<Part> _cornerParts = new List<Part>();
        LocLabel _pauseDummy = null!,
            _pauseFrames = null!;

        public Stage CurrentStage { get; private set; }
        public bool Paused { get; private set; }
        public MatchState State => _state;
        public int TotalHits { get; private set; }
        public int LastComboHits { get; private set; }
        public int LastComboDamage { get; private set; }
        public bool ScriptRunning => _scriptedP1.Count > 0;

        /// <summary>Feeds P1's next ticks from a script instead of the device (tests and the screenshot runner).</summary>
        public void QueueP1(IEnumerable<InputFrame> frames)
        {
            foreach (var f in frames)
                _scriptedP1.Enqueue(f);
        }

        /// <summary>Development hook (tests, screenshot runner): sets a robot's HP so rounds end quickly.</summary>
        public void DevSetHp(int side, int hp)
        {
            if (side == 1)
                _state.P1.Hp = hp;
            else
                _state.P2.Hp = hp;
        }

        /// <summary>Development hook: fills a robot's OVERLOAD gauge.</summary>
        public void DevSetOverload(int side, int value)
        {
            if (side == 1)
            {
                _state.P1.Overload = value;
                _state.P1.OverloadQuiet = 0;
            }
            else
            {
                _state.P2.Overload = value;
                _state.P2.OverloadQuiet = 0;
            }
        }

        /// <summary>Development hook: sets one part's HP.</summary>
        public void DevSetPart(int side, Part part, int hp)
        {
            if (side == 1)
                _state.P1.SetPart(part, hp);
            else
                _state.P2.SetPart(part, hp);
        }

        void Start()
        {
            var cfg = MatchRequest.Config;
            _def1 = StarterTypes.Get(cfg.P1);
            _def2 = StarterTypes.Get(cfg.P2);
            _sim = new Simulation(
                StarterMoves.Table(),
                StarterMoves.Fighter(_def1),
                StarterMoves.Fighter(_def2)
            );
            _state = _sim.NewMatch(cfg);

            var p1Device =
                MatchRequest.P1Device != null && MatchRequest.P1Device.added
                    ? MatchRequest.P1Device
                    : (InputDevice)Gamepad.current ?? Keyboard.current;
            _in1 = new FightInput(p1Device);
            _in2 =
                cfg.Mode == MatchMode.VersusPlayer ? new FightInput(MatchRequest.P2Device) : null;

            var root = GetComponent<UIDocument>().rootVisualElement;
            _hud = new FightHud(root, cfg, _def1, _def2, _sim);
            _overlayHost = root.Q("overlay-host");
            _overlayHost.style.display = DisplayStyle.None;
            _prompts = new PromptBar(root.Q("prompt-bar"));

            _cam = Camera.main;
            if (_cam != null)
            {
                _camHome = _cam.transform.position;
                _camHomeRot = _cam.transform.rotation;
            }
            _v1 = new RobotView(P1Robot, _def1, _sim.Fighter(1), Prefab(cfg.P1));
            _v2 = new RobotView(P2Robot, _def2, _sim.Fighter(2), Prefab(cfg.P2));
            BeginVsIntro();
            Render();
        }

        GameObject? Prefab(StarterType t) =>
            RobotPrefabs != null && (int)t < RobotPrefabs.Length ? RobotPrefabs[(int)t] : null;

        void Update()
        {
            var input = MenuInput.Instance;
            if (input == null)
                return;
            var merged = input.Merged();
            switch (CurrentStage)
            {
                case Stage.VsIntro:
                    UpdateVsIntro(merged);
                    break;
                case Stage.Fight:
                    UpdateFight(input, merged);
                    break;
                case Stage.CornerBreak:
                    UpdateCornerBreak(merged);
                    break;
                case Stage.Results:
                    _overlayMenu.Tick(merged);
                    break;
            }

            Render();
            _hud.Update(
                _state,
                _sim,
                MatchRequest.Dummy,
                MatchRequest.ShowFrames,
                MenuInput.KindOf(_in1.Device ?? Keyboard.current),
                showBanner: CurrentStage != Stage.VsIntro
            );
            _prompts.Show(PromptsFor(), input.LastKind);
        }

        PromptSet PromptsFor() =>
            CurrentStage switch
            {
                Stage.VsIntro => PromptSet.Skip,
                Stage.Fight => Paused ? PromptSet.Confirm | PromptSet.Back : PromptSet.Pause,
                _ => PromptSet.Confirm,
            };

        // ---------------------------------------------------------------- VS intro (challenger declaration)

        void BeginVsIntro()
        {
            CurrentStage = Stage.VsIntro;
            _stageTime = 0f;
            _hud.SetBottomVisible(false);
            _overlay = Show(IntroTemplate);
            FillIntroSide("p1", _def1);
            FillIntroSide("p2", _def2);
            _overlay.Q<LocLabel>("p2-tag").Key =
                _state.Config.Mode == MatchMode.VersusPlayer
                    ? "ui.select.p2"
                    : "ui.intro.challenger";
            AudioService.PlayFight(UiCues.Bell);
        }

        void FillIntroSide(string p, StarterTypeDef def)
        {
            _overlay.Q<LocLabel>($"{p}-name").Key = def.NameKey;
            _overlay.Q<LocLabel>($"{p}-class").Key = StarterTypes.ClassKey(def.Class);
            var stats = _overlay.Q<LocLabel>($"{p}-stats");
            stats.Key = "ui.intro.stats";
            stats.SetArgs(Loc.Number(def.Hp), Loc.Number(def.Stamina), def.Power);
        }

        void UpdateVsIntro(NavFrame merged)
        {
            _stageTime += Time.unscaledDeltaTime;
            if (_cam != null)
            {
                // Close-up on P1, then on the challenger, then back to the side camera.
                Vector3 pos;
                Vector3 look;
                if (_stageTime < VsIntroSeconds * 0.42f)
                {
                    float x = RobotView.F(_state.P1.X);
                    pos = new Vector3(x + 2.3f, 1.9f, -3.6f);
                    look = new Vector3(x, 1.45f, 0f);
                }
                else if (_stageTime < VsIntroSeconds * 0.84f)
                {
                    float x = RobotView.F(_state.P2.X);
                    pos = new Vector3(x - 2.3f, 1.9f, -3.6f);
                    look = new Vector3(x, 1.45f, 0f);
                }
                else
                {
                    pos = _camHome;
                    look = _camHome + _camHomeRot * Vector3.forward;
                }
                float k = 1f - Mathf.Exp(-5f * Time.unscaledDeltaTime);
                _cam.transform.position = Vector3.Lerp(_cam.transform.position, pos, k);
                _cam.transform.rotation = Quaternion.Slerp(
                    _cam.transform.rotation,
                    Quaternion.LookRotation(look - pos),
                    k
                );
            }
            if (_stageTime > VsIntroSeconds || merged.Submit || merged.Start)
                EndVsIntro();
        }

        /// <summary>Ends the challenger declaration and starts round 1's intro.</summary>
        public void EndVsIntro()
        {
            if (CurrentStage != Stage.VsIntro)
                return;
            CloseOverlay();
            if (_cam != null)
            {
                _cam.transform.position = _camHome;
                _cam.transform.rotation = _camHomeRot;
            }
            _hud.SetBottomVisible(true);
            CurrentStage = Stage.Fight;
            _accumulator = 0f;
        }

        // ---------------------------------------------------------------- fight

        void UpdateFight(MenuInput input, NavFrame merged)
        {
            if (Paused)
            {
                if (merged.Start || merged.Cancel)
                    ClosePause(sound: true);
                else
                    _overlayMenu.Tick(merged);
                return;
            }
            foreach (var f in input.Frames)
            {
                if (f.Start)
                {
                    OpenPause();
                    return;
                }
            }

            _in1.Latch();
            _in2?.Latch();
            _accumulator += Mathf.Min(Time.unscaledDeltaTime, 0.25f);
            while (_accumulator >= TickSeconds)
            {
                _accumulator -= TickSeconds;
                var a = _scriptedP1.Count > 0 ? _scriptedP1.Dequeue() : _in1.Sample();
                _sim.Tick(ref _state, a, P2Input());
                HandleEvents();
                if (_state.Phase == MatchPhase.CornerBreak)
                {
                    BeginCornerBreak();
                    return;
                }
                if (_state.Phase == MatchPhase.MatchOver)
                {
                    BeginResults();
                    return;
                }
            }
            FollowCamera();
        }

        InputFrame P2Input()
        {
            if (_in2 != null)
                return _in2.Sample();
            if (_state.Config.Mode != MatchMode.Training)
                return default; // the CPU opponent arrives with WP-D19; until then it stands still
            byte back = _state.P2.Facing > 0 ? InputFrame.Left : InputFrame.Right;
            return MatchRequest.Dummy switch
            {
                DummyAction.Block => new InputFrame(0, back),
                DummyAction.CrouchBlock => new InputFrame(0, (byte)(back | InputFrame.Down)),
                _ => default,
            };
        }

        void HandleEvents()
        {
            for (int i = 0; i < _state.EventCount; i++)
            {
                var e = _state.Event(i);
                var defender = e.Side == 1 ? _v2 : _v1;
                switch (e.Type)
                {
                    case SimEventType.Hit:
                        TotalHits++;
                        LastComboHits = e.ComboHits;
                        LastComboDamage = e.ComboDamage;
                        defender.Flash(blocked: false);
                        _hud.ShowCombo(e.Side, e.ComboHits, e.ComboDamage, e.Counter);
                        AudioService.PlayFight(
                            e.Counter ? UiCues.Counter
                            : e.Move == MoveId.Uppercut
                            || e.Move == MoveId.BodyKick
                            || e.Move == MoveId.Straight
                                ? UiCues.HitHeavy
                            : UiCues.HitLight
                        );
                        break;
                    case SimEventType.Block:
                        defender.Flash(blocked: true);
                        _hud.ShowSub("ui.fight.block", 0.5f);
                        AudioService.PlayFight(UiCues.Block);
                        break;
                    case SimEventType.PartBroken:
                        _hud.ShowSub(
                            "ui.fight.part_broken",
                            1.2f,
                            Loc.Get(FightHud.PartKey(e.Part))
                        );
                        AudioService.PlayFight(UiCues.PartBreak);
                        break;
                    case SimEventType.Groggy:
                        AudioService.PlayFight(UiCues.Groggy);
                        break;
                    case SimEventType.Overload:
                        AudioService.PlayFight(UiCues.Overload);
                        break;
                    case SimEventType.Ko:
                        AudioService.PlayFight(UiCues.HitHeavy);
                        break;
                    case SimEventType.Fight:
                    case SimEventType.RoundOver:
                        AudioService.PlayFight(UiCues.Bell);
                        break;
                }
            }
        }

        void FollowCamera()
        {
            if (_cam == null)
                return;
            float mid = (RobotView.F(_state.P1.X) + RobotView.F(_state.P2.X)) * 0.5f;
            var p = _cam.transform.position;
            _cam.transform.position = new Vector3(
                Mathf.Lerp(p.x, _camHome.x + mid, 1f - Mathf.Exp(-6f * Time.unscaledDeltaTime)),
                p.y,
                p.z
            );
        }

        void Render()
        {
            _v1.Draw(_state.P1, _sim, 1, _state.Hitstop > 0);
            _v2.Draw(_state.P2, _sim, 2, _state.Hitstop > 0);
        }

        // ---------------------------------------------------------------- pause and training options

        public void OpenPause()
        {
            Paused = true;
            Time.timeScale = 0f;
            AudioService.Play(UiCues.NavConfirm);
            _overlay = Show(PauseTemplate);
            _overlayMenu = new MenuList();
            _overlayMenu.Add(_overlay.Q("item-resume"), () => ClosePause(sound: false));
            _pauseDummy = _overlay.Q<LocLabel>("item-dummy");
            if (_state.Config.Mode == MatchMode.Training)
                _overlayMenu.Add(_pauseDummy, CycleDummy);
            else
                _pauseDummy.style.display = DisplayStyle.None;
            _pauseFrames = _overlay.Q<LocLabel>("item-frames");
            _overlayMenu.Add(_pauseFrames, ToggleFrames);
            _overlayMenu.Add(_overlay.Q("item-restart"), Restart);
            _overlayMenu.Add(_overlay.Q("item-select"), () => Leave(ReturnTarget.RobotSelect));
            _overlayMenu.Add(_overlay.Q("item-main"), () => Leave(ReturnTarget.MainMenu));
            RefreshPauseLabels();
        }

        void CycleDummy()
        {
            MatchRequest.Dummy = (DummyAction)(((int)MatchRequest.Dummy + 1) % 3);
            RefreshPauseLabels();
        }

        void ToggleFrames()
        {
            MatchRequest.ShowFrames = !MatchRequest.ShowFrames;
            RefreshPauseLabels();
        }

        void RefreshPauseLabels()
        {
            _pauseDummy.SetArgs(Loc.Get(FightHud.DummyKey(MatchRequest.Dummy)));
            _pauseFrames.SetArgs(
                Loc.Get(MatchRequest.ShowFrames ? "ui.settings.on" : "ui.settings.off")
            );
        }

        void ClosePause(bool sound)
        {
            if (sound)
                AudioService.Play(UiCues.NavBack);
            Paused = false;
            Time.timeScale = 1f;
            CloseOverlay();
        }

        // ---------------------------------------------------------------- corner break

        void BeginCornerBreak()
        {
            CurrentStage = Stage.CornerBreak;
            _overlay = Show(CornerTemplate);
            var sub = _overlay.Q<LocLabel>("subtitle");
            sub.Key = "ui.corner.subtitle";
            sub.SetArgs(_state.Round, _state.WinsP1, _state.WinsP2);
            _p2Repair = Simulation.MostDamagedPart(_state.P2, _sim.Fighter(2));
            var opp = _overlay.Q<LocLabel>("opponent");
            opp.Key = "ui.corner.opponent";
            opp.SetArgs(Loc.Get(FightHud.PartKey(_p2Repair)));
            _cornerCountdown = _overlay.Q<LocLabel>("countdown");
            _cornerCountdown.Key = "ui.corner.countdown";

            var host = _overlay.Q("parts");
            _overlayMenu = new MenuList();
            _cornerParts.Clear();
            var body = _sim.Fighter(1);
            for (int i = 0; i < (int)Part.Count; i++)
            {
                var part = (Part)i;
                int hp = _state.P1.GetPart(part);
                var st = StatusRules.StateOf(hp, body.PartMax);
                var row = new VisualElement();
                row.AddToClassList("corner-row");
                row.EnableInClassList("is-damaged", st == PartState.Damaged);
                row.EnableInClassList("is-broken", st == PartState.Broken);
                row.Add(new LocLabel(FightHud.PartKey(part), "corner-part-name"));
                var info = new LocLabel("ui.part.row", "corner-part-state");
                info.SetArgs(
                    Loc.Get(FightHud.StateKey(st)),
                    100 * System.Math.Max(0, hp) / body.PartMax
                );
                row.Add(info);
                host.Add(row);
                _cornerParts.Add(part);
                _overlayMenu.Add(row, () => ConfirmCornerBreak(part));
            }
            _overlayMenu.Select((int)Simulation.MostDamagedPart(_state.P1, body), sound: false);
            _cornerDeadline = Time.unscaledTime + CornerBreakSeconds;
            AudioService.PlayFight(UiCues.Bell);
        }

        void UpdateCornerBreak(NavFrame merged)
        {
            float left = _cornerDeadline - Time.unscaledTime;
            _cornerCountdown.SetArgs(Mathf.Max(0, Mathf.CeilToInt(left)));
            if (left <= 0f)
            {
                _overlayMenu.Activate();
                return;
            }
            _overlayMenu.Tick(merged);
        }

        /// <summary>P1 repairs <paramref name="part"/>; the idle robot repairs its most damaged part.</summary>
        public void ConfirmCornerBreak(Part part)
        {
            if (CurrentStage != Stage.CornerBreak)
                return;
            _sim.ApplyCornerBreak(ref _state, part, _p2Repair);
            CloseOverlay();
            CurrentStage = Stage.Fight;
            _accumulator = 0f;
        }

        // ---------------------------------------------------------------- results (winner declaration)

        void BeginResults()
        {
            CurrentStage = Stage.Results;
            _hud.SetBottomVisible(false);
            _overlay = Show(ResultsTemplate);
            var winner = _overlay.Q<LocLabel>("winner");
            if (_state.MatchWinner == 3)
                winner.Key = "ui.results.draw";
            else
            {
                winner.Key = "ui.fight.side_name";
                winner.SetArgs(
                    Loc.Get(
                        _state.MatchWinner == 1
                            ? "ui.select.p1"
                            : FightHud.P2HeadKey(_state.Config.Mode)
                    ),
                    Loc.Get(_state.MatchWinner == 1 ? _def1.NameKey : _def2.NameKey)
                );
            }
            var score = _overlay.Q<LocLabel>("score");
            score.Key = "ui.results.score";
            score.SetArgs(_state.WinsP1, _state.WinsP2);
            _overlayMenu = new MenuList();
            _overlayMenu.Add(_overlay.Q("item-rematch"), Restart);
            _overlayMenu.Add(_overlay.Q("item-select"), () => Leave(ReturnTarget.RobotSelect));
            _overlayMenu.Add(_overlay.Q("item-main"), () => Leave(ReturnTarget.MainMenu));
            AudioService.PlayFight(UiCues.Bell);
        }

        // ---------------------------------------------------------------- helpers

        VisualElement Show(VisualTreeAsset template)
        {
            CloseOverlay();
            var root = template.Instantiate();
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.top = 0;
            root.style.right = 0;
            root.style.bottom = 0;
            _overlayHost.Add(root);
            _overlayHost.style.display = DisplayStyle.Flex;
            return root;
        }

        void CloseOverlay()
        {
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
            _overlayHost.style.display = DisplayStyle.None;
        }

        void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(FightScene);
        }

        void Leave(ReturnTarget target)
        {
            Time.timeScale = 1f;
            MatchRequest.ReturnTo = target;
            SceneManager.LoadScene(MenuScene);
        }

        void OnDestroy() => Time.timeScale = 1f;

        public static string ModeKey(MatchMode m) =>
            m switch
            {
                MatchMode.VersusPlayer => "ui.versus.pvp",
                MatchMode.Training => "ui.menu.training",
                _ => "ui.versus.cpu",
            };
    }
}
