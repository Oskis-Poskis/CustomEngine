using WindowTemplate;
using WindowTemplate.Common;

using Engine.UI;
using Engine.Scene;
using Engine.Common;

using ImGuiNET;

using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine
{
    public class AppWindow : HostWindow
    {
        public AppWindow(NativeWindowSettings settings, bool UseWindowState) : base(settings, UseWindowState)
        {
            default_S = new Shader($"{base_directory}Shaders/mesh.vert", $"{base_directory}Shaders/mesh.frag");
            viewport_camera = new Camera(new Vector3(0, 0, 5), -Vector3.UnitZ, 70, 10);
            viewport_size = window_size;
            prev_viewport_size = viewport_size;
        }

        public static Camera viewport_camera;
        public static Shader default_S;
        public static StatCounter stats = new();
        private UI.ImGuiController UIController;

        List<Mesh> test_meshes = new();
        
        int viewport_fbo;
        int viewport_texture, viewport_depth_texture;
        Vector2i viewport_size, prev_viewport_size;

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

            viewport_fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewport_fbo);
            
            viewport_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, viewport_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, window_size.X, window_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, viewport_texture, 0);

            viewport_depth_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, viewport_depth_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, window_size.X, window_size.X, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, viewport_depth_texture, 0);

            if (!(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete)) Console.WriteLine("Framebuffer error :(");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            for (int X = 0; X < 4; X++)
            {
                for (int Y = 0; Y < 4; Y++)
                {
                    Mesh test_mesh = new Mesh("Assets/smooth_monkey.fbx")
                    {
                        Rotation = new(-90, 0, 0),
                        Position = new Vector3(X * 5.0f, 0.0f, Y * 5.0f)
                    };

                    test_meshes.Add(test_mesh);
                }   
            }

            Maximized += (sender) =>
            {
                UIController.WindowResized(window_size.X, window_size.Y);
            };
            
            Resize += (sender) =>
            {
                UIController.WindowResized(window_size.X, window_size.Y);
            };

            UIController = new UI.ImGuiController(window_size.X, window_size.Y);
            ImGui.GetIO().FontGlobalScale = 1.25f;  
            UserInterface.LoadTheme();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            viewport_camera.Move((float)args.Time, KeyboardState);

            bool leftalt_down = IsKeyDown(Keys.LeftAlt);

            CursorState = (IsMouseButtonDown(MouseButton.Button2) || IsMouseButtonDown(MouseButton.Button3) || leftalt_down) ? CursorState.Grabbed : CursorState.Normal;

            if (IsMouseButtonDown(MouseButton.Button3)) viewport_camera.Input(new Vector2(MouseState.Delta.X, MouseState.Delta.Y), true);
            else if (leftalt_down)
            {
               if(IsMouseButtonDown(MouseButton.Button2)) viewport_camera.Zoom(MouseState.Delta.X * 0.1f);
            }
            else if (IsMouseButtonDown(MouseButton.Button2)) viewport_camera.Input(MouseState.Delta, false);
        }

        private int selected_texture = 0;
        private readonly string[] texture_options = {"Combined", "Depth"};

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            // Viewport pass
            GL.Viewport(0, 0, viewport_size.X, viewport_size.Y);
            if (viewport_size != prev_viewport_size)
            {
                GL.BindTexture(TextureTarget.Texture2D, viewport_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, viewport_size.X, viewport_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

                GL.BindTexture(TextureTarget.Texture2D, viewport_depth_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, viewport_size.X, viewport_size.X, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

                viewport_camera.aspect_ratio = (float)viewport_size.X / viewport_size.Y;
                prev_viewport_size = viewport_size;
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewport_fbo);
            GL.ClearColor(0.03f, 0.03f, 0.03f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            stats.Count(args);
            viewport_camera.Update((float)args.Time);

            default_S.Use();
            default_S.SetVector3("viewDir", viewport_camera.direction);
            default_S.SetMatrix4("view", viewport_camera.view);
            default_S.SetMatrix4("projection", viewport_camera.projection);
            
            foreach (Mesh mesh in test_meshes)
            {
                default_S.SetMatrix4("model", mesh.model_matrix);
                mesh.Render();
            }

            // UI pass
            GL.Viewport(0, 0, window_size.X, window_size.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UIController.Update(this, (float)args.Time);
            ImGui.DockSpaceOverViewport();

            ImGui.BeginMainMenuBar();
            ImGui.SetNextItemWidth(175);
            if (ImGui.Combo("##texture", ref selected_texture, texture_options, texture_options.Length));
            ImGui.EndMainMenuBar();

            ImGui.Begin("Viewport");
            viewport_size = new(
                Convert.ToInt32(MathHelper.Abs(ImGui.GetWindowContentRegionMin().X - ImGui.GetWindowContentRegionMax().X)),
                Convert.ToInt32(MathHelper.Abs(ImGui.GetWindowContentRegionMin().Y - ImGui.GetWindowContentRegionMax().Y)));
            switch (selected_texture)
            {
                case 0:
                    ImGui.Image(viewport_texture, new(viewport_size.X, viewport_size.Y), new(0, 1), new(1, 0), new(1), new(0));
                    break;
                
                case 1:
                    ImGui.Image(viewport_depth_texture, new(viewport_size.X, viewport_size.Y), new(0, 1), new(1, 0), new(1), new(0));
                    break;
            }
            ImGui.End();

            ImGui.Begin("Outliner");
            ImGui.End();

            UIController.Render();
            SwapBuffers();
        }

        private static unsafe void Scrolling(Window* window, double offsetX, double offsetY)
        {
            viewport_camera.Zoom((float)offsetY);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
    
            switch (e.Key)
            {
                case Keys.Escape:
                    Close();
                    break;
            }
        }
    }
}