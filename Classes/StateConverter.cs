using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GUI.Classes;

public class StateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return null;

        var convert = string.Concat("avares://GUI/Assets/", Enum.GetName(typeof(ConversionStateEnum), value), ".png");
        var bitmap = new Bitmap(AssetLoader.Open(new Uri(convert)));
        return bitmap;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}