using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace TrueStretchedValorant
{
    public partial class MainWindow : Window
    {
        private readonly AppOrchestrator _orchestrator = new();
        private readonly UserSettings _settings;

        private static readonly SolidColorBrush CyanBrush = new(Color.FromRgb(0x00, 0xFF, 0xD1));
        private static readonly SolidColorBrush GreenBrush = new(Color.FromRgb(0x2E, 0xD5, 0x73));
        private static readonly SolidColorBrush DimBrush = new(Color.FromRgb(0x6B, 0x6B, 0x82));

        public MainWindow()
        {
            InitializeComponent();
            _settings = UserSettings.Load();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");
            if (File.Exists(iconPath))
                Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));

            _orchestrator.Initialize();
            ((App)Application.Current).RegisterOrchestrator(_orchestrator);

            TxtNativeW.Text = _orchestrator.NativeWidth.ToString();
            TxtNativeH.Text = _orchestrator.NativeHeight.ToString();
            _orchestrator.PropertyChanged += Orchestrator_PropertyChanged;

            RestoreSavedSettings();
        }

        private void RestoreSavedSettings()
        {
            TxtStretchedW.Text = _settings.StretchedWidth.ToString();
            TxtStretchedH.Text = _settings.StretchedHeight.ToString();

            if (_settings.IniFilePath is not null && File.Exists(_settings.IniFilePath))
            {
                var r = _orchestrator.SetIniFile(_settings.IniFilePath);
                if (r.Success) TxtIniPath.Text = _settings.IniFilePath;
            }

            if (_settings.QResPath is not null && File.Exists(_settings.QResPath))
            {
                var r = _orchestrator.SetQResFile(_settings.QResPath);
                if (r.Success) TxtQResPath.Text = _settings.QResPath;
            }
        }

        private void SaveSettings()
        {
            if (int.TryParse(TxtStretchedW.Text, out int w)) _settings.StretchedWidth = w;
            if (int.TryParse(TxtStretchedH.Text, out int h)) _settings.StretchedHeight = h;
            _settings.Save();
        }

        private void BtnBrowseIni_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Sélectionnez GameUserSettings.ini",
                Filter = "Fichiers INI (*.ini)|*.ini|Tous (*.*)|*.*",
                FileName = "GameUserSettings.ini"
            };
            if (dlg.ShowDialog() != true) return;

            var r = _orchestrator.SetIniFile(dlg.FileName);
            TxtIniPath.Text = r.Success ? dlg.FileName : "Fichier invalide";
            if (r.Success) { _settings.IniFilePath = dlg.FileName; SaveSettings(); }
            else MessageBox.Show(r.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnBrowseQRes_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Sélectionnez QRes.exe",
                Filter = "Exécutables (*.exe)|*.exe|Tous (*.*)|*.*",
                FileName = "QRes.exe"
            };
            if (dlg.ShowDialog() != true) return;

            var r = _orchestrator.SetQResFile(dlg.FileName);
            TxtQResPath.Text = r.Success ? dlg.FileName : "Fichier invalide";
            if (r.Success) { _settings.QResPath = dlg.FileName; SaveSettings(); }
            else MessageBox.Show(r.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseResolution(out int w, out int h))
            {
                MessageBox.Show("Résolution invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveSettings();
            var r = _orchestrator.StartStretched(w, h);
            if (!r.Success)
            {
                MessageBox.Show(r.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SetUiLocked(true);
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _orchestrator.StopStretched();
            SetUiLocked(false);
        }

        private void SetUiLocked(bool locked)
        {
            BtnStart.IsEnabled = !locked;
            BtnStop.IsEnabled = locked;
            BtnBrowseIni.IsEnabled = !locked;
            BtnBrowseQRes.IsEnabled = !locked;
            TxtStretchedW.IsReadOnly = locked;
            TxtStretchedH.IsReadOnly = locked;
        }

        private bool TryParseResolution(out int width, out int height)
        {
            width = 0; height = 0;
            return int.TryParse(TxtStretchedW.Text, out width)
                && int.TryParse(TxtStretchedH.Text, out height)
                && width >= 640 && width <= 7680
                && height >= 480 && height <= 4320;
        }

        private void Orchestrator_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppOrchestrator.StatusText):
                    LblStatus.Text = _orchestrator.StatusText; break;
                case nameof(AppOrchestrator.IniStatus):
                    LblIni.Text = _orchestrator.IniStatus;
                    DotIni.Fill = _orchestrator.IniStatus.Contains("✓") ? GreenBrush : DimBrush; break;
                case nameof(AppOrchestrator.QResStatus):
                    LblQRes.Text = _orchestrator.QResStatus;
                    DotQRes.Fill = _orchestrator.QResStatus.Contains("✓") ? GreenBrush : DimBrush; break;
                case nameof(AppOrchestrator.ResolutionStatus):
                    LblResolution.Text = _orchestrator.ResolutionStatus;
                    DotRes.Fill = _orchestrator.ResolutionStatus.Contains("stretched") ? CyanBrush : DimBrush; break;
                case nameof(AppOrchestrator.IsStretched):
                    if (!_orchestrator.IsStretched) { SetUiLocked(false); DotRes.Fill = DimBrush; } break;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveSettings();
            _orchestrator.PropertyChanged -= Orchestrator_PropertyChanged;
            base.OnClosing(e);
        }
    }
}
