using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Wox.Plugin.Recent
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : UserControl
    {


        public SettingsPanel(Settings settings)
        {
            InitializeComponent();
            _settings = settings;

            this.dirManagerPath.Text = _settings.DirectoryManager;
            this.fileProcessorPath.Text = _settings.FileProcessor;
        }

        private void dirManagerSetButtonAction(object sender, RoutedEventArgs e)
        {
            var fileBrowserDialog = new OpenFileDialog();
            fileBrowserDialog.Filter = "Application(*.exe)|*.exe|All files|*.*";
            fileBrowserDialog.CheckFileExists = true;
            fileBrowserDialog.CheckPathExists = true;
            if (fileBrowserDialog.ShowDialog() == true)
            {
                dirManagerPath.Text = fileBrowserDialog.FileName;
                _settings.DirectoryManager = fileBrowserDialog.FileName;
            }
        }

        private void fileProcessorSetButtonAction(object sender, RoutedEventArgs e)
        {
            var fileBrowserDialog = new OpenFileDialog();
            fileBrowserDialog.Filter = "Application(*.exe)|*.exe|All files|*.*";
            fileBrowserDialog.CheckFileExists = true;
            fileBrowserDialog.CheckPathExists = true;
            if (fileBrowserDialog.ShowDialog() == true)
            {
                fileProcessorPath.Text = fileBrowserDialog.FileName;
                _settings.FileProcessor = fileBrowserDialog.FileName;
            }
        }

        private Settings _settings;

    }
}
