using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GUI;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog(string message)
    {
        InitializeComponent();
        MessageTextBlock.Text = message;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        bool? result = (sender as Button)!.CommandParameter switch
        {
            "Y" => true,
            "N" => false,
            _ => null
        };
        Close(result);
    }
}