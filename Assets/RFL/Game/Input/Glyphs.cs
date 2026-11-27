#nullable enable
using RFL.Game.UI;

namespace RFL.Game.Input
{
    /// <summary>Placeholder button names per device until the Steam Input glyph sets arrive (WP-D26).</summary>
    public static class Glyphs
    {
        public static string Confirm(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "A",
                DeviceKind.PlayStation => "Cross",
                _ => "Enter",
            };

        public static string Back(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "B",
                DeviceKind.PlayStation => "Circle",
                _ => "Esc",
            };

        public static string Tabs(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "LB / RB",
                DeviceKind.PlayStation => "L1 / R1",
                _ => "Q / E",
            };

        public static string Pause(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "Menu",
                DeviceKind.PlayStation => "Options",
                _ => "Esc",
            };

        public static string LeftPunch(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "X",
                DeviceKind.PlayStation => "Square",
                _ => "U",
            };

        public static string RightPunch(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "Y",
                DeviceKind.PlayStation => "Triangle",
                _ => "I",
            };

        public static string LeftKick(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "A",
                DeviceKind.PlayStation => "Cross",
                _ => "J",
            };

        public static string RightKick(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "B",
                DeviceKind.PlayStation => "Circle",
                _ => "K",
            };

        public static string Walk(DeviceKind k) =>
            k == DeviceKind.Keyboard ? "A / D" : Loc.Get("ui.glyph.stick");

        public static string DummyToggle(DeviceKind k) =>
            k switch
            {
                DeviceKind.Xbox => "View",
                DeviceKind.PlayStation => "Create",
                _ => "Tab",
            };

        public static string Change(DeviceKind k) =>
            k == DeviceKind.Keyboard ? Loc.Get("ui.glyph.arrows") : Loc.Get("ui.glyph.dpad");
    }
}
