using Microsoft.Maui.Controls.Shapes;

namespace BufeApp.Controls;

public partial class AnimatedSvgIcon : ContentView
{
    private bool _isShowingStartIcon = true;

    // 1. Define Bindable Property for the First Icon (StartIcon)
    public static readonly BindableProperty StartIconProperty = BindableProperty.Create(
        nameof(StartIcon),
        typeof(string),
        typeof(AnimatedSvgIcon),
        propertyChanged: OnStartIconChanged); // Trigger this when XAML sets the value

    public string StartIcon
    {
        get => (string)GetValue(StartIconProperty);
        set => SetValue(StartIconProperty, value);
    }

    // 2. Define Bindable Property for the Second Icon (EndIcon)
    public static readonly BindableProperty EndIconProperty = BindableProperty.Create(
        nameof(EndIcon),
        typeof(string),
        typeof(AnimatedSvgIcon));

    public string EndIcon
    {
        get => (string)GetValue(EndIconProperty);
        set => SetValue(EndIconProperty, value);
    }

    public AnimatedSvgIcon()
    {
        InitializeComponent();
    }

    // This method runs automatically when you set StartIcon in XAML
    private static void OnStartIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AnimatedSvgIcon)bindable;
        if (newValue is string pathString)
        {
            // Initialize the visible path immediately
            var converter = new PathGeometryConverter();
            control.ActivePath.Data = (Geometry)converter.ConvertFromInvariantString(pathString);
        }
    }

    public async Task Animate()
    {
        // Determine which icon is next
        string nextIconData = _isShowingStartIcon ? EndIcon : StartIcon;
        _isShowingStartIcon = !_isShowingStartIcon;

        if (string.IsNullOrEmpty(nextIconData)) return;

        // Load the new data into the hidden path
        var converter = new PathGeometryConverter();
        HiddenPath.Data = (Geometry)converter.ConvertFromInvariantString(nextIconData);

        // Reset animation states for the entering icon
        HiddenPath.Opacity = 0;
        HiddenPath.Scale = 0.5;
        HiddenPath.Rotation = -45;

        // Animate
        var fadeOut = ActivePath.FadeTo(0, 250, Easing.CubicOut);
        var scaleOut = ActivePath.ScaleTo(0.5, 250, Easing.CubicOut);

        var fadeIn = HiddenPath.FadeTo(1, 250, Easing.CubicIn);
        var scaleIn = HiddenPath.ScaleTo(1, 250, Easing.CubicOut);
        var rotateIn = HiddenPath.RotateTo(0, 250, Easing.CubicOut);

        await Task.WhenAll(fadeOut, scaleOut, fadeIn, scaleIn, rotateIn);

        // Swap references
        var temp = ActivePath;
        ActivePath = HiddenPath;
        HiddenPath = temp;
    }
}