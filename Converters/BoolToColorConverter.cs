using System.Globalization;

namespace BufeApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.Orange;
        public Color FalseColorLight { get; set; } = Colors.White;
        public Color FalseColorDark { get; set; } = Colors.Gray;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return TrueColor;
            }
            
            // Return appropriate color based on current app theme
            return Application.Current?.RequestedTheme == AppTheme.Dark 
                ? FalseColorDark 
                : FalseColorLight;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
