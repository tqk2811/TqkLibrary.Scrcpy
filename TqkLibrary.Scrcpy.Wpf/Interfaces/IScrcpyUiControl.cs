using System.Windows.Controls;
using TqkLibrary.Scrcpy.Interfaces;

namespace TqkLibrary.Scrcpy.Wpf.Interfaces
{
    public interface IScrcpyUiControl : IScrcpyControlBinding
    {
        Image Image { get; }
        System.Drawing.Size VideoSize { get; }
        System.Drawing.Rectangle DrawRect { get; }
    }
}
