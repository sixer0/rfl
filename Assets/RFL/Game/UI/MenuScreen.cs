#nullable enable
using RFL.Game.Audio;
using RFL.Game.Input;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    /// <summary>One menu screen or modal. Layout comes from UXML; behaviour from the subclass.</summary>
    public abstract class MenuScreen
    {
        protected MenuController App { get; private set; } = null!;
        public VisualElement Root { get; private set; } = null!;
        protected abstract VisualTreeAsset Template(MenuController app);

        public virtual PromptSet Prompts => PromptSet.Confirm | PromptSet.Back;

        internal void Attach(MenuController app, VisualElement host, bool fill)
        {
            App = app;
            Root = Template(app).Instantiate();
            // Fill the host in both cases: a modal's scrim is absolutely positioned inside this container.
            {
                Root.style.position = Position.Absolute;
                Root.style.left = 0;
                Root.style.top = 0;
                Root.style.right = 0;
                Root.style.bottom = 0;
            }
            host.Add(Root);
            Build();
        }

        protected virtual void Build() { }

        public virtual void Enter() { }

        public virtual void Resume() { }

        public virtual void Exit() { }

        public abstract void Tick(in NavFrame merged, MenuInput input);

        protected void Back()
        {
            AudioService.Play(UiCues.NavBack);
            App.Pop();
        }

        protected static LocLabel Q(VisualElement root, string name) => root.Q<LocLabel>(name);
    }
}
