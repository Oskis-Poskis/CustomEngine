using WindowTemplate;
using WindowTemplate.Common;
using Engine.Common;
using Engine.Scene;
using Engine.UI;
using Engine.Debug;

using ImGuiNET;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

using SN = System.Numerics;

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
        private bool show_statistics = true;

        public static Camera viewport_camera;
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
            GLFW.SetScrollCallback(WindowPtr, Scrolling);

            mesh1 = new Mesh("Assets/smooth_monkey.fbx")
            {
                Rotation = new(-90, 0, 0)
            };

            for (int X = 0; X < 4; X++)
            {
                for (int Y = 0; Y < 4; Y++)
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

            UIController = new UI.ImGuiController(window_size.X, window_size.Y);
            ImGui.GetIO().FontGlobalScale = 1.25f;  
            UserInterface.LoadTheme();

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
            GL.ClearColor(0.03f, 0.03f, 0.03f, 1);

            stats.Count(args);
            viewport_camera.Update((float)args.Time);

            default_S.Use();

            default_S.SetMatrix4("view", viewport_camera.view);
            default_S.SetMatrix4("projection", viewport_camera.projection);
            default_S.SetMatrix4("model", mesh1.model_matrix);
            
            mesh1.Render();
            foreach (Mesh mesh in test_meshes)
            {
                default_S.SetMatrix4("model", mesh.model_matrix);
                mesh.Render();
            }

            Camera.Render();

            UIController.Update(this, (float)args.Time);

            if (show_statistics) Debug.Debug.RenderStatistics();

            if (ImGui.Begin("Buffer Debug", ImGuiWindowFlags.NoResize))
            {
                float ratio = 0.25f;
                ImGui.SetWindowSize(new(window_size.X * ratio, window_size.Y * ratio + ImGui.GetFontSize() + 14));
                // GL.BindTexture(TextureTarget.Texture2D, depthMap);
                ImGui.GetForegroundDrawList().AddText(
                    ImGui.GetWindowPos() + new SN.Vector2(12, ImGui.GetFontSize() + 14),
                    ImGui.ColorConvertFloat4ToU32(new SN.Vector4(150, 150, 150, 255)),
                    "Stencil"
                );
                ImGui.Image(nint.Zero, new(window_size.X * ratio, window_size.Y * ratio), new(0, 1), new(1, 0));
                ImGui.End();
            }

            UIController.Render();

            SwapBuffers();
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