using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GptTesting : MonoBehaviour
{
    public CustomGPTScript customGPTScript;

    private void Start()
    {
        // Start testene, for eksempel:
        StartCoroutine(ResponseTimeTest("Hei, hvordan har du det?"));
    }

    private IEnumerator ResponseTimeTest(string input)
    {
        float startTime = Time.realtimeSinceStartup;
        customGPTScript.onResponseEvent.AddListener((response) =>
        {
            float endTime = Time.realtimeSinceStartup;
            Debug.Log($"Respons tid: {endTime - startTime} sekunder.");
        });

        customGPTScript.SendGPTReply(input);

        // Vent til responsen er mottatt
        yield return new WaitUntil(() => customGPTScript.HasReceivedResponse); 
    }
}
