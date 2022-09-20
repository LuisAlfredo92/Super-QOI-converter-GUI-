using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Core;
using Microsoft.Win32;

namespace Super_QOI_converter__GUI_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IOptionsConfirmation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            FilesListView.LayoutUpdated += UnlockButtons;
        }

        /// <summary>
        /// Function from IConfirmOptions that will confirm if the user wants to
        /// copy attributes and dates from the original files
        /// </summary>
        /// <param name="originalFile">Path to the original file.
        /// It can be null since it isn't necessary. In this case we don't use it</param>
        /// <returns>If the respective checkbox is checked</returns>
        public bool ConfirmCopy(string originalFile = "") => CopyAttributesCheckBox.IsChecked!.Value;

        /// <summary>
        /// Function from IConfirmOptions that will confirm if the user wants to
        /// delete the original files
        /// </summary>
        /// <param name="originalFile">Path to the original file.
        /// It can be null since it isn't necessary. In this case we don't use it</param>
        /// <returns>If the respective checkbox is checked</returns>
        public bool ConfirmDeletion(string originalFile = "") => DeleteOriginalFilesCheckBox.IsChecked!.Value;

        /// <summary>
        /// Function from IConfirmOptions that will confirm or ask if the user wants to
        /// overwrite existing files
        /// </summary>
        /// <param name="existingFile">Path to the existing file.
        /// It can't be null because we must show the already existing file so the user
        /// can modify it externally or see the file we're talking about</param>
        /// <returns>Depending on the option marked on the combobox, it will indicate
        /// to Core if it can continue or not, and modify the new name of the file</returns>
        public bool ConfirmOverwrite(ref string existingFile)
        {
            switch (OverwriteComboBox.SelectedIndex)
            {
                case 0: // Ask
                    var messageBox = new AskOverwrite { FileName = existingFile };
                    if (!messageBox.ShowDialog()!.Value)
                        return false;

                    if (messageBox.SelectedOption == AskOverwrite.Options.Rename)
                        existingFile = existingFile.Insert(existingFile.Length - 4, "_copy");
                    return true;

                case 1: // Skip
                    return false;

                case 2: // Rename
                    existingFile = existingFile.Insert(existingFile.Length - 4, "_copy");
                    return true;

                case 3: // overwrite
                    return true;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Manages if the Core finds a directory. Not used
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void ManageDirectory(string directoryPath) => FilesListView.Items.Add(Directory.GetFiles(directoryPath));

        /// <summary>
        /// Function called when the user press the Add files button
        /// </summary>
        private void AddFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Title = "Select files to be added",
                Filter = "Supported images|*.jpeg;*.png;*.jpg;*.bmp;*.JPEG;*.PNG;*.JPG;*.BMP"
            };
            openFileDialog.ShowDialog();
            foreach (var filePath in openFileDialog.FileNames.Where(
                         elem => !FilesListView.Items.Contains(elem)))
                FilesListView.Items.Add(filePath);
        }

        /// <summary>
        /// Function called when the user press the Clear list button
        /// </summary>
        private void ClearListBtn_Click(object sender, RoutedEventArgs e) => FilesListView.Items.Clear();

        /// <summary>
        /// Function called to remove only one element from the ListView
        /// </summary>
        private void DelItemBtn_OnClick(object sender, RoutedEventArgs e)
            => FilesListView.Items.Remove(((sender as Button)!).CommandParameter);

        /// <summary>
        /// Event that occurs when the ListView is updated to enable or disable
        /// the Clear list and Start conversion buttons
        /// </summary>
        private void UnlockButtons(object? sender, EventArgs e)
        {
            if (FilesListView.Items.Count > 0)
                ClearListBtn.IsEnabled = StartConversionBtn.IsEnabled = true;
            else
                ClearListBtn.IsEnabled = StartConversionBtn.IsEnabled = false;
        }

        /// <summary>
        /// Starts the conversion of the files from The ListView
        /// </summary>
        private void StartConversionBtn_OnClick(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < FilesListView.Items.Count;)
            {
                var item = (string)FilesListView.Items[i];
                if (Converter.ConvertToQoi(this, item))
                    FilesListView.Items.Remove(item);
                else
                    i++;
            }
        }
    }
}