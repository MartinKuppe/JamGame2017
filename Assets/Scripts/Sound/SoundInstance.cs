using UnityEngine;
using System.Collections;
using System;

public class SoundInstance : MonoBehaviour {

    public VolumeNodeInstance Volume;
    public Sound Sound;
    public uint Id = 0;

    public State CurrentState;
    public enum State
    {
        Idle,
        Playing,
        Paused
    }

    private AudioSource _source;

    void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        CurrentState = State.Idle;
        name = "Idle";
    }

    void Update()
    {
        if(IsPlaying())
        {
            ConstraintAudioSource();

            // Check if the source is still playing --
            if (!_source.isPlaying && CurrentState != State.Paused)
            {
                Free();
            }
        }
    }

    public bool IsAvailable()
    {
        return CurrentState == State.Idle;
    }

    public bool IsPlaying()
    {
        return CurrentState == State.Playing || CurrentState == State.Paused;
    }

    public uint Play(VolumeNodeInstance layer, Sound sound)
    {
        Sound = sound;

        // Interface volume with Sound's info --
        var volumeNode = new VolumeNode();
        volumeNode.VolumeCoef = sound.Volume;
        Volume = new VolumeNodeInstance(volumeNode, layer);

        // Add the AudioSource --
        _source.clip = Sound.AudioClip;
        _source.loop = Sound.Loop;
        _source.Play();

        ConstraintAudioSource();

        // Set up the state --
        name = Sound.name;
        CurrentState = State.Playing;

        // Set the id --
        Id = SoundSystem.GetSoundInstanceId();
        return Id;
    }

    public void Stop()
    {
        _source.Stop();
        Free();
    }

    private void Free()
    {
        Id = 0;
        _source.loop = false;
        _source.clip = null;
        Sound = null;
        CurrentState = State.Idle;
        name = "Idle";
    }

    private void ConstraintAudioSource()
    {
        _source.volume = Volume.GetVolume();
        _source.pitch = Sound.Pitch;
        _source.panStereo = Sound.StereoPan;
    }

    internal void Pause()
    {
        Debug.Log("Pause");
        Volume.IsMute = true;
    }

    internal void Resume()
    {
        Debug.Log("Resume");
        Volume.IsMute = false;
    }
}
