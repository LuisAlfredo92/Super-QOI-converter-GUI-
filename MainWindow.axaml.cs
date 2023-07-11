using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GUI.Classes;

namespace GUI;

public partial class MainWindow : Window
{
    private List<ImageToConvert> _imagesList = new();
    private uint _imageIndex;
    private readonly List<CultureInfo> _cultures;

    public MainWindow()
    {
        InitializeComponent();
        ParallelProcessesNumericUpDown.Value = ParallelProcessesNumericUpDown.Maximum = Environment.ProcessorCount;
        ImagesToConvertDataGrid.ItemsSource = _imagesList;
        AddHandler(DragDrop.DropEvent, DropFiles);

        // Set language
        _cultures = new List<CultureInfo> { CultureInfo.GetCultureInfo("en"), CultureInfo.GetCultureInfo("es") };
        List<string> languages = new();
        var tempIndex = 0;
        for (var i = 0; i < _cultures.Count; i++)
        {
            var culture = _cultures[i];
            languages.Add(culture.DisplayName);

            if (culture.TwoLetterISOLanguageName == CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                tempIndex = i;
        }

        LanguageComboBox.ItemsSource = languages;
        LanguageComboBox.SelectedIndex = tempIndex;
        LanguageComboBox.SelectionChanged += LanguageComboBox_OnSelectionChanged;
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
            // Show data grid and re-bind items
            ImagesToConvertPlaceHolder.IsVisible = false;
            ImagesToConvertDataGrid.ItemsSource = null;
            ImagesToConvertDataGrid.ItemsSource = _imagesList;
            ImagesToConvertDataGrid.IsVisible = StartConversionButton.IsEnabled = true;
            if(StartImmediatelyCheckBox.IsChecked ?? false)
                ConvertImagesInList();
        }
        else
        {
            // Hide data grid
            ImagesToConvertPlaceHolder.IsVisible = true;
            ImagesToConvertDataGrid.IsVisible = StartConversionButton.IsEnabled = false;
        }
    }

    private void ClearListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)!.CommandParameter is null) // Clears list
            _imagesList.Clear();
        else // Removes only correctly converted files
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

    private void StartConversionButton_OnClick(object? sender, RoutedEventArgs e) => ConvertImagesInList();

    private void ConvertImagesInList()
    {
        var imagesToProcess = _imagesList.Where(img => img.State != ConversionStateEnum.Correct).ToList();
        if (!imagesToProcess.Any()) return;

        StartConversionButton.Content = "Working,,,";
        StartConversionButton.IsEnabled = false;

        var processedImages = 0;
        // Get the current settings.
        ThreadPool.GetMinThreads(out _, out var minIoc);
        ThreadPool.SetMaxThreads((int)(ParallelProcessesNumericUpDown.Value ?? 1), minIoc);


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

    private void LanguageComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Assets.Resources.Culture = _cultures[LanguageComboBox.SelectedIndex];
    }
}