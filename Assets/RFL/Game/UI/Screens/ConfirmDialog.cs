#nullable enable
using System;
using RFL.Game.Audio;
using RFL.Game.Input;
using UnityEngine.UIElements;

namespace RFL.Game.UI.Screens
{
    /// <summary>Yes / No modal. Focus starts on No; Back means No.</summary>
    public sealed class ConfirmDialog : MenuScreen
    {
        readonly string _messageKey;
        readonly Action _onYes;
        MenuList _menu = null!;

        public ConfirmDialog(string messageKey, Action onYes)
        {
            _messageKey = messageKey;
            _onYes = onYes;
        }

        protected override VisualTreeAsset Template(MenuController app) => app.Confirm;

        protected override void Build()
        {
            Q(Root, "message").Key = _messageKey;
            _menu = new MenuList(horizontal: true);
            _menu.Add(Root.Q("item-yes"), () =>
            {
                App.CloseModal();
                _onYes?.Invoke();
            });
            _menu.Add(Root.Q("item-no"), () => App.CloseModal());
            _menu.Select(1, sound: false);
        }

        public override void Tick(in NavFrame merged, MenuInput input)
        {
            if (merged.Cancel)
            {
                AudioService.Play(UiCues.NavBack);
                App.CloseModal();
                return;
            }
            _menu.Tick(merged);
        }
    }
}
