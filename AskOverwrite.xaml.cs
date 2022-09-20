using System;
using System.Windows;
using System.Windows.Controls;

namespace Super_QOI_converter__GUI_
{
    /// <summary>
    /// Lógica de interacción para AskOverwrite.xaml
    /// </summary>
    public partial class AskOverwrite : Window
    {
        /// <summary>
        /// The options that the user can select when the program finds an existing file
        /// </summary>
        public enum Options // If DialogOptions is false, that means Skip
        {
            Rename,
            Overwrite
        }

        /// <summary>
        /// Sets the file path it will be showed in the message.
        /// </summary>
        public string FileName
        {
            set => MsgTextBlock.Text = $"{value} already exists.{Environment.NewLine}What do you want to do?";
        }

        public Options SelectedOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="AskOverwrite"/> class.
        /// </summary>
        public AskOverwrite()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Function executed when the user press any of the options button
        /// </summary>
        //TODO: Manage names instead of content to be able to localize the program
        private void BtnPressed(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button)!.Content)
            {
                case "Rename":
                    DialogResult = true;
                    SelectedOption = Options.Rename;
                    return;

                case "Skip":
                    DialogResult = false;
                    return;

                case "Overwrite":
                    DialogResult = true;
                    SelectedOption = Options.Overwrite;
                    return;
            }
        }

        /// <summary>
        /// Handles if the user closes the window instead of selecting an option
        /// </summary>
        private void OnClosingWindow(object sender, System.ComponentModel.CancelEventArgs e) =>
            DialogResult ??= false;
    }
}