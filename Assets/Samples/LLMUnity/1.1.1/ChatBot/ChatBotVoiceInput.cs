using LeastSquares.Overtone;
using LLMUnity;
using System;
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
            //if (!warmUpDone)
            //{
            //    Debug.Log("Warmup not completed or empty input.");
            //    return;
            //}

            //Debug.Log($"Processing voice input: {voiceText}");
            //voiceTextTemp.text = voiceText;
            //SubmitTextToLLM(voiceText);

            if (!warmUpDone)
            {
                Debug.Log("Warmup not completed");
                debugLog.text = "Cannot process input: LLM not ready.";
                return;
            }

            if (string.IsNullOrEmpty(voiceText) || voiceText == "Teksting av Nicolai Winther")
            {
                Debug.Log("Empty input.");
                debugLog.text = "Cannot process empty input.";
                return;
            }

            Debug.Log($"Processing voice input: {voiceText}");
            voiceTextTemp.text = voiceText;
            SubmitTextToLLM(voiceText);
        }

        private void SubmitTextToLLM(string text)
        {
            string message = text;
            try
            {
                Task chatTask = llm.Chat(message, response => UpdateChatOutput(response, "AI"));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message to LLM: {e.Message}");
                debugLog.text = "Error sending message to LLM.";
            }


            //string message = text;
            //Task chatTask = llm.Chat(message, response => UpdateChatOutput(response, "AI"));
        }

        public void UpdateChatOutput(string text, string sender)
        {
            //var sb = new StringBuilder();
            //foreach (char c in text)
            //{
            //    if (c != '#')
            //        sb.Append(c);
            //}
            //sb.ToString();


            //// Append new message to the chat output.
            //chatOutput.text = $"{sb}";
            //string finalText = sb.ToString();
            //if (tts != null)
            //    tts.ReceiveText(finalText); // Send the text to the TTS component

            //Debug.Log($"{sender}:default text: {text}");
            //debugLog.text = "";

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c != '#')
                    sb.Append(c);
            }
            string finalText = sb.ToString();

            chatOutput.text = finalText;
            if (tts != null)
            {
                try
                {
                    tts.ReceiveText(finalText);
                }
                catch (Exception e)
                {
                    Debug.LogError($"TTS Error: {e.Message}");
                    debugLog.text = "TTS processing error.";
                }
            }

            Debug.Log($"{sender} text: {text}");
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            Debug.Log("LLM client is ready for input.");
            debugLog.text = "LLM client is ready for input.";
        }
    }
}

