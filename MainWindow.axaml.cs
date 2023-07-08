using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace GUI;

public partial class MainWindow : Window
{
    private List<ImageToConvert> _imagesList = new();
    private uint _imageIndex = 0;
    public MainWindow()
    {
        InitializeComponent();
        ImagesToConvertDataGrid.ItemsSource = _imagesList;
    }

    private async void AddFilesOrFoldersButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Show file picker dialog
        FilePickerFileType filter = new("Images")
        {
            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.pbm", "*.pgm", "*.ppm", "*tga", "*.tiff", "*.webp" }
        };
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select images to convert to QOI",
            AllowMultiple = true,
            FileTypeFilter = new[] { filter }
        });
        if (files.Count < 1) return;

        // Check if the user selected GIF images
        if (files.Any(file => Path.GetExtension(file.Path.AbsolutePath).EndsWith("gif")))
        {
            var confirmDialog = new ConfirmDialog(Assets.Resources.GIF_files_confirmation);
            var a = await confirmDialog.ShowDialog<bool?>(this);
            switch (a)
            {
                case true:
                    files = files.Where(file => !Path.GetExtension(file.Path.AbsolutePath).EndsWith("gif")).ToList();
                    break;
                case null:
                    return;
            }
        }
        
        // Add selected files to list
        foreach (var file in files) 
            _imagesList.Add(new ImageToConvert(file.Path.AbsolutePath, _imageIndex++));

        ValidateFilesList();
    }

    private void ValidateFilesList()
    {
        if (_imagesList.Any())
        {
            ImagesToConvertPlaceHolder.IsVisible = false;
            ImagesToConvertDataGrid.IsVisible = true;
            ImagesToConvertDataGrid.ItemsSource = null;
            ImagesToConvertDataGrid.ItemsSource = _imagesList;
        }
        else
        {
            ImagesToConvertPlaceHolder.IsVisible = true;
            ImagesToConvertDataGrid.IsVisible = false;
        }
    }

    private void ClearListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)!.CommandParameter is null)
            _imagesList.Clear();
        else
            _imagesList = _imagesList.Where(image => image.State != ConversionState.Correct).ToList();
        ValidateFilesList();
    }
}