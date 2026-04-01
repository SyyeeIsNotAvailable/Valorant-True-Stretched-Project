using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace TrueStretchedValorant
{
    public sealed class ResolutionManager
    {
        private string? _qresPath;
        private bool _isStretched;

        public int NativeWidth { get; private set; }
        public int NativeHeight { get; private set; }
        public bool IsStretched => _isStretched;
        public bool HasQRes => _qresPath is not null && File.Exists(_qresPath);

        public void Initialize()
        {
            var source = PresentationSource.FromVisual(Application.Current.MainWindow);
            double dpiX = source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
            double dpiY = source?.CompositionTarget?.TransformToDevice.M22 ?? 1.0;
            NativeWidth = (int)(SystemParameters.PrimaryScreenWidth * dpiX);
            NativeHeight = (int)(SystemParameters.PrimaryScreenHeight * dpiY);
        }

        public void SetQResPath(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("QRes.exe introuvable.", path);
            _qresPath = path;
        }

        public (bool Success, string Message) SetResolution(int width, int height)
        {
            if (_qresPath is null || !File.Exists(_qresPath))
                return (false, "QRes.exe non configuré.");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _qresPath,
                    Arguments = $"/x:{width} /y:{height}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);

                _isStretched = (width != NativeWidth || height != NativeHeight);
                return (true, $"Résolution → {width}x{height}");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur QRes : {ex.Message}");
            }
        }

        public (bool Success, string Message) RestoreNative()
        {
            var result = SetResolution(NativeWidth, NativeHeight);
            _isStretched = false;
            return result;
        }
    }
}
