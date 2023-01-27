using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Lobby soundtrack first, in-game soundtrack second")]
    public AudioSource[] audioSources;
    [Range(0, 1)]
    public float soundtrackVolume;
    private MMF_Player toggleSoundPlayer;
    
    private bool lobbyEnabled = true;
    private bool gameSoundtrackEnabled = false;
    [HideInInspector] public bool soundTrackEnabled = true;


    private void Awake()
    {
        toggleSoundPlayer = GetComponent<MMF_Player>();
    }

    private void Start()
    {
        audioSources[0].volume = soundtrackVolume;
    }

    void Update()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].pitch = Time.timeScale;
        }
    }

    public void ToggleLobbySoundtrack(bool on)
    {
        lobbyEnabled = on;

        if (soundTrackEnabled)
            audioSources[0].DOFade(on ? soundtrackVolume : 0f, 1f).SetUpdate(true);
    }

    public void ToggleGameSoundtrack(bool on)
    {
        gameSoundtrackEnabled = on;

        if (soundTrackEnabled)
            audioSources[1].DOFade(on ? soundtrackVolume : 0f, 1f).SetUpdate(true);
    }

    public void ToggleSoundTrack(bool on)
    {
        soundTrackEnabled = on;
        toggleSoundPlayer.PlayFeedbacks();

        if (on)
        {
            ToggleGameSoundtrack(gameSoundtrackEnabled);
            ToggleLobbySoundtrack(lobbyEnabled);
        }
        else
        {
            audioSources[0].DOFade(0f, 1f).SetUpdate(true);
            audioSources[1].DOFade(0f, 1f).SetUpdate(true);
        }
    }
}