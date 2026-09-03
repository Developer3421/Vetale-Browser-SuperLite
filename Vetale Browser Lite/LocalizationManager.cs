using System;
using System.Linq;
using System.Windows;

namespace Vetale_Browser_Lite
{
    /// <summary>
    /// XAML-dictionary localization (Translations/*.xaml).
    /// The choice persists in the encrypted settings DB (DB #4).
    /// </summary>
    public static class LocalizationManager
    {
        public static readonly string[] Available = { "uk", "en", "de", "ru", "tr" };
        private const string SettingsKey = "language";
        private static ResourceDictionary _currentDict;

        public static string Current { get; private set; } = "uk";

        public static event EventHandler LanguageChanged;

        public static void Initialize()
        {
            var saved = SettingsStore.Get(SettingsKey, string.Empty);
            if (Array.IndexOf(Available, saved) >= 0)
            {
                Apply(saved, save: false);
                return;
            }
            // First run: match OS language when supported, else Ukrainian
            var iso = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            Apply(Array.IndexOf(Available, iso) >= 0 ? iso : "uk", save: true);
        }

        public static void Apply(string lang, bool save = true)
        {
            if (Array.IndexOf(Available, lang) < 0)
                lang = "uk";
            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri($"Translations/{lang}.xaml", UriKind.Relative)
                };
                var merged = Application.Current.Resources.MergedDictionaries;
                if (_currentDict != null)
                    merged.Remove(_currentDict);
                merged.Add(dict);
                _currentDict = dict;
                Current = lang;
                if (save)
                    SettingsStore.Set(SettingsKey, lang);
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
            catch { /* keep previous language on any error */ }
        }

        public static string Get(string key)
        {
            try
            {
                var obj = Application.Current?.TryFindResource(key);
                if (obj is string s)
                    return s;
            }
            catch { }
            return key;
        }
    }
}
