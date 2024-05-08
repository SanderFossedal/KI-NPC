using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace LeastSquares.Overtone
{
    public class StartTTS : MonoBehaviour
    {
        public TTSPlayer _player;
        //public SaveAudioButton saveAudio;
        //public RotateBackground _rotate;
        private bool _isSpeaking;
        //public TMP_InputField inputText;

        private void Start()
        {
            //var button = GetComponent<Button>();
            //button.onClick.AddListener(OnClicked);
        }

        public void ReceiveText(string text)
        {
            StartCoroutine(CheckInputText(text));
            //Debug.Log($"StartTTS -> Received text: {text}");
        }

        private IEnumerator CheckInputText(string text)
        {
            if (!_player.Engine.Loaded || string.IsNullOrEmpty(text)) yield break;

            //inputText.text = text; // Optionally set this to visually show the text in a UI element
            StartTTSProcess(text); // Trigger the existing speech functionality
            yield return null;
        }

        private async void StartTTSProcess(string inputText)
        {
            //var buttonText = GetComponentInChildren<TMP_Text>();
            // If the speech recognition engine is not loaded, do nothing.
            if (!_player.Engine.Loaded) return;

            //buttonText.text = "Speaking...".ToUpperInvariant();
            //GetComponent<Button>().interactable = false;
            //_rotate.speed = 10;
            await _player.Speak(inputText ?? string.Empty);
            var audioClip = _player.source.clip;
            //saveAudio.audioClip = audioClip;
            await Task.Delay((int)(1000 * audioClip.length));
            //buttonText.text = "Speak".ToUpperInvariant();
            //GetComponent<Button>().interactable = true;
            //_rotate.speed = 0;
            _isSpeaking = false;
        }
    }
}