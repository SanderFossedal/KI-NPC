using System;
using System.IO;
using System.Linq;
using Assets.Overtone.Scripts;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace LeastSquares.Overtone
{
    [CustomEditor(typeof(TTSVoice))]
    public class TTSVoiceEditor : Editor
    {
        private SerializedProperty voiceNameProp;
        private SerializedProperty speakerIdProp;
        private VoiceModel[] installedModels;
        private string[] modelOptions;
        private int speakerCount;

        public VoiceModel CurrentModel => installedModels.Length > 0 ? installedModels[Array.IndexOf(modelOptions, voiceNameProp.stringValue)] : null;
        
        private void OnEnable()
        {
            voiceNameProp = serializedObject.FindProperty("voiceName");
            speakerIdProp = serializedObject.FindProperty("speakerId");
            
            UpdateInstalledModelNames();
            UpdateSpeakerCount();
        }

        public override void OnInspectorGUI()
        {   
            serializedObject.Update();
            if (modelOptions.Length > 0)
            {
                var selectedIndex = Array.IndexOf(modelOptions, voiceNameProp.stringValue);
                selectedIndex = selectedIndex < 0 ? 0 : selectedIndex;
                var newSelectedIndex = EditorGUILayout.Popup("Voice Name", selectedIndex, modelOptions);
                newSelectedIndex = WrapModelIndex(newSelectedIndex);
                if (selectedIndex != newSelectedIndex)
                {
                    selectedIndex = newSelectedIndex;
                    voiceNameProp.stringValue = modelOptions[selectedIndex];
                    UpdateSpeakerCount();
                }
                voiceNameProp.stringValue = modelOptions[selectedIndex];

                if (speakerCount > 1)
                {
                    int speakerIndex = WrapSpeakerIndex(speakerIdProp.intValue);
                    int newSpeakerIndex = speakerIndex;
                    if (speakerCount > 1)
                        newSpeakerIndex = EditorGUILayout.IntSlider("Speaker ID", speakerIndex, 0, speakerCount-1);
                    speakerIdProp.intValue = WrapSpeakerIndex(newSpeakerIndex);
                }
                else
                {
                    speakerIdProp.intValue = 0;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No voice models found. Please install a voice model from the TTSEngine component.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Model Download Manager"))
            {
                EditorApplication.ExecuteMenuItem(DownloadManager.ContextMenuLocation);
            }
        }

        private int WrapSpeakerIndex(int index)
        {
            return Mathf.Clamp(index, 0, speakerCount-1);
        }
        
        private int WrapModelIndex(int index)
        {
            return Mathf.Clamp(index, 0, installedModels.Length-1);
        }

        
        private void UpdateInstalledModelNames()
        {
            installedModels = VoiceModel.UpdateInstalledModels();
            modelOptions = installedModels.Select(S => Path.GetFileName(S.BytesPath).Replace(".bytes", "")).ToArray();
        }

        private void UpdateSpeakerCount()
        {
            if (CurrentModel == null) return;
            speakerCount = LoadSpeakerCount(CurrentModel.ConfigPath);
        }

        private int LoadSpeakerCount(string configPath)
        {
            if (!File.Exists(configPath)) return 1;
            var json = File.ReadAllText(configPath);
            var config = JObject.Parse(json);
            var speakerCountObj = config["num_speakers"];
            return speakerCountObj.ToObject<int>();
        }
    }
}
