using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LeastSquares.Overtone
{
    public class VoiceModel
    {
        public string LanguageIsoCode { get; private set; }
        public string ModelName { get; private set; }
        public string Quality { get; private set; }
        public string ConfigPath { get; private set; }
        public string BytesPath { get; private set; }

        public VoiceModel(string languageIsoCode, string modelName, string quality, string configPath, string bytesPath)
        {
            Quality = quality;
            LanguageIsoCode = languageIsoCode;
            ModelName = modelName;
            ConfigPath = configPath;
            BytesPath = bytesPath;
        }
        public static VoiceModel[] UpdateInstalledModels()
        {
            string resourcePath = Path.Combine(Application.dataPath, "Overtone/Resources/");
            if (!Directory.Exists(resourcePath))
            {
                Debug.LogError($"The folder {resourcePath} does not exist.");
                return new VoiceModel[] { };
            }

            var modelFiles = Directory.GetFiles(resourcePath, "*.bytes", SearchOption.AllDirectories);
            var configFilePattern = @"^(?<iso>[a-z]{2}(?:-[a-z]{2})?)-(?<model>[\w_]+)-(?<quality>[\w_]+)(?:\.config\.json|\.config)$";
            var configFiles = Directory.GetFiles(resourcePath, "*.config.json", SearchOption.AllDirectories)
                .Where(file => Regex.IsMatch(Path.GetFileName(file), configFilePattern));

            var installedModels = new List<VoiceModel>();
            foreach (var configFile in configFiles)
            {
                var match = Regex.Match(Path.GetFileName(configFile), configFilePattern);
                if (match.Success)
                {
                    string languageIsoCode = match.Groups["iso"].Value;
                    string modelName = match.Groups["model"].Value;
                    string quality = match.Groups["quality"].Value;

                    string modelPath = modelFiles.FirstOrDefault(f => Path.GetFileName(f).StartsWith($"{languageIsoCode}-{modelName}-{quality}"));
                    if (modelPath != null)
                    {
                        installedModels.Add(
                            new VoiceModel(
                                languageIsoCode,
                                modelName,
                                quality,
                                configFile,
                                modelPath
                            )
                        );
                    }
                }
            }
            return installedModels.ToArray();
        }
        
        public void Delete()
        {
            File.Delete(this.ConfigPath);
            File.Delete(this.BytesPath);
        }
    }
}