// Updated MainWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Yggdrasil_Core.ViewModels;
using Yggdrasil_Core.Utils;
using Yggdrasil_Core.Services;
using Yggdrasil_Core.Managers;
using Yggdrasil_Core.Security;

namespace Yggdrasil_Core
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, UserControl> toolForms = new Dictionary<string, UserControl>();
        private bool isDarkMode = true;
        private ToggleManager toggleManager;

        public MainWindow()
        {
            InitializeComponent();
            Logger.Init(consoleLog);
            toggleManager = new ToggleManager(OnToolToggle);
            toggleManager.Register("Odin's Eye", odinsEyeToggleBtn);
            toggleManager.Register("MacroPad", macroPadToggleBtn); // Add this line
            Loaded += OnLoaded;
            ToolsList.SelectionChanged += OnToolSelect;
            themeBtn.Click += OnThemeToggle;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!await LicenseValidator.Validate())
            {
                Close();
                return;
            }
            licenseNumber.Text = $"License: {LicenseValidator.GetLicenseNumber()}";
            await LoadDefaultTool();
            Logger.Info("App started.");
        }

        private async Task LoadDefaultTool()
        {
            ToolsList.SelectedItem = odinsEyeListBtn;
            await LoadToolForm("Odin's Eye");
        }

        private async void OnToolSelect(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsList.SelectedItem is ListBoxItem item)
            {
                var toolName = (item.Content as DockPanel)?.Children[1] is TextBlock tb ? tb.Text : item.Content.ToString();
                await LoadToolForm(toolName);
            }
        }

        private async Task LoadToolForm(string toolName)
        {
            ToolFormPanel.Children.Clear();
            if (!toolForms.TryGetValue(toolName, out var form))
            {
                form = CreateToolForm(toolName);
                if (form != null) toolForms[toolName] = form;
            }
            if (form != null)
            {
                ToolFormPanel.Children.Add(form);
                Logger.Info($"{toolName} loaded.");
                await Task.Delay(1);
            }
        }

        private UserControl CreateToolForm(string toolName)
        {
            switch (toolName)
            {
                case "Odin's Eye": return new Forms.OdinsEyeForm();
                case "Macro Pad": return new Forms.MacroPadForm(); // Add this case
                default: return null;
            }
        }

        private void OnToolToggle(string toolName, bool isEnabled)
        {
        }

        private async void OnAttachClick(object sender, RoutedEventArgs e)
        {
            if (processList.SelectedItem is ComboBoxItem selectedItem)
            {
                var processName = selectedItem.Content.ToString();
                var processes = Process.GetProcessesByName(processName.Replace("RO Client ", "ragexe"));
                if (processes.Length > 0 && await ClientHandler.Attach(processes[0]))
                {
                    statusLabel.Text = "Status: Attached";
                }
            }
        }

        private void OnDetachClick(object sender, RoutedEventArgs e)
        {
            ClientHandler.Detach();
            statusLabel.Text = "Status: Detached";
        }

        private void OnThemeToggle(object sender, RoutedEventArgs e)
        {
            isDarkMode = !isDarkMode;
            var dict = new ResourceDictionary { Source = new Uri(isDarkMode ? "pack://application:,,,/Resources/DarkTheme.xaml" : "pack://application:,,,/Resources/LightTheme.xaml") };
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dict);
            Logger.Info($"Theme switched to {(isDarkMode ? "Dark" : "Light")}.");
        }
    }
}