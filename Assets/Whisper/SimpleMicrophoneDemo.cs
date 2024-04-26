using LLMUnitySamples;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class SimpleMicrophoneDemo : MonoBehaviour
{
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;
    public ChatBotVoiceInput chatBotVoiceInput;

    private string _buffer;

    private void Awake()
    {
        // Set up event listeners
        microphoneRecord.OnRecordStop += OnRecordStop;

    }
    private void Update()
    {
        // Check for spacebar press to start or stop recording
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!microphoneRecord.IsRecording)
            {
                microphoneRecord.StartRecord();
                UnityEngine.Debug.Log("Recording started...");
            }
            else
            {
                microphoneRecord.StopRecord();
                UnityEngine.Debug.Log("Recording stopped.");
            }
        }
    }


    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        _buffer = "";


        //UnityEngine.Debug.Log("Processing audio...");
        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

        if (res == null)
        {
            UnityEngine.Debug.Log("No text recognized.");
            return;
        }

        _buffer = res.Result;
        chatBotVoiceInput.ProcessVoiceInput(_buffer);

    }


}

