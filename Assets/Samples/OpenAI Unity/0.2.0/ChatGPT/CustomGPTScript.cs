using OpenAI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomGPTScript : MonoBehaviour
{

    public OnResponseEvent onResponseEvent;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openai = new OpenAIApi();

    private List<ChatMessage> messages = new List<ChatMessage>();

    private string prompt;

    [Header("GPT Configuration")]
    [Tooltip("Kontrollerer graden av tilfeldighet i svarene. Høyere verdi gir mer varierte svar.")]
    public float temperature = 0.7f;

    [Tooltip("Maksimalt antall tokens (ord/tegn) som skal genereres.")]
    public int maxTokens = 150;

    [Tooltip("Kontrollerer nukleær sampling. Verdier nær 1 velger tokens proporsjonalt til deres sannsynlighet.")]
    public float topP = 1;

    [Tooltip("Reduserer sannsynligheten for gjentagelse av tidligere tokens.")]
    public float frequencyPenalty = 0;

    [Tooltip("Reduserer sannsynligheten for gjentagelse av allerede nevnte emner.")]
    public float presencePenalty = 0;

    // Start is called before the first frame update
    void Start()
    {

        // Load the prompt from a text file in Resources
        TextAsset promptTextAsset = Resources.Load<TextAsset>("System_promt");

        if (promptTextAsset != null)
        {
            prompt = promptTextAsset.text;
        }
        else
        {
            Debug.LogError("Prompt file not found. Make sure there's a 'prompt.txt' file in the Resources folder.");
            prompt = "Skriv at det var en feil med å hente promt, uansett hva som er sagt";

        }
    }

    public async void SendGPTReply(string newText)
    {
        // Trim the input to remove leading and trailing whitespaces
        string trimmedInput = newText.Trim();

        // Check if the trimmed input is empty and return if it is
        if (string.IsNullOrEmpty(trimmedInput))
        {
            Debug.LogWarning("Attempted to send an empty message. Operation aborted.");
            return;
        }

        Debug.Log("Sending request to OpenAI API...");

        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        //messages.Add(newMessage);

        if (messages.Count == 0) newMessage.Content = prompt + "\n" + newText;

        messages.Add(newMessage);


        // Complete the instruction
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613",
            Messages = messages,
            Temperature = temperature,
            MaxTokens = maxTokens,
            FrequencyPenalty = frequencyPenalty,
            PresencePenalty = presencePenalty

        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();

            messages.Add(message);

            onResponseEvent.Invoke(message.Content);

        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }


    }

    //Testing relatert

    public bool HasReceivedResponse { get; private set; }

    private void OnResponseReceived(string response)
    {
        HasReceivedResponse = true;
        // Trigger andre nødvendige handlinger
    }

}
