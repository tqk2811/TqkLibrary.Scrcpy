using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using TqkLibrary.Scrcpy.Interfaces;
using TqkLibrary.Scrcpy.Control;
using System.Xml.Linq;
using TqkLibrary.Wpf.Interop.DirectX;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Media;

namespace TqkLibrary.Scrcpy.Wpf
{
    public partial class ScrcpyControlExt
    {
        static readonly FrameworkPropertyMetadataOptions _options =
            FrameworkPropertyMetadataOptions.None;
        //FrameworkPropertyMetadataOptions.AffectsMeasure |
        //FrameworkPropertyMetadataOptions.AffectsArrange |
        //FrameworkPropertyMetadataOptions.AffectsParentMeasure |
        //FrameworkPropertyMetadataOptions.AffectsParentArrange;

        public static readonly DependencyProperty ScrcpyUiViewProperty = DependencyProperty.RegisterAttached(
            "ScrcpyUiView",
            typeof(ScrcpyUiView),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(null, _options, ScrcpyUiViewPropertyChangedCallback)
            );
        public static void SetScrcpyUiView(DependencyObject element, ScrcpyUiView value)
        {
            element.SetValue(ScrcpyUiViewProperty, value);
        }
        public static ScrcpyUiView? GetScrcpyUiView(DependencyObject element)
        {
            return element.GetValue(ScrcpyUiViewProperty) as ScrcpyUiView;
        }
        static void ScrcpyUiViewPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image image)
            {
                if (image.Source is not ScrcpyControlExt)
                {
                    image.Source = new ScrcpyControlExt(image);
                }
                ScrcpyControlExt d3D11Image = (ScrcpyControlExt)image.Source;
                if (e.OldValue is not null) d3D11Image._renderHandler.ClearView();
                if (e.NewValue is not null) d3D11Image._renderHandler.RegisterView();
            }
        }



        public static readonly DependencyProperty ControlProperty = DependencyProperty.Register(
            "Control",
            typeof(IControl),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(null, _options)
            );
        public static void SetControl(DependencyObject element, IControl value)
        {
            element.SetValue(ControlProperty, value);
        }
        public static IControl? GetControl(DependencyObject element)
        {
            return element.GetValue(ControlProperty) as IControl;
        }



        public static readonly DependencyProperty IsControlProperty = DependencyProperty.Register(
            "IsControl",
            typeof(bool),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(true, _options));
        public static void SetIsControl(DependencyObject element, bool value)
        {
            element.SetValue(IsControlProperty, value);
        }
        public static bool GetIsControl(DependencyObject element)
        {
            return (bool)element.GetValue(IsControlProperty);
        }



        public static readonly DependencyProperty IsMouseHandlerProperty = DependencyProperty.Register(
            "IsMouseHandler",
            typeof(bool),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(false, _options));
        public static void SetIsMouseHandler(DependencyObject element, bool value)
        {
            element.SetValue(IsMouseHandlerProperty, value);
        }
        public static bool GetIsMouseHandler(DependencyObject element)
        {
            return (bool)element.GetValue(IsMouseHandlerProperty);
        }



        public static readonly DependencyProperty IsKeyHandlerProperty = DependencyProperty.Register(
            "IsKeyHandler",
            typeof(bool),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(false, _options));
        public static void SetIsKeyHandler(DependencyObject element, bool value)
        {
            element.SetValue(IsKeyHandlerProperty, value);
        }
        public static bool GetIsKeyHandler(DependencyObject element)
        {
            return (bool)element.GetValue(IsKeyHandlerProperty);
        }



        public static readonly DependencyProperty MousePointerIdProperty = DependencyProperty.Register(
            "MousePointerId",
            typeof(long),
            typeof(ScrcpyControlExt),
            new FrameworkPropertyMetadata(ScrcpyMousePointerId.POINTER_ID_GENERIC_FINGER, _options));
        public static void SetMousePointerId(DependencyObject element, long value)
        {
            element.SetValue(MousePointerIdProperty, value);
        }
        public static long GetMousePointerId(DependencyObject element)
        {
            return (long)element.GetValue(MousePointerIdProperty);
        }

    }
}
