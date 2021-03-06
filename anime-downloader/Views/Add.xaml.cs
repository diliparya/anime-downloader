﻿using System.Windows;
using System.Windows.Input;

namespace anime_downloader.Views {
    /// <summary>
    ///     Interaction logic for Add.xaml
    /// </summary>
    public partial class Add {
        public Add() {
            InitializeComponent();
        }

        private void episode_textbox_GotFocus(object sender, RoutedEventArgs e) {
            EpisodeTextbox.SelectAll();
        }

        private void name_textbox_GotFocus(object sender, RoutedEventArgs e) {
            NameTextbox.SelectAll();
        }

        private void episode_textbox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }
    }
}