using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Overtone.Scripts;
using UnityEngine;
using UnityEngine.Assertions;

namespace LeastSquares.Overtone
{
    public class TTSEngine : MonoBehaviour
    {
        public bool Loaded { get; private set; }
        private IntPtr _context;
        private readonly object _lock = new object();
        public bool Disposed { get; private set; }
        
        void Awake()
        {
            lock (_lock)
            {
                _context = TTSNative.OvertoneStart();
                Debug.Log("Loaded speech model successfully");
                Loaded = true;
            }
        }
        
        public async Task<AudioClip> Speak(string text, TTSVoiceNative voice)
        {
            
            Debug.Log($"TTS for '{text ?? string.Empty}'");
            var units = SSMLPreprocessor.Preprocess(text );
            var samples = new List<float>();
            TTSResult result = null;
            foreach (var unit in units)
            {
                result = await SpeakSamples(unit, voice);
                samples.AddRange(result.Samples);
            }
            
            Debug.Log($"Done. Returned '{samples.Count}' samples");
            return MakeClip(text, new TTSResult
            {
                Samples = samples.ToArray(),
                Channels = result.Channels,
                SampleRate = result.SampleRate
            });
        }

        private AudioClip MakeClip(string name, TTSResult result)
        {
            var clip = AudioClip.Create(name ?? string.Empty, (int)result.Samples.Length, (int)result.Channels, (int)result.SampleRate, false);
            clip.SetData(result.Samples, 0);
            return clip;
        }
        
        public async Task<TTSResult> SpeakSamples(SpeechUnit unit, TTSVoiceNative voice)
        {
            var tcs = new TaskCompletionSource<TTSResult>();
        
            float[] samples = null;
            using var textPtr = new FixedString(unit.Text);
            var result = new TTSNative.OvertoneResult
            {
                Channels = 0
            };

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        voice.AcquireReaderLock();
                        if (Disposed || voice.Disposed)
                        {
                            samples = Array.Empty<float>();
                            Debug.LogWarning("Couldn't process TTS. TTSEngine or TTSVoiceNative has been disposed.");
                            return;
                        }
                        Assert.AreNotEqual(voice.Pointer, IntPtr.Zero);
                        Assert.AreNotEqual(voice.ConfigPointer.Address, IntPtr.Zero);
                        Assert.AreNotEqual(voice.ModelPointer.Address, IntPtr.Zero);
                        Assert.AreNotEqual(_context, IntPtr.Zero);
                        Assert.AreNotEqual(textPtr.Address, IntPtr.Zero);
                        result = TTSNative.OvertoneText2Audio(_context, textPtr.Address, voice.Pointer);
                        samples = PtrToSamples(result.Samples, result.LengthSamples);
                        voice.ReleaseReaderLock();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error while processing TTS: {e.Message}");
                        tcs.SetException(e);
                    }
                }
            });

            tcs.SetResult(new TTSResult
            {
                Channels = result.Channels,
                SampleRate = result.SampleRate,
                Samples = samples
            });

            TTSNative.OvertoneFreeResult(result);

            return await tcs.Task;
        }
        
        
        private float[] PtrToSamples(IntPtr int16Buffer, uint samplesLength)
        {
            var floatSamples = new float[samplesLength];
            var int16Samples = new short[samplesLength];

            Marshal.Copy(int16Buffer, int16Samples, 0, (int)samplesLength);

            for (int i = 0; i < samplesLength; i++)
            {
                floatSamples[i] = int16Samples[i] / (float)short.MaxValue;
            }
        
            return floatSamples;
        }

        void OnDestroy()
        {
            Debug.Log("Marking as should destroy");
            Dispose();
        }

        void Dispose()
        {
            lock (_lock)
            {
                Disposed = true;
                if (_context != IntPtr.Zero)
                    TTSNative.OvertoneFree(_context);
                Debug.Log("Successfully cleaned up TTS Engine");
            }
        }
    }

    public class TTSResult
    {
        public float[] Samples;
        public uint Channels;
        public uint SampleRate;
    }
}