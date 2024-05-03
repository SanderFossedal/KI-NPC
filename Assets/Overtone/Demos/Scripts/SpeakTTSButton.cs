using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LeastSquares.Overtone
{
    public class SpeakTTSButton : MonoBehaviour
    {
        public TTSPlayer _player;
        public SaveAudioButton saveAudio;
        public RotateBackground _rotate;
        private bool _isSpeaking;
        public TMP_InputField inputText;
        
        private void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClicked);
        }
        
        private async void OnClicked()
        {
            var buttonText = GetComponentInChildren<TMP_Text>();
            // If the speech recognition engine is not loaded, do nothing.
            if (!_player.Engine.Loaded) return;

            buttonText.text = "Speaking...".ToUpperInvariant();
            GetComponent<Button>().interactable = false;
            _rotate.speed = 10;
            await _player.Speak(inputText.text ?? string.Empty);
            var audioClip = _player.source.clip;
            saveAudio.audioClip = audioClip;
            await Task.Delay((int)(1000 * audioClip.length));
            buttonText.text = "Speak".ToUpperInvariant();
            GetComponent<Button>().interactable = true;
            _rotate.speed = 0;
            _isSpeaking = false;
        }
    }
}