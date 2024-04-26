using LLMUnity;
using UnityEngine;

public class NPCRoomDetector : MonoBehaviour
{
    private LLM llmManager;

    private void Start()
    {
        // Finn LLM manager i scenen
        llmManager = FindObjectOfType<LLM>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            RoomDescription room = other.GetComponent<RoomDescription>();
            if (room != null)
            {
                // Oppdater LLM prompt med rombeskrivelsen
                UpdateLLMPrompt(room.description);
            }
        }
    }

    private void UpdateLLMPrompt(string description)
    {
        // Anta at det er en metode i LLM for å oppdatere prompt
        //llmManager.UpdatePrompt(description);
    }
}
