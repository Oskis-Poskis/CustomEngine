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
            postprocess_S = new ComputeShader($"{base_directory}Shaders/postprocess.comp");

            viewport_camera = new Camera(
                new Vector3(
                    0.0f,
                    (float)MathHelper.Sin(MathHelper.DegreesToRadians(45)) * 5,
                    (float)MathHelper.Sin(MathHelper.DegreesToRadians(45)) * 5),
                new Vector3(
                    0.0f,
                    -(float)MathHelper.Sin(MathHelper.DegreesToRadians(45)),
                    -(float)MathHelper.Sin(MathHelper.DegreesToRadians(45))),
                70, 10);
            viewport_size = window_size;
            prev_viewport_size = viewport_size;

            test_gear = new(8, 1.0f, 1.5f, 0.2f);
        }

        public static Camera viewport_camera;
        public static Shader default_S;
        public static ComputeShader postprocess_S;

        public static StatCounter stats = new();
        private UI.ImGuiController UIController;

        private Mesh test_mesh;
        private Gear test_gear;
        
        int viewport_fbo;
        int viewport_texture, normal_texture, viewport_depth_texture, postprocess_texture;
        Vector2i viewport_size, prev_viewport_size;

        protected unsafe override void OnLoad()
        {
            base.OnLoad();

            DllResolver.InitLoader();

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.LineWidth(5.0f);
            GL.PointSize(5.0f);
            GLFW.SetScrollCallback(WindowPtr, Scrolling);

            viewport_fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewport_fbo);
            
            viewport_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, viewport_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, window_size.X, window_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, viewport_texture, 0);

            normal_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, normal_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, window_size.X, window_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, normal_texture, 0);

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

            postprocess_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, postprocess_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, window_size.X, window_size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindImageTexture(0, postprocess_texture, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);

            test_mesh = new Mesh("Assets/smooth_monkey.fbx")
            {
                Rotation = new(-90, 0, 0)
            };

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

            CursorState = ((IsMouseButtonDown(MouseButton.Button1) && leftalt_down) || (IsMouseButtonDown(MouseButton.Button2) && leftalt_down) || IsMouseButtonDown(MouseButton.Button3)) ? CursorState.Grabbed : CursorState.Normal;

            if (IsMouseButtonDown(MouseButton.Button3)) viewport_camera.Input(new Vector2(MouseState.Delta.X, MouseState.Delta.Y), true);
            else if (leftalt_down)
            {
               if (IsMouseButtonDown(MouseButton.Button1)) viewport_camera.Input(new Vector2(MouseState.Delta.X, MouseState.Delta.Y), true);
               else if(IsMouseButtonDown(MouseButton.Button2)) viewport_camera.Zoom(MouseState.Delta.X * 0.02f);
            }
            // else if (IsMouseButtonDown(MouseButton.Button2)) viewport_camera.Input(MouseState.Delta, false);
        }

        bool backface_culling = true;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            // Viewport pass
            GL.Viewport(0, 0, viewport_size.X, viewport_size.Y);
            if (viewport_size != prev_viewport_size)
            {
                GL.BindTexture(TextureTarget.Texture2D, viewport_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, viewport_size.X, viewport_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

                GL.BindTexture(TextureTarget.Texture2D, normal_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, viewport_size.X, viewport_size.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

                GL.BindTexture(TextureTarget.Texture2D, viewport_depth_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, viewport_size.X, viewport_size.X, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

                GL.BindTexture(TextureTarget.Texture2D, postprocess_texture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, viewport_size.X, viewport_size.Y, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

                viewport_camera.aspect_ratio = (float)viewport_size.X / viewport_size.Y;
                prev_viewport_size = viewport_size;
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, viewport_fbo);
            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.ClearColor(0.075f, 0.075f, 0.075f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            stats.Count(args);
            viewport_camera.Update((float)args.Time);

            default_S.Use();
            // default_S.SetVector3("viewDir", viewport_camera.direction);
            default_S.SetMatrix4("view", viewport_camera.view);
            default_S.SetMatrix4("projection", viewport_camera.projection);
            default_S.SetMatrix4("model", test_mesh.model_matrix);
            // test_mesh.Render();
            test_gear.Render();

            GL.BindImageTexture(0, viewport_texture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(1, normal_texture, 0, false, 0, TextureAccess.ReadOnly, SizedInternalFormat.Rgba32f);
            GL.BindImageTexture(2, postprocess_texture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);            

            postprocess_S.Use();
            GL.DispatchCompute(viewport_size.X / 16, viewport_size.Y / 16, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            // UI pass
            GL.Viewport(0, 0, window_size.X, window_size.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            UIController.Update(this, (float)args.Time);
            ImGui.DockSpaceOverViewport();



            ImGui.Begin("Viewport");
            viewport_size = new(
                Convert.ToInt32(MathHelper.Abs(ImGui.GetWindowContentRegionMin().X - ImGui.GetWindowContentRegionMax().X)),
                Convert.ToInt32(MathHelper.Abs(ImGui.GetWindowContentRegionMin().Y - ImGui.GetWindowContentRegionMax().Y)));
            ImGui.Image(postprocess_texture, new(viewport_size.X, viewport_size.Y), new(0, 1), new(1, 0), new(1), new(0));
            ImGui.End();

            ImGui.Begin("Settings");

            if (ImGui.Checkbox(" Backface Culling", ref backface_culling))
            {
                if (backface_culling) GL.Enable(EnableCap.CullFace);
                else GL.Disable(EnableCap.CullFace);
            }

            ImGui.End();




            ImGui.Begin("Gear Properties");
            ImGui.BeginTable("##Table", 2, ImGuiTableFlags.SizingStretchProp);
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Teeth");

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            int stored_teeth = (int)test_gear.teeth_count;
            if (ImGui.SliderInt("##Teeth", ref stored_teeth, 2, 64))
            {
                test_gear.teeth_count = (uint)stored_teeth;
                test_gear.UpdateGearData();
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.SliderFloat("##InnerRadius", ref test_gear.inner_radius, 0.0f, 5.0f))
            {
                test_gear.UpdateGearData();
            }

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Inner Radius");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.SliderFloat("##OuterRadius", ref test_gear.outer_radius, 0.0f, 5.0f))
            {
                test_gear.UpdateGearData();
            }

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Outer Radius");

            ImGui.EndTable();

            ImGui.Dummy(new(0.0f, 20.0f));
            ImGui.Text("Info:");
            ImGui.Text("Gear Vertices: " + test_gear.vertice_count);
            ImGui.Text("Gear Triangles: " + test_gear.triangle_count);
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

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            UIController.PressChar((char)e.Unicode);
        }
    }
}