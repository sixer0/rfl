#nullable enable
using System;
using System.Collections.Generic;
using RFL.Game.Audio;
using RFL.Game.Core;
using RFL.Game.Input;
using RFL.Game.Profile;
using UnityEngine;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>
    /// Settings with three tabs. Language, volumes and VSync apply at once; window mode and resolution
    /// wait for Confirm. Leaving the screen saves the profile.
    /// </summary>
    public sealed class SettingsScreen : MenuScreen
    {
        enum Kind
        {
            Cycle,
            Volume,
            Action,
        }

        sealed class Row
        {
            public Kind Kind;
            public string LabelKey = null!;
            public Func<int> Get = null!;
            public Action<int> Set = null!;
            public Func<int, string> Text = null!;
            public int Count;
            public bool Deferred;
            public int Pending = -1;
            public Action OnAction = null!;
            public VisualElement Element = null!;
            public Label Value = null!;
            public VisualElement Pips = null!;
        }

        static readonly string[] TabKeys =
        {
            "ui.settings.tab.game",
            "ui.settings.tab.audio",
            "ui.settings.tab.display",
        };

        readonly List<Row> _rows = new List<Row>();
        readonly List<LocLabel> _tabs = new List<LocLabel>();
        VisualElement _rowHost = null!;
        LocLabel _hint = null!;
        SettingsTab _tab = SettingsTab.Game;
        int _index;
        List<Vector2Int> _resolutions = null!;

        static SettingsService Svc => GameServices.Settings;

        protected override VisualTreeAsset Template(MenuController app) => app.Settings;

        public override PromptSet Prompts =>
            PromptSet.Change | PromptSet.Tabs | PromptSet.Confirm | PromptSet.Back;

        public SettingsTab Tab => _tab;
        public int RowIndex => _index;

        protected override void Build()
        {
            _rowHost = Root.Q("rows");
            _hint = Q(Root, "hint");
            var tabHost = Root.Q("tabs");
            for (int i = 0; i < TabKeys.Length; i++)
            {
                var t = new LocLabel(TabKeys[i], "tab");
                int ti = i;
                t.RegisterCallback<ClickEvent>(_ => ShowTab((SettingsTab)ti, sound: true));
                tabHost.Add(t);
                _tabs.Add(t);
            }
            _resolutions = SettingsService.Resolutions();
            ShowTab(SettingsTab.Game, sound: false);
        }

        public override void Enter() => Loc.Changed += RefreshValues;

        public override void Exit()
        {
            Loc.Changed -= RefreshValues;
            Svc.Save();
        }

        void ShowTab(SettingsTab t, bool sound)
        {
            _tab = t;
            for (int i = 0; i < _tabs.Count; i++)
                _tabs[i].EnableInClassList("is-active", i == (int)t);
            _rows.Clear();
            _rowHost.Clear();
            switch (t)
            {
                case SettingsTab.Game:
                    AddCycle(
                        "ui.settings.language",
                        SettingsService.Languages.Length,
                        () => Array.IndexOf(SettingsService.Languages, Svc.LanguageCode),
                        v =>
                        {
                            Svc.S.language = SettingsService.Languages[v];
                            Svc.ApplyLanguage();
                        },
                        v => Loc.Get("ui.settings.lang." + SettingsService.Languages[v])
                    );
                    break;
                case SettingsTab.Audio:
                    AddVolume(
                        "ui.settings.master",
                        () => Svc.S.master,
                        v =>
                        {
                            Svc.S.master = v;
                            Svc.ApplyAudio();
                        }
                    );
                    AddVolume("ui.settings.music", () => Svc.S.music, v => Svc.S.music = v);
                    AddVolume("ui.settings.effects", () => Svc.S.effects, v => Svc.S.effects = v);
                    AddVolume("ui.settings.ui", () => Svc.S.ui, v => Svc.S.ui = v);
                    break;
                case SettingsTab.Display:
                    var mode = AddCycle(
                        "ui.settings.window_mode",
                        3,
                        () => Svc.S.windowMode,
                        v =>
                        {
                            Svc.S.windowMode = v;
                            Svc.ApplyDisplay();
                        },
                        v => Loc.Get(WindowModeKey(v))
                    );
                    mode.Deferred = true;
                    var res = AddCycle(
                        "ui.settings.resolution",
                        _resolutions.Count,
                        CurrentResolutionIndex,
                        v =>
                        {
                            Svc.S.resolutionW = _resolutions[v].x;
                            Svc.S.resolutionH = _resolutions[v].y;
                            Svc.ApplyDisplay();
                        },
                        v => _resolutions[v].x + " × " + _resolutions[v].y
                    );
                    res.Deferred = true;
                    AddCycle(
                        "ui.settings.vsync",
                        2,
                        () => Svc.S.vsync ? 1 : 0,
                        v =>
                        {
                            Svc.S.vsync = v == 1;
                            Svc.ApplyDisplay();
                        },
                        v => Loc.Get(v == 1 ? "ui.settings.on" : "ui.settings.off")
                    );
                    break;
            }
            var reset = new Row
            {
                Kind = Kind.Action,
                LabelKey = "ui.settings.defaults",
                OnAction = () =>
                {
                    Svc.ResetTab(_tab);
                    RefreshValues();
                },
            };
            AddRow(reset);
            Select(0, sound: false);
            if (sound)
                AudioService.Play(UiCues.TabSwitch);
        }

        int CurrentResolutionIndex()
        {
            var want =
                Svc.S.resolutionW > 0
                    ? new Vector2Int(Svc.S.resolutionW, Svc.S.resolutionH)
                    : new Vector2Int(Screen.width, Screen.height);
            int i = _resolutions.IndexOf(want);
            return i >= 0 ? i : _resolutions.Count - 1;
        }

        static string WindowModeKey(int v) =>
            v switch
            {
                1 => "ui.settings.wm.windowed",
                2 => "ui.settings.wm.exclusive",
                _ => "ui.settings.wm.borderless",
            };

        Row AddCycle(
            string labelKey,
            int count,
            Func<int> get,
            Action<int> set,
            Func<int, string> text
        )
        {
            var r = new Row
            {
                Kind = Kind.Cycle,
                LabelKey = labelKey,
                Count = count,
                Get = get,
                Set = set,
                Text = text,
            };
            AddRow(r);
            return r;
        }

        void AddVolume(string labelKey, Func<int> get, Action<int> set)
        {
            AddRow(
                new Row
                {
                    Kind = Kind.Volume,
                    LabelKey = labelKey,
                    Count = SettingsData.VolumeMax + 1,
                    Get = get,
                    Set = set,
                }
            );
        }

        void AddRow(Row r)
        {
            int i = _rows.Count;
            var el = new VisualElement();
            el.AddToClassList("row");
            el.Add(new LocLabel(r.LabelKey, "row-label"));
            if (r.Kind != Kind.Action)
            {
                var wrap = new VisualElement();
                wrap.AddToClassList("row-value-wrap");
                var left = new Label("<");
                left.AddToClassList("arrow");
                left.RegisterCallback<ClickEvent>(_ =>
                {
                    Select(i, false);
                    Change(-1);
                });
                var right = new Label(">");
                right.AddToClassList("arrow");
                right.RegisterCallback<ClickEvent>(_ =>
                {
                    Select(i, false);
                    Change(1);
                });
                wrap.Add(left);
                if (r.Kind == Kind.Volume)
                {
                    r.Pips = new VisualElement();
                    r.Pips.AddToClassList("pips");
                    r.Pips.AddToClassList("volume-pips");
                    for (int p = 0; p < SettingsData.VolumeMax; p++)
                    {
                        var pip = new VisualElement();
                        pip.AddToClassList("pip");
                        r.Pips.Add(pip);
                    }
                    wrap.Add(r.Pips);
                }
                r.Value = new Label();
                r.Value.AddToClassList(r.Kind == Kind.Volume ? "row-number" : "row-value");
                wrap.Add(r.Value);
                wrap.Add(right);
                el.Add(wrap);
            }
            el.RegisterCallback<PointerEnterEvent>(_ => Select(i, sound: i != _index));
            el.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is Label l && l.ClassListContains("arrow"))
                    return;
                Select(i, false);
                Submit();
            });
            r.Element = el;
            _rows.Add(r);
            _rowHost.Add(el);
            RefreshRow(r);
        }

        void Select(int i, bool sound)
        {
            if (_rows.Count == 0)
                return;
            if (_index >= 0 && _index < _rows.Count)
            {
                var old = _rows[_index];
                old.Element.RemoveFromClassList("is-selected");
                if (old.Pending >= 0 && i != _index)
                {
                    old.Pending = -1; // leaving a row discards its unapplied value
                    RefreshRow(old);
                }
            }
            _index = (i % _rows.Count + _rows.Count) % _rows.Count;
            _rows[_index].Element.AddToClassList("is-selected");
            UpdateHint();
            if (sound)
                AudioService.Play(UiCues.NavMove);
        }

        void Change(int step)
        {
            var r = _rows[_index];
            if (r.Kind == Kind.Action)
                return;
            int cur = r.Pending >= 0 ? r.Pending : r.Get();
            int next;
            if (r.Kind == Kind.Volume)
            {
                next = Mathf.Clamp(cur + step, 0, r.Count - 1);
                if (next == cur)
                {
                    AudioService.Play(UiCues.NavError);
                    return;
                }
            }
            else
            {
                next = (cur + step + r.Count) % r.Count;
            }
            if (r.Deferred)
                r.Pending = next == r.Get() ? -1 : next;
            else
                r.Set(next);
            AudioService.Play(UiCues.NavMove);
            RefreshRow(r);
            UpdateHint();
        }

        void Submit()
        {
            var r = _rows[_index];
            if (r.Kind == Kind.Action)
            {
                AudioService.Play(UiCues.NavConfirm);
                r.OnAction();
                return;
            }
            if (r.Deferred && r.Pending >= 0)
            {
                r.Set(r.Pending);
                r.Pending = -1;
                AudioService.Play(UiCues.NavConfirm);
                RefreshRow(r);
                UpdateHint();
            }
        }

        void RefreshValues()
        {
            foreach (var r in _rows)
                RefreshRow(r);
            UpdateHint();
        }

        static void RefreshRow(Row r)
        {
            if (r.Kind == Kind.Action)
                return;
            int v = r.Pending >= 0 ? r.Pending : r.Get();
            if (r.Kind == Kind.Volume)
            {
                r.Value.text = Loc.Number(v);
                for (int p = 0; p < r.Pips.childCount; p++)
                    r.Pips[p].EnableInClassList("pip-on", p < v);
            }
            else
            {
                r.Value.text = r.Text(v);
            }
            r.Element.EnableInClassList("is-pending", r.Pending >= 0);
        }

        void UpdateHint()
        {
            var r = _rows.Count > 0 ? _rows[_index] : null;
            if (r != null && r.Pending >= 0)
            {
                _hint.Key = "ui.settings.apply_hint";
                var kind =
                    MenuInput.Instance != null ? MenuInput.Instance.LastKind : DeviceKind.Keyboard;
                _hint.SetArgs(Glyphs.Confirm(kind));
            }
            else
            {
                _hint.Key = string.Empty;
                _hint.text = string.Empty;
            }
        }

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            if (merged.Cancel)
            {
                Back();
                return;
            }
            if (merged.TabPrev || merged.TabNext)
            {
                int n = TabKeys.Length;
                ShowTab(
                    (SettingsTab)(((int)_tab + (merged.TabNext ? 1 : -1) + n) % n),
                    sound: true
                );
                return;
            }
            if (merged.Y != 0)
                Select(_index - merged.Y, sound: true);
            if (merged.X != 0)
                Change(merged.X);
            if (merged.Submit)
                Submit();
        }
    }
}
