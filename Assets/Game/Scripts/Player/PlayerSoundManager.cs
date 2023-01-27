using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoundManager : MonoBehaviour
{
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }

    [UsedImplicitly]
    public void OnToggleSound(InputValue value)
    {
        if (value.isPressed)
        {
            soundManager.ToggleSoundTrack(!soundManager.soundTrackEnabled);
        }
    }
}
