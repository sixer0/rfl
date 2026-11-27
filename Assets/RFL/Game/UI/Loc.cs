#nullable enable
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace RFL.Game.UI
{
    /// <summary>
    /// String lookup for the "UI" string table (ID and EN). Player-facing numbers are formatted with the
    /// selected locale ("1.550" in Bahasa Indonesia, "1,550" in English); data never is (PF-REQ-071).
    /// </summary>
    public static class Loc
    {
        public const string Table = "UI";

        public static event Action? Changed;

        static bool _hooked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Changed = null;
            _hooked = false;
        }

        static void Hook()
        {
            if (_hooked)
                return;
            _hooked = true;
            LocalizationSettings.SelectedLocaleChanged += _ => Changed?.Invoke();
        }

        public static bool Ready => LocalizationSettings.InitializationOperation.IsDone;

        public static string Code
        {
            get
            {
                var l = LocalizationSettings.SelectedLocale;
                return l != null ? l.Identifier.Code : "en";
            }
        }

        public static IFormatProvider Formatter
        {
            get
            {
                var l = LocalizationSettings.SelectedLocale;
                return l != null && l.Formatter != null
                    ? l.Formatter
                    : CultureInfo.InvariantCulture;
            }
        }

        public static string Get(string key)
        {
            Hook();
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            var table = LocalizationSettings.StringDatabase.GetTable(Table);
            var entry = table != null ? table.GetEntry(key) : null;
            if (entry == null)
            {
                UnityEngine.Debug.LogWarning($"Missing string '{key}' in table {Table} ({Code}).");
                return key;
            }
            return entry.Value;
        }

        public static string Format(string key, params object[] args) =>
            args == null || args.Length == 0 ? Get(key) : string.Format(Formatter, Get(key), args);

        public static string Number(int value) => value.ToString("N0", Formatter);

        public static void SetLanguage(string code)
        {
            Hook();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(
                new LocaleIdentifier(code)
            );
            if (locale != null && LocalizationSettings.SelectedLocale != locale)
                LocalizationSettings.SelectedLocale = locale;
        }
    }
}
