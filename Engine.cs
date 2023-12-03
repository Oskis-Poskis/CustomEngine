using WindowTemplate;
using WindowTemplate.Common;
using Engine.Common;
using Engine.Scene;
using Engine.UI;

using ImGuiNET;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Engine
{
    public class AppWindow : HostWindow
    {
        public AppWindow(NativeWindowSettings settings, bool UseWindowState) : base(settings, UseWindowState)
        {
            default_S = new Shader($"{base_directory}Shaders/mesh.vert", $"{base_directory}Shaders/mesh.frag");
            guidelines_S = new Shader($"{base_directory}Shaders/guides.vert", $"{base_directory}Shaders/guides.frag");
            viewport_camera = new Camera(new Vector3(0, 0, 5), -Vector3.UnitZ, 70, 10);
        }

        private static UI.ImGuiController UIController;
        public static StatCounter stats = new();
        private bool show_statistics = false;

        private static Camera viewport_camera;
        Mesh mesh1;
        Mesh test_mesh;
        List<Mesh> test_meshes = new();

        public static Shader default_S;
        public static Shader guidelines_S;
        public static string project_path = Environment.CurrentDirectory;
        
        protected unsafe override void OnLoad()
        {
            base.OnLoad();

            DllResolver.InitLoader();

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.LineWidth(2.0f);
            GL.PointSize(5.0f);

            mesh1 = new Mesh("Assets/smooth_monkey.fbx")
            {
                Rotation = new(-90, 0, 0)
            };

            for (int X = 0; X < 6; X++)
            {
                for (int Y = 0; Y < 6; Y++)
                {
                    test_mesh = new Mesh("Assets/smooth_monkey.fbx")
                    {
                        Rotation = new(-90, 0, 0),
                        Position = new Vector3(X * 5.0f, 0.0f, Y * 5.0f)
                    };

                    test_meshes.Add(test_mesh);
                }   
            }

            Maximized += (sender) =>
            {
                viewport_camera.aspect_ratio = window_aspect;
                guidelines_S.SetFloat("aspect", window_aspect);
                UIController.WindowResized(window_size.X, window_size.Y);
            };
            
            Resize += (sender) =>
            {
                viewport_camera.aspect_ratio = window_aspect;
                guidelines_S.SetFloat("aspect", window_aspect);
                UIController.WindowResized(window_size.X, window_size.Y);
            };

            GLFW.SetScrollCallback(WindowPtr, Scrolling);

            UIController = new UI.ImGuiController(window_size.X, window_size.Y);
            ImGui.GetIO().FontGlobalScale = 2.0f;

            var watcher = new FileSystemWatcher($"{base_directory}Shaders");
            watcher.Changed += ShaderFileChanged;
            watcher.EnableRaisingEvents = true;
        }

        private void ShaderFileChanged(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"File: {e.Name} was changed in shader directory, bla bla bla...");
        }

        private static unsafe void Scrolling(Window* window, double offsetX, double offsetY)
        {
            viewport_camera.Input(new Vector2((float)offsetX, (float)offsetY) * 50.0f, true);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            viewport_camera.Move((float)args.Time, KeyboardState);

            CursorState = (IsMouseButtonDown(MouseButton.Button2) || IsKeyDown(Keys.LeftAlt)) ? CursorState.Grabbed : CursorState.Normal;
            if (CursorState == CursorState.Grabbed) viewport_camera.Input(MouseState.Delta, false);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.05f, 0.05f, 0.05f, 1);

            stats.Count(args);
            viewport_camera.Update((float)args.Time);

            default_S.Use();

            default_S.SetMatrix4("view", viewport_camera.view);
            default_S.SetMatrix4("projection", viewport_camera.projection);
            default_S.SetMatrix4("model", mesh1.model_matrix);
            
            mesh1.Render();
            foreach (Mesh mesh in test_meshes)
            {
                default_S.Use();
                default_S.SetMatrix4("model", mesh.model_matrix);
                mesh.Render();
            }

            Camera.Render();

            UIController.Update(this, (float)args.Time);

            if (show_statistics) RenderStatistics();

            UIController.Render();

            SwapBuffers();
        }

        public static void RenderStatistics()
        {
            float memory = 0.0f;
            using (Process proc = Process.GetCurrentProcess())
            {
                // The proc.PrivateMemorySize64 will returns the private memory usage in byte.
                // Would like to Convert it to Megabyte? divide it by 2^20
                memory = proc.PrivateMemorySize64 / (1024 * 1024);
            }

            int free_video_memory = 0;
            int total_video_memory = 0;
            if (GLFW.ExtensionSupported("GL_NVX_gpu_memory_info") && GL.GetString(StringName.Vendor) == "NVIDIA Corporation")
            {
                total_video_memory = GL.GetInteger((GetPName)0x9048) / 1024;
                free_video_memory = total_video_memory - GL.GetInteger((GetPName)0x9049) / 1024;
            }

            if (GLFW.ExtensionSupported("GL_ATI_meminfo") && GL.GetString(StringName.Vendor) == "AMD")
            {

            }

            ImGui.GetForegroundDrawList().AddText(
                new System.Numerics.Vector2(20, 20),
                ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(150, 150, 150, 255)),
                "gl-version: " + GL.GetString(StringName.Version) + "\n" +
                "vendor: " + GL.GetString(StringName.Vendor) +
                "\n\n" +
                "window_size: " + window_size.X + " x " + window_size.Y + "\n" +
                "aspect: " + window_aspect.ToString("0.000") +
                "\n\n" +
                "viewport_camera.direction:" + "\n" +
                "X: " + viewport_camera.direction.X.ToString("0.000") + "\n" +
                "Y: " + viewport_camera.direction.Y.ToString("0.000") + "\n" +
                "Z: " + viewport_camera.direction.Z.ToString("0.000")
                +"\n\n" +
                "viewport_camera.position:" + "\n" +
                "X: " + viewport_camera.position.X.ToString("0.000") + "\n" +
                "Y: " + viewport_camera.position.Y.ToString("0.000") + "\n" +
                "Z: " + viewport_camera.position.Z.ToString("0.000")
                +"\n\n" +
                "fps: " + stats.fps.ToString("0") + "\n" +
                "latency: " + stats.ms.ToString("0.00") + " ms" +
                "\n\n" +
                "memory: " + memory + "mb" + "\n" + 
                "gpu-memory: " + free_video_memory + " / " + total_video_memory + "mb");
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
    
            switch (e.Key)
            {
                case Keys.Escape:
                {
                    Close();
                    break;
                }

                case Keys.D0:
                {
                    Camera.show_guidelines = HelperClass.ToggleBool(Camera.show_guidelines);
                    break;
                }

                case Keys.D9:
                {
                    show_statistics = HelperClass.ToggleBool(show_statistics);
                    break;
                }
            }
        }
    }
}