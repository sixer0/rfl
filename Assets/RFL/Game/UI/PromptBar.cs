#nullable enable
using System;
using RFL.Game.Input;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    /// <summary>Bottom bar of button prompts for the device the player used last (PF-REQ-023 basis).</summary>
    public sealed class PromptBar
    {
        readonly VisualElement _root;
        PromptSet _shown = (PromptSet)(-1);
        DeviceKind _kind;

        public PromptBar(VisualElement root)
        {
            _root = root;
            Loc.Changed += Invalidate;
            root.RegisterCallback<DetachFromPanelEvent>(_ => Loc.Changed -= Invalidate);
        }

        void Invalidate() => _shown = (PromptSet)(-1);

        public void Show(PromptSet set, DeviceKind k)
        {
            if (_root == null || (set == _shown && k == _kind))
                return;
            _shown = set;
            _kind = k;
            _root.Clear();
            _root.style.display = set == PromptSet.None ? DisplayStyle.None : DisplayStyle.Flex;
            if ((set & PromptSet.Change) != 0)
                Add(Glyphs.Change(k), "ui.prompt.change");
            if ((set & PromptSet.Tabs) != 0)
                Add(Glyphs.Tabs(k), "ui.prompt.tabs");
            if ((set & PromptSet.Confirm) != 0)
                Add(Glyphs.Confirm(k), "ui.prompt.confirm");
            if ((set & PromptSet.Back) != 0)
                Add(Glyphs.Back(k), "ui.prompt.back");
            if ((set & PromptSet.Pause) != 0)
                Add(Glyphs.Pause(k), "ui.prompt.pause");
            if ((set & PromptSet.Skip) != 0)
                Add(Glyphs.Confirm(k), "ui.prompt.skip");
        }

        void Add(string glyph, string key)
        {
            var item = new VisualElement();
            item.AddToClassList("prompt");
            var g = new Label(glyph);
            g.AddToClassList("glyph");
            item.Add(g);
            item.Add(new LocLabel(key, "prompt-text"));
            _root.Add(item);
        }
    }
}
