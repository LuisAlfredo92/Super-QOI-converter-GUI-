using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GUI.Classes;
using Core;
using GUI.Dialogs;

namespace GUI;

public partial class MainWindow : Window, IOptionsConfirmation
{
    private List<ImageToConvert> _imagesList = new();
    private uint _imageIndex;
    private readonly List<CultureInfo> _cultures;
    private bool _confirmCopy;
    private bool _deleteOriginal;

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
        if (files.Any(file => Path.GetExtension(file.Path.LocalPath).EndsWith("gif")))
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

        List<string> paths = files.Select(file => file.Path.LocalPath).ToList();

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
        _confirmCopy = CopyAttributesCheckBox.IsChecked ?? true;
        _deleteOriginal = DeleteOriginalCheckBox.IsChecked ?? false;

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

    private void Convert(object? data)
    {
        if(data is not ImageToConvert image) return;

        // TODO: Check if file is an image
        image.State = ConversionStateEnum.Working;
        if(Converter.ConvertToQoi(this, image.FilePath))
            image.State = ConversionStateEnum.Correct;
        else
        {
            image.State = ConversionStateEnum.Error;
            image.ToolTipMessage = "There was an error during conversion";
        }
    }

    private void LanguageComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Assets.Resources.Culture = _cultures[LanguageComboBox.SelectedIndex];
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if((sender as Button)!.CommandParameter is not uint id) return;
        _imagesList.Remove(_imagesList.First(image => image.Id == id));
        ValidateFilesList();
    }

    public bool ConfirmCopy(string originalFile = "") => _confirmCopy;

    public bool ConfirmDeletion(string originalFile = "") => _deleteOriginal;

    public bool ConfirmOverwrite(ref string existingFile)
    {
        switch (ExistingFilesComboBox.SelectedIndex)
        {
            // Ask
            case 0:
                var fileName = Path.GetFileName(existingFile);
                var showDialog = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    ConfirmOverwrite confirmOverwriteDialog = new(fileName);
                    return await confirmOverwriteDialog.ShowDialog<(OverwriteOptionsEnum, bool)?>(this);
                });

                var selectedValueTuple = showDialog.Result;
                if (selectedValueTuple is null) return false;

                switch (selectedValueTuple.Value.Item1)
                {
                    case OverwriteOptionsEnum.Skip:
                        return false;
                        case OverwriteOptionsEnum.Rename:
                            existingFile = existingFile.Insert(existingFile.Length - 4, "_copy");
                        return true;
                    case OverwriteOptionsEnum.Overwrite:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            // Skip
            case 1:
                return false;

            // Rename
            case 2:
                existingFile = existingFile.Insert(existingFile.Length - 4, "_copy");
                return true;

            // Overwrite
            case 3: return true;
        }

        return false;
    }

    public void ManageDirectory(string directoryPath)
    {
        throw new NotImplementedException();
    }
}