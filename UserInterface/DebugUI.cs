using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

using WindowTemplate;

using ImGuiNET;
using SN = System.Numerics;

namespace Engine.Debug
{
    public class Debug
    {
        public static void RenderStatistics()
        {
            float memory = 0.0f;
            using (System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess()) memory = proc.PrivateMemorySize64 / (1024.0f * 1024 * 1024);

            string gpu_memory = "";
            if (GLFW.ExtensionSupported("GL_NVX_gpu_memory_info") && GL.GetString(StringName.Vendor) == "NVIDIA Corporation")
            {
                float total_video_memory = GL.GetInteger((GetPName)0x9048) / (1024.0f * 1024);
                float free_video_memory = total_video_memory - GL.GetInteger((GetPName)0x9049) / (1024.0f * 1024);
                gpu_memory = "gpu-memory: " + free_video_memory.ToString("0.00") + "/" + total_video_memory.ToString("0.00") + "gb";
            }

            if (GLFW.ExtensionSupported("GL_ATI_meminfo") && GL.GetString(StringName.Vendor) == "AMD")
            {
                gpu_memory =
                "vbo-free-memory: " + GL.GetInteger((GetPName)0x87FB) / 1024 + "mb\n" +
                "texture-free-memory: " + GL.GetInteger((GetPName)0x87FC) / 1024 + "mb\n" +
                "renderbuffer-free-memory: " + GL.GetInteger((GetPName)0x87FD) / 1024 + "mb";
            }

            System.Text.StringBuilder string_builder = new System.Text.StringBuilder();

            string_builder.AppendLine($"renderer: {GL.GetString(StringName.Renderer)}");
            string_builder.AppendLine($"gl-version: {GL.GetString(StringName.Version)}");
            string_builder.AppendLine($"vendor: {GL.GetString(StringName.Vendor)}");
            string_builder.AppendLine();
            string_builder.AppendLine($"window_size: {HostWindow.window_size.X} x {HostWindow.window_size.Y}");
            string_builder.AppendLine($"aspect: {HostWindow.window_aspect.ToString("0.000")}");
            string_builder.AppendLine();
            string_builder.AppendLine("viewport_camera.direction:");
            string_builder.AppendLine($"X: {AppWindow.viewport_camera.direction.X.ToString("0.000")}");
            string_builder.AppendLine($"Y: {AppWindow.viewport_camera.direction.Y.ToString("0.000")}");
            string_builder.AppendLine($"Z: {AppWindow.viewport_camera.direction.Z.ToString("0.000")}");
            string_builder.AppendLine();
            string_builder.AppendLine("viewport_camera.position:");
            string_builder.AppendLine($"X: {AppWindow.viewport_camera.position.X.ToString("0.000")}");
            string_builder.AppendLine($"Y: {AppWindow.viewport_camera.position.Y.ToString("0.000")}");
            string_builder.AppendLine($"Z: {AppWindow.viewport_camera.position.Z.ToString("0.000")}");
            string_builder.AppendLine();
            string_builder.AppendLine($"fps: {AppWindow.stats.fps.ToString("0")}");
            string_builder.AppendLine($"latency: {AppWindow.stats.ms.ToString("0.00")} ms");
            string_builder.AppendLine();
            string_builder.AppendLine($"memory: {memory.ToString("0.00")}gb");
            string_builder.Append(gpu_memory);

            ImGui.GetForegroundDrawList().AddText(
                new SN.Vector2(20, 20),
                ImGui.ColorConvertFloat4ToU32(new SN.Vector4(150, 150, 150, 255)),
                string_builder.ToString()
            );
        }
    }
}