using System;
using System.IO;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

namespace Vetale_Browser_Lite
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationManager.Initialize();

            base.OnStartup(e);

            InitializeCef();
        }

        private static void InitializeCef()
        {
            var settings = new CefSettings();

            // ============================================================
            // CACHE
            // ============================================================

            settings.CachePath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Vetale Browser SuperLite",
                "CefCache");

            // Persist cookies/sessions like standard browsers.
            settings.PersistSessionCookies = true;

            // Windowed (HWND) rendering via HwndHost control: no OSR,
            // so CEF does not force --disable-gpu-compositing.
            settings.WindowlessRenderingEnabled = false;

            // Disable CefSharp's default switches.
            settings.CommandLineArgsDisabled = true;


            // ============================================================
            // GPU
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-gpu",
                "1");

            settings.CefCommandLineArgs.Add(
                "ignore-gpu-blocklist",
                "1");

            // NOTE: no "disable-software-rasterizer" here on purpose —
            // without a software fallback any GPU hiccup = blank pages.


            // ============================================================
            // GPU COMPOSITING
            // ============================================================

            settings.CefCommandLineArgs.Remove(
                "disable-gpu");

            settings.CefCommandLineArgs.Remove(
                "disable-gpu-compositing");

            settings.CefCommandLineArgs.Remove(
                "disable-gpu-vsync");

            settings.CefCommandLineArgs.Add(
                "enable-gpu-compositing",
                "1");


            // ============================================================
            // GPU RASTERIZATION
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-gpu-rasterization",
                "1");

            settings.CefCommandLineArgs.Add(
                "enable-oop-rasterization",
                "1");

            settings.CefCommandLineArgs.Add(
                "enable-zero-copy",
                "1");


            // ============================================================
            // DIRECT3D / ANGLE
            // ============================================================

            // D3D11: default stable backend.
            settings.CefCommandLineArgs.Add(
                "use-angle",
                "d3d11");


            // ============================================================
            // WEBGL
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-webgl",
                "1");

            settings.CefCommandLineArgs.Add(
                "enable-webgl2",
                "1");


            // ============================================================
            // WEBGPU
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-unsafe-webgpu",
                "1");


            // ============================================================
            // EXPERIMENTAL GPU FEATURES (single key — Dictionary.Add
            // throws on duplicates)
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-features",
                string.Join(",",
                    "WebNN",
                    "WebMachineLearningNeuralNetwork",
                    "WebGPU",
                    // Trying SkiaGraphite per request — if pages go blank,
                    // this flag is the first suspect for revert.
                    "SkiaGraphite",
                    "Accelerated2dCanvas",
                    "CanvasOopRasterization"
                ));


            // ============================================================
            // HARDWARE VIDEO
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-accelerated-video-decode",
                "1");

            settings.CefCommandLineArgs.Add(
                "enable-accelerated-video-encode",
                "1");


            // ============================================================
            // VIDEO GPU MEMORY
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-gpu-memory-buffer-video-frames",
                "1");


            // ============================================================
            // HARDWARE OVERLAYS
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-hardware-overlays",
                "1");

            settings.CefCommandLineArgs.Add(
                "enable-direct-composition-video-overlays",
                "1");


            // ============================================================
            // RASTER THREADS
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "num-raster-threads",
                "4");

            settings.CefCommandLineArgs.Add(
                "max-tiles-for-interest-area",
                "512");

            settings.CefCommandLineArgs.Add(
                "enable-smooth-scrolling",
                "1");


            // ============================================================
            // CANVAS
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-accelerated-2d-canvas",
                "1");


            // ============================================================
            // BACKGROUND PERFORMANCE
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "disable-background-timer-throttling",
                "1");

            settings.CefCommandLineArgs.Add(
                "disable-renderer-backgrounding",
                "1");

            settings.CefCommandLineArgs.Add(
                "disable-backgrounding-occluded-windows",
                "1");


            // ============================================================
            // V-SYNC
            // ============================================================

            // DO NOT disable GPU VSync.
            //
            // No:
            // disable-gpu-vsync


            // ============================================================
            // MEDIA
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-media-stream",
                "1");


            // ============================================================
            // JAVASCRIPT
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-javascript",
                "1");


            // ============================================================
            // NETWORK
            // ============================================================

            settings.CefCommandLineArgs.Add(
                "enable-quic",
                "1");


            // ============================================================
            // INITIALIZE CEF
            // ============================================================

            if (Cef.IsInitialized != true)
            {
                Cef.Initialize(
                    settings,
                    performDependencyCheck: true,
                    browserProcessHandler: null);
            }
        }


        // ================================================================
        // SHUTDOWN
        // ================================================================

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