using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;

public class ChatGPTmanager : MonoBehaviour
{
    public OnResponseEvent onResponseEvent;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }



    private OpenAIApi openAIAPI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    private void Start()
    {
        
    }
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();

        request.Messages = messages;

        request.Model = "gpt-3.5-turbo";

        var response = await openAIAPI.CreateChatCompletion(request);

        if(response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Debug.Log(chatResponse.Content);

            onResponseEvent.Invoke(chatResponse.Content);
        }
        
    }
}
