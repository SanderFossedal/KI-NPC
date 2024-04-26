using LLMUnity;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;


namespace LLMUnitySamples
{


    public class ChatBotVoiceInput : MonoBehaviour
    {
        public TextMeshPro chatOutput;
        public LLMClient llm;

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
            SubmitTextToLLM(voiceText);
        }

        private void SubmitTextToLLM(string text)
        {
            string message = text;
            Task chatTask = llm.Chat(message, response => UpdateChatOutput(response, "AI"));
        }

        public void UpdateChatOutput(string text, string sender)
        {
            chatOutput.text = $"{text}";
            Debug.Log($"{sender}: {text}");
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            Debug.Log("LLM client is ready for input.");
        }

        void Update()
        {
            // This method remains available for additional logic if needed in the future.
        }
    }
}

