﻿using PrimeTracker.Browsers;
using PrimeTracker.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow()
        {
            while (string.IsNullOrEmpty(AppContext.Settings.DbPath))
            {
                var dlg = new SettingsWindow();
                dlg.ShowDialog();
            }
         
            InitializeComponent();

            Reload();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.CloseBrowser();
        }

        private void OpenAmazonUrl(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;

            if (lb != null)
            {
                Video video = lb.SelectedItem as Video;

                vm.OpenVideo(video);
            }
        }

        internal void Reload()
        {
            if (vm != null)
                vm.CloseBrowser();

            vm = new MainWindowViewModel(this);
            DataContext = vm;
        }
    }
}
