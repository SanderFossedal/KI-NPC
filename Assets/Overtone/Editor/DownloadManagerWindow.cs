using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Overtone
{
    public class DownloadManager : EditorWindow
    {
        public const string ContextMenuLocation = "Window/Overtone/Download Manager";
        private VoiceModel[] installedModels = new VoiceModel[0];
        private string modelsPath;
        private WebClient webClient;
        private int _currentPercentage;
        private bool isDownloading;
        private Vector2 _scrollPosition;

        [MenuItem(ContextMenuLocation)]
        public static void ShowWindow()
        {
            GetWindow<DownloadManager>(true, "Voice Models Management");
        }

        /**
         * This method is called when the editor is enabled. It initializes the serialized properties and sets the path to the models directory.
         */
        private void OnEnable()
        {
            modelsPath = Path.Combine(Application.dataPath, "Overtone/Resources/");
            VoiceModelInformation.UpdateModelIndexes(() =>
            {
                UpdateInstalledModels();
                Repaint();
                //OnGUI();
            });
        }
        
        /**
         * This method updates the list of installed models by searching for all .bytes files in the models directory and adding them to the installedModels list.
         */
        private void UpdateInstalledModels()
        {
            installedModels = VoiceModel.UpdateInstalledModels();
        }
        
        /**
         * This method displays the GUI for selecting the model and language to use for speech recognition and translation.
         */
        private void ModelSettingsGUI()
        {
            if (installedModels.Length == 0)
            {
                EditorGUILayout.HelpBox("Please install a model first.", MessageType.Info);
                return;
            }
        }

        /**
         * This method is called when the inspector GUI is drawn. It displays the GUI for selecting the model and language and also provides options for downloading and deleting models.
         */
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            if (VoiceModelInformation.Models == null)
            {
                EditorGUILayout.HelpBox("Loading models, please wait...", MessageType.Info);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                ModelSettingsGUI();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Voice Models Management", EditorStyles.boldLabel);
                EditorGUILayout.Space();

                if (isDownloading)
                {
                    EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true)),
                        _currentPercentage * 0.01f, "Downloading...");
                }
                else
                {
                    DrawModels();
                }

                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        /**
         * This method displays the GUI for downloading or deleting an English model.
         */
        private void DrawModels()
        {
            var allModels = VoiceModelInformation.Models.ToList().OrderBy(m => m.LanguageIsoCode).ToArray();
            var installed =
                allModels.Where(M =>
                    installedModels.FirstOrDefault(N => M.ModelName == N.ModelName && M.Quality == N.Quality) != null);
            var notInstalled =
                allModels.Where(M =>
                    installedModels.FirstOrDefault(N => M.ModelName == N.ModelName && M.Quality == N.Quality) == null);

            EditorGUILayout.LabelField($"Total models available: {VoiceModelInformation.Models.Length}");
            EditorGUILayout.Space();
            DrawModelsSection("Installed Models", true, installed);
            EditorGUILayout.Space();
            DrawModelsSection("Available Models for Download", false, notInstalled);
        }

        private void DrawModelsSection(string title, bool isInstalledSection, IEnumerable<VoiceModelInformation> models)
        {
            string currentLanguage = "";
            EditorGUILayout.LabelField(title, EditorStyles.linkLabel);
            foreach (var modelInfo in models)
            {
                if (currentLanguage != modelInfo.LanguageIsoCode)
                {
                    currentLanguage = modelInfo.LanguageIsoCode;
                    EditorGUILayout.Space();
                    string languageName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(GetLanguageFromIsoCode(modelInfo.LanguageIsoCode));
                    EditorGUILayout.LabelField($"{languageName} ({modelInfo.LanguageIsoCode.ToUpper()})", EditorStyles.whiteLabel);
                }
                DrawModel(modelInfo, isInstalledSection);
            }
        }

        /**
         * This method displays the GUI for downloading or deleting a model.
         */
        private void DrawModel(VoiceModelInformation modelInfo, bool isInstalledSection)
        {
            var model = installedModels.FirstOrDefault(M => M.ModelName == modelInfo.ModelName && M.Quality == modelInfo.Quality);
            var isInstalled = model != null;
            
            if (!(isInstalledSection && isInstalled || !isInstalledSection && !isInstalled)) return;
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            
            string capitalizedModelName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modelInfo.ModelName.Replace("_", " "));
            
            GUILayout.Label(capitalizedModelName, GUILayout.Width(100));
            GUILayout.Label(modelInfo.Quality.ToUpper().Replace("_", "-"), GUILayout.Width(75));
            EditorGUILayout.Separator();
            if (isInstalledSection)
            {
                //EditorGUILayout.LabelField("Installed");

                GUI.backgroundColor = new Color32(255, 132, 132, 255);
                if (GUILayout.Button("Delete", GUILayout.Width(100)))
                {
                    model.Delete();
                    AssetDatabase.Refresh();
                    UpdateInstalledModels();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {

                if (GUILayout.Button("Download", GUILayout.Width(100)))
                {
                    DownloadModel(modelInfo.ModelUrl, modelInfo.ModelConfigUrl, modelInfo);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        
        public static string GetLanguageFromIsoCode(string isoCode)
        {
            try
            {
                CultureInfo culture = CultureInfo.GetCultureInfoByIetfLanguageTag(isoCode);
                return culture.NativeName;
            }
            catch (CultureNotFoundException)
            {
                // If the language is not supported by the .NET Framework, return an empty string or a custom error message
                return string.Empty;
            }
        }

        /**
         * This method downloads a model from the internet and saves it to the models directory.
         */
        private async void DownloadModel(string modelUrl, string configUrl, VoiceModelInformation modelInfo)
        {
            isDownloading = true;
            _currentPercentage = 0;
            webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, e) =>
            {
                _currentPercentage = e.ProgressPercentage;
                Repaint();
            };
            var total = 0;
            webClient.DownloadFileCompleted += (sender, e) =>
            {
                total++;
                if (total != 2) return;
                
                if (e.Cancelled || e.Error != null)
                {
                    Debug.LogError("Error downloading the model: " + e.Error.Message);
                }
                else
                {
                    AssetDatabase.Refresh();
                    UpdateInstalledModels();
                    EditorUtility.DisplayDialog("Download Complete", $"Successfully downloaded {modelInfo.ModelName}.", "OK");
                }

                isDownloading = false;
            };
            await webClient.DownloadFileTaskAsync(new Uri(modelUrl), Path.Combine(modelsPath,  $"{modelInfo.LanguageIsoCode}-{modelInfo.ModelName}-{modelInfo.Quality}.bytes"));
            webClient.DownloadFileAsync(new Uri(configUrl), Path.Combine(modelsPath, $"{modelInfo.LanguageIsoCode}-{modelInfo.ModelName}-{modelInfo.Quality}.config.json"));
        }
    }
}