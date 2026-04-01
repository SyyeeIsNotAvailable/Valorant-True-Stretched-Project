using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TrueStretchedValorant
{
    public sealed class AppOrchestrator : INotifyPropertyChanged
    {
        private readonly ConfigManager _config = new();
        private readonly ResolutionManager _resolution = new();

        private bool _isStretched;
        private string _statusText = "Inactif";
        private string _iniStatus = "Aucun fichier sélectionné";
        private string _qresStatus = "Aucun fichier sélectionné";
        private string _resolutionStatus = "—";

        public bool IsStretched
        {
            get => _isStretched;
            private set => SetField(ref _isStretched, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetField(ref _statusText, value);
        }

        public string IniStatus
        {
            get => _iniStatus;
            private set => SetField(ref _iniStatus, value);
        }

        public string QResStatus
        {
            get => _qresStatus;
            private set => SetField(ref _qresStatus, value);
        }

        public string ResolutionStatus
        {
            get => _resolutionStatus;
            private set => SetField(ref _resolutionStatus, value);
        }

        public int NativeWidth => _resolution.NativeWidth;
        public int NativeHeight => _resolution.NativeHeight;

        public void Initialize()
        {
            _resolution.Initialize();
            ResolutionStatus = $"{_resolution.NativeWidth}x{_resolution.NativeHeight} (natif)";
        }

        public (bool Success, string Message) SetIniFile(string path)
        {
            try
            {
                _config.SetIniPath(path);
                IniStatus = $"✓ {System.IO.Path.GetFileName(path)}";
                return (true, "OK");
            }
            catch (Exception ex)
            {
                IniStatus = "Fichier invalide";
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) SetQResFile(string path)
        {
            try
            {
                _resolution.SetQResPath(path);
                QResStatus = $"✓ {System.IO.Path.GetFileName(path)}";
                return (true, "OK");
            }
            catch (Exception ex)
            {
                QResStatus = "Fichier invalide";
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) StartStretched(int stretchedW, int stretchedH)
        {
            if (!_config.HasIniFile)
                return (false, "Sélectionnez d'abord le fichier GameUserSettings.ini.");
            if (!_resolution.HasQRes)
                return (false, "Sélectionnez d'abord QRes.exe.");

            var backup = _config.Backup();
            if (!backup.Success) { StatusText = backup.Message; return backup; }

            var patch = _config.Patch(stretchedW, stretchedH);
            if (!patch.Success) { StatusText = patch.Message; return patch; }

            _config.Lock();
            IniStatus = $"Patché & verrouillé → {stretchedW}x{stretchedH}";

            var res = _resolution.SetResolution(stretchedW, stretchedH);
            if (!res.Success) { StatusText = res.Message; return res; }

            IsStretched = true;
            ResolutionStatus = $"{stretchedW}x{stretchedH} (stretched)";
            StatusText = "✅ Stretched actif — lancez Valorant !";
            return (true, "OK");
        }

        public void StopStretched()
        {
            if (_resolution.IsStretched) _resolution.RestoreNative();
            _config.Unlock();

            IsStretched = false;
            ResolutionStatus = $"{_resolution.NativeWidth}x{_resolution.NativeHeight} (natif)";
            IniStatus = _config.HasIniFile ? "✓ Déverrouillé" : "Aucun fichier sélectionné";
            StatusText = "Inactif — résolution native restaurée";
        }

        public void Shutdown()
        {
            if (_isStretched) StopStretched();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;

            if (Application.Current?.Dispatcher.CheckAccess() == true)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            else
                Application.Current?.Dispatcher.Invoke(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
