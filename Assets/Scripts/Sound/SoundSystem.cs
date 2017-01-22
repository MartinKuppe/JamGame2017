using UnityEngine;
using SwissArmyKnife;
using System.Collections.Generic;
using System;

public enum Layer { UI, Atmosphere, SoundEffects }

public class SoundSystem : SingletonPersistent<SoundSystem> {

    public SoundSettings Settings;
    public int PoolSize = 10;

    private VolumeNodeInstance _masterVolume;
    private Dictionary<string, Sound> _registry = new Dictionary<string, Sound>();
    private Dictionary<Layer, VolumeNodeInstance> _layersVolume = new Dictionary<Layer, VolumeNodeInstance>();
    private List<SoundInstance> _pool = new List<SoundInstance>();

    private static uint soundInstanceId = 0;

  	void Awake () {
          Reset();
  	}

    #region Internal
    private void Reset()
    {
        // Destroy each child --
        var transform = gameObject.transform;
        while (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0));
        }

        // Security check --
        if (Settings == null)
        {
            Debug.LogAssertion("SoundSystem.Settings must be defined");
            return;
        }

        // Reset volumes --
        _masterVolume = new VolumeNodeInstance(Settings.Master, null);
        _layersVolume.Clear();

        int i = 0;
        foreach (Layer layer in Enum.GetValues(typeof(Layer)))
        {
            var layerSettings = Settings.Layers.Count > i ? Settings.Layers[i].Volume : new VolumeNode();
            _layersVolume.Add(layer, new VolumeNodeInstance(layerSettings, _masterVolume));
            i++;
        }

        // Create the sound registry --
        var sounds = Settings.Sounds;
        for (int j = 0; j < sounds.Count; j++)
        {
            var sound = sounds[j];
            _registry.Add(sound.Name.Trim(), sound);
        }

        // Instantiate the pool --
        for (int j = 0; j < PoolSize; j++)
        {
            // Instantiate and register a new AudioSource --
            var go = new GameObject();
            go.transform.SetParent(transform);
            _pool.Add(go.AddComponent<SoundInstance>());
        }

    }

    public float GetVolume()
    {
        return _masterVolume.GetVolume();
    }

    public float GetVolume(Layer layer)
    {
        return _layersVolume[layer].GetVolume();
    }

    public VolumeNodeInstance GetLayerVolumeNode(Layer layer)
    {
        return Application.isPlaying ? _layersVolume[layer] : null;
    }

    public VolumeNodeInstance GetMasterVolumeNode()
    {
        return Application.isPlaying ? _masterVolume : null;
    }

    private SoundInstance GetFreeSlot()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i].IsAvailable())
                return _pool[i];
        }

        return null;
    }

    public bool InternalIsAlreadyPlaying(string soundName)
    {
        Sound sound;
        _registry.TryGetValue(soundName.Trim(), out sound);
        if (sound == null)
        {
            Debug.LogError("Sound '" + soundName + "' not found in SoundSettings");
            return false;
        }

        for (int i = 0; i < _pool.Count; i++)
        {
            var slot = _pool[i];
            if (slot.Sound != null && slot.CurrentState == SoundInstance.State.Playing && slot.Sound.Name == sound.Name)
                return true;
        }

        return false;
    }
    #endregion

    #region Controls
    private uint InternalPlay(string soundName)
    {
        soundName = soundName.Trim();
        if (string.IsNullOrEmpty(soundName))
            return 0;

        // Get a slot from the pool --
        var soundInst = GetFreeSlot();
        if(soundInst == null)
        {
            Debug.LogWarning("No sound slot available.");
            return 0;
        }

        // Get the sound from the registry --
        Sound sound;
        _registry.TryGetValue(soundName.Trim(), out sound);
        if (sound == null)
        {
            Debug.LogWarning("Sound '"+ soundName + "' not found in SoundSettings");
            return 0;
        }
        
        if(sound.Layer == Layer.Atmosphere)
            StopAllInLayer(Layer.Atmosphere);

        // Play the sound --
        return soundInst.Play(GetLayerVolumeNode(sound.Layer), sound);
    }

    private void InternalStop(uint instanceId)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            var slot = _pool[i];
            if (slot.Id == instanceId)
                slot.Stop();
        }
    }


    private void InternalPause(uint instanceId)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            var slot = _pool[i];
            if (slot.Id == instanceId)
                slot.Pause();
        }
    }

    private void InternalResume(uint instanceId)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            var slot = _pool[i];
            if (slot.Id == instanceId)
                slot.Resume();
        }
    }

    private void StopAllInLayer(Layer layer)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            var slot = _pool[i];

            if (slot.Sound != null)
                if (slot.Sound.Layer == layer)
                    slot.Stop();
        }
    }

    public void SetMute(bool active)
    {
        _masterVolume.IsMute = active;
    }

    public void SetMute(Layer layer, bool active)
    {
        _layersVolume[layer].IsMute = active;
    }

    public void ToggleMute()
    {
        _masterVolume.IsMute = !_masterVolume.IsMute;
    }

    public void ToggleMute(Layer layer)
    {
        var layerInstance = _layersVolume[layer];
        layerInstance.IsMute = !layerInstance.IsMute;
    }
    #endregion

    #region Singleton
    static public uint Play(string soundName)
    {
        return Instance == null ? 0 : Instance.InternalPlay(soundName);
    }

    static public bool IsAlreadyPlaying(string soundName)
    {
        return Instance == null ? false : Instance.InternalIsAlreadyPlaying(soundName);
    }

    static public uint GetSoundInstanceId()
    {
        soundInstanceId++;
        return soundInstanceId;
    }

    static public void Stop(uint instanceId)
    {
        // Invalid instanceId
        if (instanceId == 0 || Instance == null)
            return;

        Instance.InternalStop(instanceId);
    }

    static public void Pause(uint instanceId)
    {
        // Invalid instanceId
        if (instanceId == 0 || Instance == null)
            return;

        Instance.InternalPause(instanceId);
    }

    static public void Resume(uint instanceId)
    {
        // Invalid instanceId
        if (instanceId == 0 || Instance == null)
            return;

        Instance.InternalResume(instanceId);
    }

    static public void ClearLayer(Layer layer)
    {
        if (Instance != null)
            Instance.StopAllInLayer(layer);
    }
    #endregion
}

public class VolumeNodeInstance
{
    public bool IsMute { get { return _isMute; } set { _isMute = value; } }
    public float UserVolume { get { return _userVolume; } set { _userVolume = value; } }

    private VolumeNode _settings;
    private VolumeNodeInstance _parent;

    [SerializeField] private bool _isMute = false;
    [SerializeField] private float _userVolume = 1;

    public VolumeNodeInstance(VolumeNode node, VolumeNodeInstance parent)
    {
        _parent = parent;
        _settings = node;
        _userVolume = node.DefaultVolume;
    }

    public float GetVolume()
    {
        var parentVolume = _parent == null ? 1 : _parent.GetVolume();
        var mute = _isMute ? 0 : 1;
        return mute * parentVolume * _userVolume * _settings.VolumeCoef;
    }
}
