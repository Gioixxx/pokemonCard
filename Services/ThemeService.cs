using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace PokemonCardManager.Services
{
    public enum Theme
    {
        Light,
        Dark
    }

    public interface IThemeService
    {
        Theme CurrentTheme { get; }
        event EventHandler<Theme> ThemeChanged;
        void SetTheme(Theme theme);
        void LoadTheme();
    }

    public class ThemeService : IThemeService
    {
        private readonly string _settingsPath;
        private Theme _currentTheme = Theme.Light;

        public Theme CurrentTheme => _currentTheme;
        public event EventHandler<Theme>? ThemeChanged;

        public ThemeService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PokemonCardManager");
            Directory.CreateDirectory(appDataPath);
            _settingsPath = Path.Combine(appDataPath, "theme.json");
        }

        public void SetTheme(Theme theme)
        {
            if (_currentTheme == theme)
                return;

            _currentTheme = theme;
            ApplyTheme(theme);
            SaveTheme();
            ThemeChanged?.Invoke(this, theme);
        }

        public void LoadTheme()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    if (settings != null && Enum.IsDefined(typeof(Theme), settings.Theme))
                    {
                        _currentTheme = settings.Theme;
                    }
                }
            }
            catch
            {
                // Use default theme if loading fails
                _currentTheme = Theme.Light;
            }

            ApplyTheme(_currentTheme);
        }

        private void ApplyTheme(Theme theme)
        {
            var app = Application.Current;
            if (app == null) return;

            // Remove existing theme dictionaries
            var resourcesToRemove = new System.Collections.Generic.List<ResourceDictionary>();
            foreach (ResourceDictionary dict in app.Resources.MergedDictionaries)
            {
                if (dict.Source != null && dict.Source.ToString().Contains("Theme"))
                {
                    resourcesToRemove.Add(dict);
                }
            }

            foreach (var dict in resourcesToRemove)
            {
                app.Resources.MergedDictionaries.Remove(dict);
            }

            // Add new theme dictionary
            var themeUri = theme == Theme.Dark
                ? new Uri("pack://application:,,,/Resources/DarkTheme.xaml")
                : new Uri("pack://application:,,,/Resources/LightTheme.xaml");

            app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }

        private void SaveTheme()
        {
            try
            {
                var settings = new ThemeSettings { Theme = _currentTheme };
                var json = JsonSerializer.Serialize(settings);
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        private class ThemeSettings
        {
            public Theme Theme { get; set; }
        }
    }
}

