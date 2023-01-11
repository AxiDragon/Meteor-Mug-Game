using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneInput : MonoBehaviour
{
    [SerializeField] private string microphoneName;
    private AudioClip recording;

    void Start()
    {
        for (var index = 0; index < Microphone.devices.Length; index++)
        {
            string s = Microphone.devices[index];
            print(s);
            if (s == microphoneName)
                recording = Microphone.Start(microphoneName, true, 1, AudioSettings.outputSampleRate);
        }
        
        if (recording != null)
            Invoke(nameof(PlayRecording), 1f);
    }


    void PlayRecording()
    {
        AudioSource.PlayClipAtPoint(recording, transform.position);
        Invoke(nameof(PlayRecording), 1f);
    }
}