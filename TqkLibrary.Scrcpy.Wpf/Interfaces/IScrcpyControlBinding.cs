using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Wpf.Interfaces
{
    public interface IScrcpyControlBinding
    {
        ScrcpyUiView? ScrcpyUiView { get; }
        IControl? Control { get; }
        bool IsControl { get; }
        bool IsMouseHandler { get; }
        bool IsKeyHandler { get; }
        long MousePointerId { get; }
    }
}
