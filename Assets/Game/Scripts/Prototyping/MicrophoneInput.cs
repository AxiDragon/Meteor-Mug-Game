using UnityEngine;

namespace Prototyping
{
    public class MicrophoneInput : MonoBehaviour
    {
        [SerializeField] private string microphoneName;
        private AudioClip recording;

#if !UNITY_WEBGL
        private void Start()
        {
            for (var index = 0; index < Microphone.devices.Length; index++)
            {
                var s = Microphone.devices[index];
                print(s);
                if (s == microphoneName)
                    recording = Microphone.Start(microphoneName, true, 1, AudioSettings.outputSampleRate);
            }

            if (recording != null)
                Invoke(nameof(PlayRecording), 1f);
        }


        private void PlayRecording()
        {
            AudioSource.PlayClipAtPoint(recording, transform.position);
            Invoke(nameof(PlayRecording), 1f);
        }
    }
#endif
    }
}