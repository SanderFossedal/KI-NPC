using LLMUnity;
using System.Text;
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
            // Fjerner alle # tegn
            //string cleanedText = text.Replace("#", "");

            // Fjerner unødvendige mellomrom (leading, trailing, og doble mellomrom i teksten)
            //cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"\s+", " ").Trim();

            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (!char.IsPunctuation(c) && c != '#')
                    sb.Append(c);
            }
            sb.ToString();


            // Append new message to the chat output.
            chatOutput.text = $"{sb}";

            Debug.Log($"{sender}:default text: {text}");

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
