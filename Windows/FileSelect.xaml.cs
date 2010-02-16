﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using IOPath = System.IO.Path;

namespace Mirosubs.Converter.Windows {
    public partial class FileSelect : UserControl {
        internal event EventHandler<VideoSelectedEventArgs> FileSelected;

        private string selectedFileName = null;

        public FileSelect() {
            InitializeComponent();
            this.videoFormatCombo.ItemsSource = VideoFormat.All;
            this.videoFormatCombo.SelectedValuePath = "Id";
            this.videoFormatCombo.DisplayMemberPath = "DisplayName";
        }
        private void FilesDropped(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] droppedFilePaths =
                    (string[])e.Data.GetData(DataFormats.FileDrop, true);
                if (droppedFilePaths.Length > 1) {
                    MessageBox.Show("You can only drop one file at a time.");
                    return;
                }
                DisplayFile(droppedFilePaths[0]);
            }
        }
        private void ChooseFileClicked(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
                DisplayFile(dlg.FileName);
        }
        private void DisplayFile(string filePath) {
            this.selectedFileName = filePath;
            topGrid.Background = new SolidColorBrush(Colors.Fuchsia);
            convertLabel.Content = "to select a different video, drag it here or";
            convertGrid.VerticalAlignment = VerticalAlignment.Bottom;
            convertGrid.Margin = new Thickness(0d, 0d, 0d, 5d);
            string fileName = IOPath.GetFileName(filePath);
            fileNameLabel.Content = fileName;
            fileChosenPanel.Visibility = Visibility.Visible;
        }

        private void ConvertClicked(object sender, RoutedEventArgs e) {
            if (this.selectedFileName == null) {
                MessageBox.Show("You must specify a file first.");
                return;
            }
            if (videoFormatCombo.SelectedValue == null) {
                MessageBox.Show("You must select a format first.");
                return;
            }
            VideoFormat selectedFormat = VideoFormat.ForId(
                (int)videoFormatCombo.SelectedValue);
            if (FileSelected != null)
                FileSelected(this, new VideoSelectedEventArgs(
                    selectedFileName, selectedFormat));
        }
    }
}