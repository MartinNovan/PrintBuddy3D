// GtkWebView.cs
// Embed WebKitGTK WebView into Avalonia via NativeControlHost (Linux X11)
// Single-file implementation: GtkManager + GtkWebView control.
// NOTE: This code targets Linux with libgtk-3 and libwebkit2gtk-4.1 installed and Avalonia on X11.

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace PrintBuddy3D.Controls
{
    /// <summary>
    /// Native GTK manager: runs a GTK thread, pumps GTK events and executes requested actions on GTK thread.
    /// </summary>
    internal static class GtkManager
    {
        private static Thread? _gtkThread;
        private static readonly ConcurrentQueue<GtkAction> Queue = new();
        private static readonly AutoResetEvent Signal = new(false);
        private static volatile bool _running;
        private static volatile bool _initialized;

        private record GtkAction(Func<object?> Action, TaskCompletionSource<object?> Tcs);

        // Ensures a dedicated GTK thread exists before any GTK/WebKit APIs are used.
        // This isolates GTK from Avalonia's UI thread, preventing re-entrancy and deadlocks.
        public static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (typeof(GtkManager))
            {
                if (_initialized) return;

                _gtkThread = new Thread(GtkThreadLoop) { IsBackground = true, Name = "GTK-Thread" };
                _gtkThread.Start();

                // Wait for initialization (timeout 5s)
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (!_initialized)
                {
                    if (sw.ElapsedMilliseconds > 5000)
                        throw new TimeoutException("GTK thread failed to initialize within 5 seconds.");
                    Thread.Sleep(10);
                }
            }
        }

        // Dedicated thread entry: initializes GTK and processes events + queued actions.
        // Note: gtk_init_check is used (instead of gtk_init) to avoid hard failures if GTK cannot initialize.
        private static void GtkThreadLoop()
        {
            try
            {
                int argc = 0;
                IntPtr argv = IntPtr.Zero;
                var ok = gtk_init_check(ref argc, ref argv);
                if (!ok)
                {
                    throw new InvalidOperationException("gtk_init_check failed");
                }

                _running = true;
                _initialized = true;

                while (_running)
                {
                    // Process GTK events
                    while (gtk_events_pending())
                    {
                        gtk_main_iteration_do(false);
                    }

                    // Drain queue
                    if (Queue.TryDequeue(out var item))
                    {
                        try
                        {
                            var res = item.Action();
                            item.Tcs.SetResult(res);
                        }
                        catch (Exception ex)
                        {
                            item.Tcs.SetException(ex);
                        }
                        continue;
                    }

                    // Wait briefly for new work; timeout allows periodic event pumping even when idle
                    Signal.WaitOne(10);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GTK thread exception: " + e);
                throw;
            }
        }

        private static Task<object?> InvokeOnGtkThreadAsync(Func<object?> func)
        {
            var tcs = new TaskCompletionSource<object?>();
            Queue.Enqueue(new GtkAction(func, tcs));
            Signal.Set();
            return tcs.Task;
        }

        private static object? InvokeOnGtkThread(Func<object?> func)
        {
            var task = InvokeOnGtkThreadAsync(func);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// Create a GtkPlug parented to the provided X11 window (parentXid) and put a WebKit WebView into it.
        /// Returns an object with native pointers you can later destroy.
        /// This method is synchronous and safe to call from other threads.
        /// </summary>
        // Creates a GtkPlug (child widget) parented into an external X11 window (Avalonia's host),
        // inserts a WebKit WebView into it and returns raw pointers for later teardown.
        // Must be called after EnsureInitialized; execution is marshalled to the GTK thread.
        public static GtkPlugWithWebView CreatePlugWithWebView(ulong parentXid, string? url)
        {
            EnsureInitialized();

            var result = InvokeOnGtkThread(() =>
            {
                IntPtr plug = gtk_plug_new(parentXid);
                if (plug == IntPtr.Zero)
                    throw new InvalidOperationException("gtk_plug_new returned null");

                IntPtr webview = webkit_web_view_new();
                if (webview == IntPtr.Zero)
                    throw new InvalidOperationException("webkit_web_view_new returned null");

                // Add WebView to plug (plug is a GtkContainer)
                gtk_container_add(plug, webview);

                // Initial navigation
                webkit_web_view_load_uri(webview, url ?? "about:blank");

                // Realize widgets; ensures GdkWindow exists for XID retrieval
                gtk_widget_show_all(plug);

                // Obtain the realized window to export XID back to Avalonia
                IntPtr gdkWin = gtk_widget_get_window(plug);
                if (gdkWin == IntPtr.Zero)
                    throw new InvalidOperationException("gtk_widget_get_window returned null (plug not realized yet)");

                ulong plugXid = gdk_x11_window_get_xid(gdkWin);

                return new GtkPlugWithWebView
                {
                    Plug = plug,
                    WebView = webview,
                    PlugXid = plugXid
                };
            });

            return (GtkPlugWithWebView)result!;
        }

        // Destroys the WebView and its GtkPlug safely on the GTK thread.
        // Any exceptions during native destruction are intentionally swallowed to avoid crashing the app during shutdown.
        public static void DestroyPlug(GtkPlugWithWebView? plug)
        {
            if (plug == null) return;
            InvokeOnGtkThread(() =>
            {
                try
                {
                    if (plug.WebView != IntPtr.Zero)
                    {
                        gtk_widget_destroy(plug.WebView);
                    }
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (plug.Plug != IntPtr.Zero)
                    {
                        gtk_widget_destroy(plug.Plug);
                    }
                }
                catch
                {
                    // ignored
                }

                return null;
            });
        }

        #region Native P/Invoke
        private const string LibGtk = "libgtk-3.so.0";
        private const string LibGdk = "libgdk-3.so.0";
        private const string LibWebKit = "libwebkit2gtk-4.1.so.0";
        // Note: Exact SONAMEs are pinned here; ensure corresponding packages are installed on target systems.
        // This implementation targets GTK3 + WebKitGTK 4.1 on X11. Wayland is not supported in this host approach.

        [DllImport(LibGtk, CharSet = CharSet.Ansi, EntryPoint = "gtk_init_check")]
        private static extern bool gtk_init_check(ref int argc, ref IntPtr argv);

        [DllImport(LibGtk, EntryPoint = "gtk_events_pending")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool gtk_events_pending();

        [DllImport(LibGtk, EntryPoint = "gtk_main_iteration_do")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool gtk_main_iteration_do([MarshalAs(UnmanagedType.I1)] bool blocking);

        [DllImport(LibGtk, EntryPoint = "gtk_plug_new")]
        private static extern IntPtr gtk_plug_new(ulong socketId);

        [DllImport(LibGtk, EntryPoint = "gtk_widget_show_all")]
        private static extern void gtk_widget_show_all(IntPtr widget);

        [DllImport(LibGtk, EntryPoint = "gtk_container_add")]
        private static extern void gtk_container_add(IntPtr container, IntPtr widget);

        [DllImport(LibGtk, EntryPoint = "gtk_widget_destroy")]
        private static extern void gtk_widget_destroy(IntPtr widget);

        [DllImport(LibGtk, EntryPoint = "gtk_widget_get_window")]
        private static extern IntPtr gtk_widget_get_window(IntPtr widget);

        [DllImport(LibGdk, EntryPoint = "gdk_x11_window_get_xid")]
        private static extern ulong gdk_x11_window_get_xid(IntPtr gdkWindow);

        [DllImport(LibWebKit, EntryPoint = "webkit_web_view_new")]
        private static extern IntPtr webkit_web_view_new();

        [DllImport(LibWebKit, CharSet = CharSet.Ansi, EntryPoint = "webkit_web_view_load_uri")]
        private static extern void webkit_web_view_load_uri(IntPtr webView, string uri);
        #endregion
    }

    /// <summary>
    /// Simple container to keep native pointers.
    /// </summary>
    internal sealed class GtkPlugWithWebView
    {
        public IntPtr Plug { get; init; }
        public IntPtr WebView { get; init; }
        public ulong PlugXid { get; init; }
    }

    /// <summary>
    /// Avalonia control that hosts a WebKitGTK WebView using NativeControlHost + GtkPlug.
    /// Works on Linux/X11. Use like any other control in your Avalonia XAML.
    /// Limitations: requires X11 backend; Wayland is not supported. The control owns native resources and
    /// cleans them up in DestroyNativeControlCore.
    /// </summary>
    public class GtkWebView : NativeControlHost
    {
        public static readonly StyledProperty<string> UrlProperty =
            AvaloniaProperty.Register<GtkWebView, string>(nameof(Url));

        private GtkPlugWithWebView? _nativePlug;

        public string Url
        {
            get => GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }

        // Creates the native GtkPlug + WebView and returns its XID wrapped in a PlatformHandle.
        // Avalonia passes a parent platform handle; we coerce it into an X11 window id (ulong).
        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
        {
            // Parent must be provided and be an XID kind
            if (parent == null)
                throw new InvalidOperationException("Native parent handle is required for GtkWebView on Linux/X11.");

            // Get the raw handle (boxed if value type)
            object handleObj = parent.Handle;

            // Parse into ulong (supports string (decimal or 0xHEX), IntPtr, nint, numeric types)
            ulong parentXid;
            try
            {
                if (handleObj is string sHandle)
                {
                    if (sHandle.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ulong.TryParse(sHandle.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out parentXid))
                            throw new FormatException("Invalid hex XID: " + sHandle);
                    }
                    else if (!ulong.TryParse(sHandle, out parentXid))
                    {
                        // Try parsing as hex without prefix
                        if (!ulong.TryParse(sHandle, System.Globalization.NumberStyles.HexNumber, null, out parentXid))
                            throw new FormatException("Invalid XID string: " + sHandle);
                    }
                }
                else if (handleObj is IntPtr iptr)
                {
                    parentXid = unchecked((ulong)iptr.ToInt64());
                }
                else if (handleObj is nint ni)
                {
                    parentXid = unchecked((ulong)ni);
                }
                else
                {
                    // Fallback: try convert
                    parentXid = Convert.ToUInt64(handleObj);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse parent XID: " + parent.Handle, ex);
            }

            // ensure GTK thread is started
            GtkManager.EnsureInitialized();

            // create plug + webview on GTK thread synchronously
            var url = string.IsNullOrEmpty(Url) ? "about:blank" : Url;
            _nativePlug = GtkManager.CreatePlugWithWebView(parentXid, url);

            // return the plug's XID as Avalonia native control handle (string form)
            return new PlatformHandle((nint)(long)_nativePlug.PlugXid, "XID");
        }

        // Ensures native resources are released when Avalonia destroys the platform control.
        protected override void DestroyNativeControlCore(IPlatformHandle control)
        {
            if (_nativePlug != null)
            {
                try
                {
                    GtkManager.DestroyPlug(_nativePlug);
                }
                catch
                {
                    // ignored
                }

                _nativePlug = null;
            }

            base.DestroyNativeControlCore(control);
        }
    }
}
