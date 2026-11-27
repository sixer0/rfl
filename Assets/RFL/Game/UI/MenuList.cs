#nullable enable
using System;
using System.Collections.Generic;
using RFL.Game.Audio;
using RFL.Game.Input;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    /// <summary>A list of selectable items driven by MenuInput, with mouse hover and click.</summary>
    public sealed class MenuList
    {
        struct Item
        {
            public VisualElement Element;
            public Action Action;
            public string? DescKey;
        }

        readonly List<Item> _items = new List<Item>();
        readonly LocLabel? _desc;
        readonly bool _horizontal;

        public MenuList(LocLabel? desc = null, bool horizontal = false)
        {
            _desc = desc;
            _horizontal = horizontal;
        }

        public int Index { get; private set; } = -1;
        public int Count => _items.Count;

        public void Add(VisualElement element, Action action, string? descKey = null)
        {
            int i = _items.Count;
            _items.Add(
                new Item
                {
                    Element = element,
                    Action = action,
                    DescKey = descKey,
                }
            );
            element.AddToClassList("menu-item");
            element.focusable = false;
            element.RegisterCallback<PointerEnterEvent>(_ => Select(i, sound: i != Index));
            element.RegisterCallback<ClickEvent>(_ =>
            {
                Select(i, sound: false);
                Activate();
            });
            if (Index < 0)
                Select(0, sound: false);
        }

        public void Select(int i, bool sound)
        {
            if (_items.Count == 0)
                return;
            if (Index >= 0 && Index < _items.Count)
                _items[Index].Element.RemoveFromClassList("is-selected");
            Index = (i % _items.Count + _items.Count) % _items.Count;
            _items[Index].Element.AddToClassList("is-selected");
            if (_desc != null)
                _desc.Key = _items[Index].DescKey ?? string.Empty;
            if (_desc != null && string.IsNullOrEmpty(_items[Index].DescKey))
                _desc.text = string.Empty;
            if (sound)
                AudioService.Play(UiCues.NavMove);
        }

        /// <summary>Moves on Y (and X for horizontal lists) and activates on Submit. Returns true if activated.</summary>
        public bool Tick(in NavFrame f)
        {
            int step = _horizontal ? (f.X != 0 ? f.X : -f.Y) : -f.Y;
            if (step != 0)
                Select(Index + step, sound: true);
            if (f.Submit)
            {
                Activate();
                return true;
            }
            return false;
        }

        public void Activate()
        {
            if (Index < 0 || Index >= _items.Count)
                return;
            AudioService.Play(UiCues.NavConfirm);
            _items[Index].Action?.Invoke();
        }
    }
}
