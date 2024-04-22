using TMPro;
using UnityEngine;

public class ChattingMode : MonoBehaviour
{


    public FPSController playerMovement; // Reference to the PlayerMovement script
    public TMP_InputField chatInputField; // Reference to the chat InputField
    public KeyCode toggleChatKey = KeyCode.F1; // The key to toggle chat mode on/off

    private bool isChatting = false; // Keeps track of whether the player is currently chatting

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleChatKey))
        {
            ToggleChattingMode();
        }

        // Optionally, automatically focus on the input field when chatting mode is enabled
        if (isChatting && !chatInputField.isFocused)
        {
            chatInputField.ActivateInputField();
        }
    }

    void ToggleChattingMode()
    {
        isChatting = !isChatting;
        chatInputField.gameObject.SetActive(isChatting); // Show or hide the chat input field

        if (playerMovement != null)
        {
            playerMovement.canMove = !isChatting; ; // Disable or enable player movement
        }

        if (isChatting)
        {
            // Optionally, clear the text field when entering chatting mode
            chatInputField.text = "";
            // Focus the input field for text input
            chatInputField.ActivateInputField();
            chatInputField.Select();

            chatInputField.onEndEdit.AddListener(OnChatInputEndEdit);

        }

    }



    void OnChatInputEndEdit(string text)
    {

        chatInputField.text = ""; // Clear the input field after handling the input
    }
}
