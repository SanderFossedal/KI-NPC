using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LeastSquares.Overtone
{
    public class VoiceModelInformation
    {
        public static readonly string[] ModelIndexes = new string[]
        {
            "https://huggingface.co/rhasspy/piper-voices/resolve/v1.0.0/"
        };
        
        public string LanguageIsoCode { get; }
        public string ModelName { get; }
        public string Quality { get; }
        public string ModelUrl { get; }
        public string ModelConfigUrl { get; }

        public VoiceModelInformation(string languageIsoCode, string modelName, string quality, string modelUrl, string modelConfigUrl)
        {
            LanguageIsoCode = languageIsoCode;
            ModelName = modelName;
            Quality = quality;
            ModelUrl = modelUrl;
            ModelConfigUrl = modelConfigUrl;
        }

        public string FullModelName => $"{LanguageIsoCode}-{ModelName}-{Quality}";

        public static VoiceModelInformation[] Models = null;
        
        public static void UpdateModelIndexes(Action callback)
        {
            var voiceModelInformation = new List<VoiceModelInformation>();
            foreach(var index in ModelIndexes)
            {
                var file = $"{index}voices.json";
                using var webClient = new System.Net.WebClient();
                webClient.DownloadStringAsync(new Uri(file));
                webClient.DownloadStringCompleted += (_, e) =>
                {
                    if (e.Error != null)
                    {
                        Debug.LogError($"Error updating model indexes {file}: {e.Error}");
                        return;
                    }

                    var jObject = JObject.Parse(e.Result);
                    foreach (var voice in jObject)
                    {
                        string isoCode = voice.Value["language"]["code"].ToString();
                        string name = voice.Value["name"].ToString();
                        string quality = voice.Value["quality"].ToString();
                        var code = isoCode.ToLowerInvariant().Replace("_", "-");
                        var files = voice.Value["files"].ToObject<JObject>();
                        var onnxFile = files.Properties()
                            .FirstOrDefault(p => p.Name.EndsWith(".onnx"))?
                            .Name;

                        var onnxJsonFile = files.Properties()
                            .FirstOrDefault(p => p.Name.EndsWith(".onnx.json"))?
                            .Name;

                        var modelUrl = $"{index}{onnxFile}";
                        var modelConfigUrl = $"{index}{onnxJsonFile}";
                        voiceModelInformation.Add(new VoiceModelInformation(code, name, quality, modelUrl, modelConfigUrl));
                    }
                        
                    Models = voiceModelInformation.ToArray();
                    callback?.Invoke();
                };
            }
        }

        private static void PrintVoiceInformationTable(string json)
        {
            var jObject = JObject.Parse(json);
            var languageMap = new Dictionary<string, List<JToken>>();
            
            foreach (var item in jObject)
            {
                var isoCode = item.Value["language"]["code"].ToString();
                var isoCodeNormalized = isoCode.ToLowerInvariant().Replace("_", "-");
                string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DownloadManager.GetLanguageFromIsoCode(isoCodeNormalized));
                string languageName = $"{name} ({isoCodeNormalized.ToLowerInvariant()})";

                if (!languageMap.ContainsKey(languageName))
                {
                    languageMap[languageName] = new List<JToken>();
                }

                languageMap[languageName].Add(item.Value);
            }

            var qualitiesOrder = new List<string> { "HIGH", "MEDIUM", "LOW", "X-LOW" };

            var sb = new StringBuilder();
            sb.AppendLine("| Language             | Best Quality | Number of Voices |");
            sb.AppendLine("|----------------------|--------------|------------------|");

            foreach (var pair in languageMap)
            {
                string Normalize(JToken quality)
                {
                    return quality.ToString().ToUpperInvariant().Replace("_", "-");
                }
                
                string bestQuality = Normalize(pair.Value
                    .OrderBy(v => qualitiesOrder.IndexOf(Normalize(v["quality"])))
                    .First()["quality"]
                    ?.ToString());

                int numOfVoices = pair.Value.Select(S => S["num_speakers"].ToObject<int>()).Sum();

                sb.AppendLine($"| {pair.Key.PadRight(20)} | {bestQuality.PadRight(12)} | {numOfVoices.ToString().PadRight(16)} |");
            }

            // 4. Write the data to a .txt file.
            File.WriteAllText("table.md", sb.ToString());
        }
    }
}