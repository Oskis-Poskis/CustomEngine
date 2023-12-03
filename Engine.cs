using WindowTemplate;
using WindowTemplate.Common;
using Engine.Common;
using Engine.Scene;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine
{
    public class AppWindow : HostWindow
    {
        public AppWindow(NativeWindowSettings settings, bool UseWindowState) : base(settings, UseWindowState)
        {
            shader = new Shader($"{base_directory}Shaders/mesh.vert", $"{base_directory}Shaders/mesh.frag");
            guidelines_S = new Shader($"{base_directory}Shaders/guides.vert", $"{base_directory}Shaders/guides.frag");
            viewport_camera = new Camera(new Vector3(0, 0, 5), -Vector3.UnitZ, 70, 10);
        }

        private static Camera viewport_camera;
        Mesh mesh1;

        public static Shader shader;
        public static Shader guidelines_S;
        public static StatCounter stats = new();

        public static string project_path = Environment.CurrentDirectory;
        
        protected unsafe override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.LineWidth(2.0f);

            mesh1 = new Mesh("Assets/smooth_monkey.fbx")
            {
                Rotation = new(-90, 0, 0)
            };

            Maximized += (sender) =>
            {
                viewport_camera.aspect_ratio = window_aspect;
                guidelines_S.SetFloat("aspect", window_aspect);
            };
            
            Resize += (sender) =>
            {
                viewport_camera.aspect_ratio = window_aspect;
                guidelines_S.SetFloat("aspect", window_aspect);
            };

            GLFW.SetScrollCallback(WindowPtr, Scrolling);

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

            GL.Viewport(0, 0, window_size.X, window_size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.05f, 0.05f, 0.05f, 1);

            stats.Count(args);
            viewport_camera.Update((float)args.Time);

            shader.Use();

            shader.SetMatrix4("view", viewport_camera.view);
            shader.SetMatrix4("projection", viewport_camera.projection);
            shader.SetMatrix4("model", mesh1.model_matrix);
            
            mesh1.Render();
            Camera.Render();

            Title = stats.fps.ToString("0.0") + " FPS  |  " + stats.ms.ToString("0.00") + "ms";

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
                    Camera.ShowGuideLines = HelperClass.ToggleBool(Camera.ShowGuideLines);
                    break;
                }
            }
        }
    }
}