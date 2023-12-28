using ImGuiNET;
using SN = System.Numerics;

namespace Engine.UI
{
    public class UserInterface
    {
        public static void LoadTheme()
        {
            ImGui.GetStyle().FrameRounding = 2;
            ImGui.GetStyle().FrameBorderSize = 0;
            ImGui.GetStyle().FramePadding = new SN.Vector2(4);
            ImGui.GetStyle().ChildBorderSize = 0;
            ImGui.GetStyle().CellPadding = new SN.Vector2(3, 3);
            ImGui.GetStyle().ItemSpacing = new SN.Vector2(4, 2);
            ImGui.GetStyle().ItemInnerSpacing = new SN.Vector2(0, 4);
            ImGui.GetStyle().WindowPadding = new SN.Vector2(2, 2);
            ImGui.GetStyle().TabRounding = 2;
            ImGui.GetStyle().ColorButtonPosition = ImGuiDir.Left;
            ImGui.GetStyle().WindowRounding = 6;
            ImGui.GetStyle().WindowBorderSize = 0;
            ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.None;
            ImGui.GetStyle().SelectableTextAlign = new(0.02f, 0);
            ImGui.GetStyle().PopupBorderSize = 0;
            ImGui.GetStyle().PopupRounding = 6;
            ImGui.GetStyle().GrabMinSize = 15;
            ImGui.GetStyle().GrabRounding = 2;
            
            ImGui.PushStyleColor(ImGuiCol.Text, new SN.Vector4(230, 230, 230, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.Border, new SN.Vector4(65, 65, 65, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new SN.Vector4(50, 50, 50, 200f) / 255);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, new SN.Vector4(150f, 150f, 150f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.PopupBg, new SN.Vector4(20, 20, 20, 255) / 255);

            // Background color
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new SN.Vector4(20f, 20f, 20f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new SN.Vector4(45f, 45f, 45f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new SN.Vector4(40f, 40f, 40f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new SN.Vector4(80f, 80f, 80f, 255f) / 255);

            // Popup BG
            ImGui.PushStyleColor(ImGuiCol.ModalWindowDimBg, new SN.Vector4(30f, 30f, 30f, 150f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TextDisabled, new SN.Vector4(150f, 150f, 150f, 255f) / 255);

            // Titles
            ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new SN.Vector4(14f, 14f, 15f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TitleBg, new SN.Vector4(14f, 14f, 14f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, new SN.Vector4(14f, 14f, 14f, 255f) / 255);

            // Tabs
            ImGui.PushStyleColor(ImGuiCol.Tab, new SN.Vector4(15f, 15f, 15f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TabActive, new SN.Vector4(35f, 35f, 35f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TabUnfocused, new SN.Vector4(15f, 15f, 15f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, new SN.Vector4(35f, 35f, 35f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.TabHovered, new SN.Vector4(80f, 80f, 80f, 255f) / 255);
            
            // Header
            ImGui.PushStyleColor(ImGuiCol.Header, new SN.Vector4(40, 40, 40, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new SN.Vector4(100, 100, 100, 180f) / 255);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new SN.Vector4(70, 70, 70, 255f) / 255);

            // Rezising bar
            ImGui.PushStyleColor(ImGuiCol.Separator, new SN.Vector4(30f, 30f, 30f, 255) / 255);
            ImGui.PushStyleColor(ImGuiCol.SeparatorHovered, new SN.Vector4(100f, 100f, 100f, 255) / 255);
            ImGui.PushStyleColor(ImGuiCol.SeparatorActive, new SN.Vector4(120f, 120f, 120f, 255) / 255);

            // Buttons
            ImGui.PushStyleColor(ImGuiCol.Button, new SN.Vector4(0.343f, 0.343f, 0.343f, 0.784f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new SN.Vector4(0.343f, 0.343f, 0.343f, 0.784f) * 1.25f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new SN.Vector4(0.343f, 0.343f, 0.343f, 0.784f) * 0.9f);

            // Docking and rezise
            // ImGui.PushStyleColor(ImGuiCol.DockingPreview, new System.Numerics.Vector4(30, 140, 120, 255) / 255);
            ImGui.PushStyleColor(ImGuiCol.DockingPreview, new SN.Vector4(100, 100, 100, 255) / 255);
            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, new SN.Vector4(217, 35, 35, 255) / 255);
            ImGui.PushStyleColor(ImGuiCol.ResizeGripHovered, new SN.Vector4(217, 35, 35, 200) / 255);
            ImGui.PushStyleColor(ImGuiCol.ResizeGripActive, new SN.Vector4(217, 35, 35, 150) / 255);
            ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, new SN.Vector4(20, 20, 20, 255) / 255);

            // Sliders, buttons, etc
            ImGui.PushStyleColor(ImGuiCol.SliderGrab, new SN.Vector4(115f, 115f, 115f, 255f) / 255);
            ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, new SN.Vector4(180f, 180f, 180f, 255f) / 255);
        }
    }
}