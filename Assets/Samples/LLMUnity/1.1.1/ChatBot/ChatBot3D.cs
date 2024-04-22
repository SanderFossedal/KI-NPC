using LLMUnity;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LLMUnitySamples
{
    public class ChatBot3D : MonoBehaviour
    {


        //public Transform chatContainer;
        //public TMP_InputField userInputField;
        public TextMeshPro chatOutput;
        public LLMClient llm;

        private bool blockInput = true;
        private bool warmUpDone = false;

        void Start()
        {
            //userInputField.onEndEdit.AddListener(onInputFieldSubmit); // Use Unity's event system to handle input submission.
            _ = llm.Warmup(WarmUpCallback);
        }

        public void onInputFieldSubmit(string input)
        {

            //Debug.Log("User: " + input);
            //if (blockInput || string.IsNullOrWhiteSpace(input))
            //{
            //    return;
            //}

            blockInput = true;
            string message = input.Replace("\v", "\n"); // Sanitize input text.
            Debug.Log($"User message: {message}");

            //UpdateChatOutput(message, "User"); // Display user's message.
            Task chatTask = llm.Chat(message, response => UpdateChatOutput(response, "AI"), AllowInput);
            //userInputField.text = ""; // Clear input field.
        }

        public void UpdateChatOutput(string text, string sender)
        {
            // Append new message to the chat output.
            chatOutput.text = $"{text}";

            Debug.Log($"{sender}: {text}");

            AllowInput();
        }

        public void WarmUpCallback()
        {
            warmUpDone = true;
            //userInputField.placeholder.GetComponent<Text>().text = "Message me"; // Update placeholder text of input field.
            AllowInput();
        }

        public void AllowInput()
        {
            blockInput = false;
            //userInputField.interactable = true; // Re-enable the input field.
        }

        void Update()
        {
            // Additional logic if needed, for instance, to focus the input field or to handle UI updates.
        }
    }
}
