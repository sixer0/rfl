#nullable enable
using System.Collections;
using System.Collections.Generic;
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Game.UI.Screens;
using RFL.Sim;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    /// <summary>Runs the menu scene: a stack of screens, one optional modal and the prompt bar.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class MenuController : MonoBehaviour
    {
        [Header("Screen layouts (UXML)")]
        public VisualTreeAsset TitleScreen = null!;
        public VisualTreeAsset MainMenu = null!;
        public VisualTreeAsset Versus = null!;
        public VisualTreeAsset Difficulty = null!;
        public VisualTreeAsset RobotSelect = null!;
        public VisualTreeAsset Settings = null!;
        public VisualTreeAsset Confirm = null!;

        public static MenuController? Instance { get; private set; } = null!;
        readonly List<MenuScreen> _stack = new List<MenuScreen>();
        MenuScreen? _modal;
        VisualElement _screenHost = null!;
        VisualElement _modalHost = null!;
        PromptBar _prompts = null!;

        public bool Ready { get; private set; }
        public MenuScreen? Top => _modal ?? (_stack.Count > 0 ? _stack[_stack.Count - 1] : null);
        public MenuScreen? TopScreen => _stack.Count > 0 ? _stack[_stack.Count - 1] : null;

        void Awake() => Instance = this;

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        IEnumerator Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _screenHost = root.Q("screen-host");
            _modalHost = root.Q("modal-host");
            _modalHost.style.display = DisplayStyle.None;
            _prompts = new PromptBar(root.Q("prompt-bar"));

            yield return LocalizationSettings.InitializationOperation;
            GameServices.Settings.ApplyLanguage();

            OpenFirstScreen();
            Ready = true;
        }

        void OpenFirstScreen()
        {
            var target = MatchRequest.ReturnTo;
            MatchRequest.ReturnTo = ReturnTarget.Title;
            if (target == ReturnTarget.Title)
            {
                Push(new TitleScreen());
                return;
            }
            Push(new MainMenuScreen());
            if (target != ReturnTarget.RobotSelect)
                return;
            var mode = MatchRequest.Config.Mode;
            if (mode != MatchMode.Training)
                Push(new VersusScreen());
            if (mode == MatchMode.VersusCpu)
                Push(new DifficultyScreen());
            Push(new RobotSelectScreen(mode, MatchRequest.Config.Cpu));
        }

        void Update()
        {
            if (!Ready)
                return;
            var input = MenuInput.Instance;
            var top = Top;
            if (input == null || top == null)
                return;
            top.Tick(input.Merged(), input);
            var shownTop = Top;
            _prompts.Show(shownTop != null ? shownTop.Prompts : PromptSet.None, input.LastKind);
        }

        public void Push(MenuScreen screen)
        {
            if (_stack.Count > 0)
                _stack[_stack.Count - 1].Root.style.display = DisplayStyle.None;
            screen.Attach(this, _screenHost, fill: true);
            _stack.Add(screen);
            screen.Enter();
        }

        public void Pop()
        {
            if (_stack.Count <= 1)
                return;
            var s = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            s.Exit();
            s.Root.RemoveFromHierarchy();
            var t = _stack[_stack.Count - 1];
            t.Root.style.display = DisplayStyle.Flex;
            t.Resume();
        }

        public void Replace(MenuScreen screen)
        {
            if (_stack.Count > 0)
            {
                var s = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
                s.Exit();
                s.Root.RemoveFromHierarchy();
            }
            Push(screen);
        }

        public void ShowModal(MenuScreen m)
        {
            if (_modal != null)
                CloseModal();
            _modal = m;
            _modalHost.style.display = DisplayStyle.Flex;
            m.Attach(this, _modalHost, fill: false);
            m.Enter();
        }

        public void CloseModal()
        {
            if (_modal == null)
                return;
            _modal.Exit();
            _modal.Root.RemoveFromHierarchy();
            _modal = null;
            _modalHost.style.display = DisplayStyle.None;
        }
    }
}
