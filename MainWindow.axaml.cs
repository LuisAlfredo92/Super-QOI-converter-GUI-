using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GUI.Classes;

namespace GUI;

public partial class MainWindow : Window
{
    private List<ImageToConvert> _imagesList = new();
    private uint _imageIndex;
    public MainWindow()
    {
        InitializeComponent();
        ImagesToConvertDataGrid.ItemsSource = _imagesList;
        AddHandler(DragDrop.DropEvent, DropFiles);
    }

    private async void AddFilesButton_OnClick(object? sender, RoutedEventArgs e)
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
            ImagesToConvertDataGrid.ItemsSource = null;
            ImagesToConvertDataGrid.ItemsSource = _imagesList;
            ImagesToConvertDataGrid.IsVisible = StartConversionButton.IsEnabled = true;
        }
        else
        {
            ImagesToConvertPlaceHolder.IsVisible = true;
            ImagesToConvertDataGrid.IsVisible = StartConversionButton.IsEnabled = false;
        }
    }

    private void ClearListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)!.CommandParameter is null)
            _imagesList.Clear();
        else
            _imagesList = _imagesList.Where(image => image.State != ConversionStateEnum.Correct).ToList();
        ValidateFilesList();
    }

    private void DropFiles(object? sender, DragEventArgs e)
    {
        var data = e.Data;

        // Checks if the user dropped files correctly
        if (!data.Contains(DataFormats.Files)) return;
        var files = data.GetFiles();
        if (files is null) return;

        List<string> paths = files.Select(file => file.Path.AbsolutePath).ToList();

        // Add selected files to list
        foreach (var path in paths)
            _imagesList.Add(new ImageToConvert(path, _imageIndex++));

        ValidateFilesList();
    }

    private void StartConversionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var imagesToProcess = _imagesList.Where(img => img.State != ConversionStateEnum.Correct).ToList();
        if(!imagesToProcess.Any()) return;
        
        StartConversionButton.Content = "Working,,,";
        StartConversionButton.IsEnabled = false;
        var processedImages = 0;
        ThreadPool.SetMaxThreads(4, 4);
        foreach (var image in imagesToProcess)
        {
            image.State = ConversionStateEnum.Waiting;
            ThreadPool.QueueUserWorkItem((data) =>
            {
                Convert(image);
                processedImages++;
                Dispatcher.UIThread.Post(() =>
                {
                    ImagesToConvertDataGrid.ItemsSource = null;
                    ImagesToConvertDataGrid.ItemsSource = _imagesList;
                    
                    if (processedImages < imagesToProcess.Count) return;
                    StartConversionButton.Content = "Start conversion";
                    StartConversionButton.IsEnabled = true;
                });
            });
        }
        ImagesToConvertDataGrid.ItemsSource = null;
        ImagesToConvertDataGrid.ItemsSource = _imagesList;
    }

    private static void Convert(object? data)
    {
        if(data is not ImageToConvert image) return;

        // TODO: Check if file is an image
        image.State = ConversionStateEnum.Working;
        Thread.Sleep(5000);
        image.State = ConversionStateEnum.Correct;
    }
}