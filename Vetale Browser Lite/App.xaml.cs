using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using CefSharp;
using CefSharp.Wpf;

namespace Vetale_Browser_Lite
{ 
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationManager.Initialize();

            base.OnStartup(e);

            if (Cef.IsInitialized != true)
            {
                // Persistent profile like standard browsers: a stable folder in
                // %LocalAppData% (not the working directory, which may change),
                // so cache, cookies and sessions survive restarts.
                var profileDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Vetale Browser SuperLite");
                System.IO.Directory.CreateDirectory(profileDir);

                var settings = new CefSettings
                {
                    CachePath = System.IO.Path.Combine(profileDir, "cache"),
                    PersistSessionCookies = true,
                    LogSeverity = LogSeverity.Disable,
                    WindowlessRenderingEnabled = false
                };

                // Performance optimizations for fast JS execution and rendering
                settings.CefCommandLineArgs.Add("disable-gpu-vsync", "1");
                settings.CefCommandLineArgs.Add("disable-low-end-device-mode", "1");
                settings.CefCommandLineArgs.Add("enable-accelerated-video-decode", "1");
                settings.CefCommandLineArgs.Add("max-tiles-for-interest-area", "512");
                settings.CefCommandLineArgs.Add("num-raster-threads", "4");
                settings.CefCommandLineArgs.Add("enable-gpu-rasterization", "1");
                settings.CefCommandLineArgs.Add("enable-zero-copy", "1");
                settings.CefCommandLineArgs.Add("disable-background-timer-throttling", "1");
                settings.CefCommandLineArgs.Add("disable-renderer-backgrounding", "1");
                settings.CefCommandLineArgs.Add("enable-hardware-overlays", "1");

                Cef.Initialize(settings);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Cef.IsInitialized == true)
            {
                Cef.Shutdown();
            }

            base.OnExit(e);
        }
    }
}
