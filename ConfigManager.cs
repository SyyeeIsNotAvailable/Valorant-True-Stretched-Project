using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrueStretchedValorant
{
    public sealed class ConfigManager
    {
        private static readonly string BackupFolder = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Backup_INI");

        private static readonly HashSet<string> WidthKeys = new(StringComparer.Ordinal)
        {
            "ResolutionSizeX", "LastUserConfirmedResolutionSizeX",
            "DesiredScreenWidth", "LastUserConfirmedDesiredScreenWidth"
        };

        private static readonly HashSet<string> HeightKeys = new(StringComparer.Ordinal)
        {
            "ResolutionSizeY", "LastUserConfirmedResolutionSizeY",
            "DesiredScreenHeight", "LastUserConfirmedDesiredScreenHeight"
        };

        private static readonly Dictionary<string, string> ForcedValues = new(StringComparer.Ordinal)
        {
            ["bShouldLetterbox"] = "False",
            ["bLastConfirmedShouldLetterbox"] = "False",
            ["bUseVSync"] = "False",
            ["bUseDynamicResolution"] = "False",
            ["LastConfirmedFullscreenMode"] = "2",
            ["PreferredFullscreenMode"] = "2"
        };

        private string? _iniFilePath;
        private bool _isLocked;

        public string? IniFilePath => _iniFilePath;
        public bool HasIniFile => _iniFilePath is not null && File.Exists(_iniFilePath);
        public bool IsLocked => _isLocked;

        public void SetIniPath(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Fichier INI introuvable.", path);
            _iniFilePath = path;
        }

        public (bool Success, string Message) Backup()
        {
            if (_iniFilePath is null)
                return (false, "Aucun fichier INI sélectionné.");

            try
            {
                Directory.CreateDirectory(BackupFolder);
                string fileName = Path.GetFileNameWithoutExtension(_iniFilePath);
                string backupPath = Path.Combine(BackupFolder, $"{fileName}.ini.bak");

                if (File.Exists(backupPath))
                    File.SetAttributes(backupPath, FileAttributes.Normal);

                File.Copy(_iniFilePath, backupPath, overwrite: true);
                return (true, "Backup OK.");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur backup : {ex.Message}");
            }
        }

        public (bool Success, string Message) Patch(int width, int height)
        {
            if (_iniFilePath is null)
                return (false, "Aucun fichier INI sélectionné.");

            try
            {
                Unlock();

                var lines = File.ReadAllLines(_iniFilePath).ToList();
                int modifiedCount = 0;
                bool fullscreenModeExists = false;
                string? preferredSection = null;
                string? currentSection = null;
                int sectionLastLineIndex = -1;

                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].Trim();

                    if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
                    {
                        if (currentSection == preferredSection && preferredSection is not null)
                        {
                            sectionLastLineIndex = i - 1;
                            while (sectionLastLineIndex >= 0 && string.IsNullOrWhiteSpace(lines[sectionLastLineIndex]))
                                sectionLastLineIndex--;
                        }
                        currentSection = trimmed;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(trimmed)) continue;

                    int eqIndex = trimmed.IndexOf('=');
                    if (eqIndex < 0) continue;
                    string key = trimmed[..eqIndex];

                    if (key == "FullscreenMode")
                    {
                        fullscreenModeExists = true;
                        lines[i] = SetValue(lines[i], eqIndex, "2");
                        modifiedCount++;
                        continue;
                    }

                    if (key == "PreferredFullscreenMode")
                        preferredSection = currentSection;

                    if (WidthKeys.Contains(key))
                    {
                        lines[i] = SetValue(lines[i], eqIndex, width.ToString());
                        modifiedCount++;
                    }
                    else if (HeightKeys.Contains(key))
                    {
                        lines[i] = SetValue(lines[i], eqIndex, height.ToString());
                        modifiedCount++;
                    }
                    else if (ForcedValues.TryGetValue(key, out string? forced))
                    {
                        lines[i] = SetValue(lines[i], eqIndex, forced);
                        modifiedCount++;
                    }
                }

                if (preferredSection is not null && currentSection == preferredSection && sectionLastLineIndex < 0)
                {
                    sectionLastLineIndex = lines.Count - 1;
                    while (sectionLastLineIndex >= 0 && string.IsNullOrWhiteSpace(lines[sectionLastLineIndex]))
                        sectionLastLineIndex--;
                }

                // FullscreenMode n'existe pas par défaut dans le .ini, on l'injecte à la fin de la section
                if (!fullscreenModeExists && sectionLastLineIndex >= 0)
                {
                    lines.Insert(sectionLastLineIndex + 1, "FullscreenMode=2");
                    modifiedCount++;
                }

                File.WriteAllLines(_iniFilePath, lines);
                return (true, $"INI patché → {width}x{height} ({modifiedCount} valeurs)");
            }
            catch (Exception ex)
            {
                return (false, $"Erreur patch : {ex.Message}");
            }
        }

        public void Lock()
        {
            if (_iniFilePath is null || !File.Exists(_iniFilePath)) return;
            File.SetAttributes(_iniFilePath, File.GetAttributes(_iniFilePath) | FileAttributes.ReadOnly);
            _isLocked = true;
        }

        public void Unlock()
        {
            if (_iniFilePath is null || !File.Exists(_iniFilePath)) return;
            var attrs = File.GetAttributes(_iniFilePath);
            if (attrs.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(_iniFilePath, attrs & ~FileAttributes.ReadOnly);
            _isLocked = false;
        }

        private static string SetValue(string line, int eqIndex, string newValue)
            => line[..(eqIndex + 1)] + newValue;
    }
}