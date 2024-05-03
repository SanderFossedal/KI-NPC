using LeastSquares.Overtone;
using UnityEngine;

namespace Assets.Overtone.Scripts
{
    public class TTSVoice : MonoBehaviour
    {
        public string voiceName;
        public int speakerId;
        private string oldVoiceName;
        private int oldSpeakerId;
        public TTSVoiceNative VoiceModel { get; private set; }
        void Update()
        {
            if (voiceName != oldVoiceName)
            {
                oldVoiceName = voiceName;
                VoiceModel?.Dispose();
                VoiceModel = TTSVoiceNative.LoadVoiceFromResources(voiceName);
            }

            if (speakerId != oldSpeakerId)
            {
                oldSpeakerId = speakerId;
                VoiceModel.SetSpeakerId(speakerId);
            }
        }

        void OnDestroy()
        {
            VoiceModel?.Dispose();
        }
    }
}