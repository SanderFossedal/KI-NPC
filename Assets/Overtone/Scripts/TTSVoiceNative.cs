using System;
using System.Threading;
using UnityEngine;

namespace LeastSquares.Overtone
{
    public class TTSVoiceNative
    {
        public const int Timeout = 8000;
        public IntPtr Pointer { get; set; }
        public FixedPointerToHeapAllocatedMem ConfigPointer { get; set; }
        public FixedPointerToHeapAllocatedMem ModelPointer { get; set; }
        public bool Disposed { get; private set; }
        private readonly ReaderWriterLock _lock = new ReaderWriterLock();
        
        public static TTSVoiceNative LoadVoiceFromResources(string voiceName)
        {
            var modelAsset = Resources.Load<TextAsset>($"{voiceName}");
            var configAsset = Resources.Load<TextAsset>($"{voiceName}.config");
            if (modelAsset == null)
            {
                Debug.LogError($"Failed to find voice model {voiceName}.bytes in Resources");
                return null;
            }
            if (configAsset == null)
            {
                Debug.LogError($"Failed to find voice model {voiceName}.config.json in Resources");
                return null;
            }

            var configBytes = configAsset.bytes;
            var modelBytes = modelAsset.bytes;
            var configPtr = FixedPointerToHeapAllocatedMem.Create(configBytes, (uint) configBytes.Length);
            var modelPtr = FixedPointerToHeapAllocatedMem.Create(modelBytes, (uint) modelBytes.Length);
            
            var ptr = TTSNative.OvertoneLoadVoice(
                configPtr.Address, 
                configPtr.SizeInBytes,
                modelPtr.Address, 
                modelPtr.SizeInBytes
            );
            TTSNative.OvertoneSetSpeakerId(ptr, 0);
            return new TTSVoiceNative
            {
                Pointer = ptr,
                ConfigPointer = configPtr,
                ModelPointer = modelPtr,
            };
        }

        public void SetSpeakerId(int speakerId)
        {
            TTSNative.OvertoneSetSpeakerId(Pointer, speakerId);
        }

        public void AcquireReaderLock()
        {
            _lock.AcquireReaderLock(Timeout);
        }
        
        public void ReleaseReaderLock()
        {
            _lock.ReleaseReaderLock();
        }

        public void Dispose()
        {
            _lock.AcquireWriterLock(Timeout);
            Disposed = true;
            ConfigPointer.Free();
            ModelPointer.Free();
            TTSNative.OvertoneFreeVoice(Pointer);
            _lock.ReleaseWriterLock();
        }
    }
}