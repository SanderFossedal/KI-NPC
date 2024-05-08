using LeastSquares.Overtone;
using LLMUnity;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


namespace LLMUnitySamples
{


    public class ChatBotVoiceInput : MonoBehaviour
    {
        public TextMeshPro chatOutput;
        public TextMeshProUGUI voiceTextTemp;
        public TextMeshProUGUI debugLog;
        public LLMClient llm;
        public StartTTS tts;

        private bool warmUpDone = false;

        void Start()
        {
            _ = llm.Warmup(WarmUpCallback);
        }

        // Call this method with the output from your voice-to-text system
        public void ProcessVoiceInput(string voiceText)
        {
            if (!warmUpDone)
            {
                Debug.Log("Warmup not completed or empty input.");
                return;
            }

            Debug.Log($"Processing voice input: {voiceText}");
            voiceTextTemp.text = voiceText;
            SubmitTextToLLM(voiceText);
        }

        private void SubmitTextToLLM(string text)
        {
            string message = text;
            Task chatTask = llm.Chat(message, response => UpdateChatOutput(response, "AI"));
        }

        public void UpdateChatOutput(string text, string sender)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c != '#')
                    sb.Append(c);
            }
            sb.ToString();


            // Append new message to the chat output.
            chatOutput.text = $"{sb}";
            string finalText = sb.ToString();
            if (tts != null)
                tts.ReceiveText(finalText); // Send the text to the TTS component

            Debug.Log($"{sender}:default text: {text}");
            debugLog.text = "";
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            Debug.Log("LLM client is ready for input.");
            debugLog.text = "LLM client is ready for input.";
        }

        void Update()
        {

        }
    }
}

