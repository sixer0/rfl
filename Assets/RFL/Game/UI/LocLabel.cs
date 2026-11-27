#nullable enable
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFL.Game.UI
{
    /// <summary>
    /// A label that shows a string-table entry by key and updates when the language changes.
    /// In UXML: &lt;RFL.Game.UI.LocLabel key="ui.menu.versus" /&gt;. Never put display text in UXML.
    /// </summary>
    [UxmlElement]
    public partial class LocLabel : Label
    {
        string _key = null!;
        object[] _args = null!;

        [UxmlAttribute("key")]
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                Refresh();
            }
        }

        public LocLabel()
        {
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Loc.Changed += Refresh;
                Refresh();
            });
            RegisterCallback<DetachFromPanelEvent>(_ => Loc.Changed -= Refresh);
        }

        public LocLabel(string key, params string[] classes)
            : this()
        {
            foreach (var c in classes)
                AddToClassList(c);
            Key = key;
        }

        /// <summary>Format arguments for the entry ("{0}"); numbers use the selected locale.</summary>
        public void SetArgs(params object[] values)
        {
            _args = values;
            Refresh();
        }

        public void Refresh()
        {
            if (!Application.isPlaying || string.IsNullOrEmpty(_key))
                return;
            text = Loc.Format(_key, _args ?? Array.Empty<object>());
        }
    }
}
