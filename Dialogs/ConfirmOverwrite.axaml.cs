using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GUI.Classes;

namespace GUI.Dialogs;

public partial class ConfirmOverwrite : Window
{
    public ConfirmOverwrite(string existingFile)
    {
        InitializeComponent();
        MessageTextBlock.Text = string.Format(Assets.Resources.Already_exists, existingFile);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedValue = Enum.Parse <OverwriteOptionsEnum>((sender as Button)!.CommandParameter!.ToString()!);
        Close((selectedValue, DontAskAgainCheckBox.IsChecked ?? false));
    }
}