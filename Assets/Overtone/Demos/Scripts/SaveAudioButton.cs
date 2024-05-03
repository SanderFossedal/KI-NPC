using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;

namespace LeastSquares.Overtone
{

    public class SaveAudioButton : MonoBehaviour
    {
        public AudioClip audioClip;
        public Button convertButton;

        void Start()
        {
            convertButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (audioClip == null)
            {
                Debug.LogWarning("No audio clip to save!");
                return;
            }

            if (!Application.isEditor)
            {
                Debug.LogWarning("Can only save in editor mode!");
                return;
            }
            
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var validName = Regex.Replace(audioClip.name, "[" + Regex.Escape(invalidChars) + "]", "_");
            var filename = validName.Length > 64 ?validName.Substring(0, 64) : validName;
            var path = $"Assets/Overtone/Demos/Output/{filename}.wav";
            ConvertAndSave(audioClip, path);
        }

        private static void ConvertAndSave(AudioClip clip, string filePath)
        {
            var wavFile = ConvertAudioClipToWav(clip);
            if (!Directory.Exists("Assets/Overtone/Demos/Output/"))
            {
                Directory.CreateDirectory("Assets/Overtone/Demos/Output/");
            }
            File.WriteAllBytes(filePath, wavFile);
            Debug.Log("Saved audio to " + filePath + " successfully!");
        }

        private static byte[] ConvertAudioClipToWav(AudioClip clip)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // WAV file header
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + clip.samples * 2);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((short)(clip.channels * 2));
            writer.Write((short)16);
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(clip.samples * clip.channels * 2);

            // Write the audio data
            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            for (var i = 0; i < samples.Length; i++)
            {
                var intData = (short)(samples[i] * 32768);
                writer.Write(intData);
            }

            writer.Flush();
            byte[] byteArray = stream.ToArray();

            writer.Close();
            stream.Close();

            return byteArray;
        }
    }

}