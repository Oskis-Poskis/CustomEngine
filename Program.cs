using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Engine
{
    class Program
    {
        static void Main()
        {
            NativeWindowSettings window_settings = new()
            {
                Vsync = VSyncMode.Off,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.Debug,
                Title = "title"
            };

            AppWindow window = new AppWindow(window_settings, true);
            window.Run();
        }
    }
}