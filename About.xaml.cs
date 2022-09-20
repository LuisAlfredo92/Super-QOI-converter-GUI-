﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Super_QOI_converter__GUI_
{
    /// <summary>
    /// Lógica de interacción para About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void GoToLink(object sender, RoutedEventArgs e)
        {
            var link = (sender as Button)!.Content switch
            {
                "QoiSharp" => "https://github.com/NUlliiON/QoiSharp",
                "NUlliiON" => "https://github.com/NUlliiON",
                "StbImageSharp" => "https://github.com/StbSharp/StbImageSharp",
                "StbSharp" => "https://github.com/StbSharp",
                _ => "https://github.com/LuisAlfredo92"
            };
            var psi = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}