using UnityEngine;
using System.Collections.Generic;
using SwissArmyKnife;

public class Particles : Singleton<Particles> {

    public ParticleRegistry Registry;

    private Dictionary<string, Particle> _registry = new Dictionary<string, Particle>();
    private LinkedList<ParticleInstance> _pool = new LinkedList<ParticleInstance>();

    private static uint _particleKey = 0;

    void Awake()
    {
        Reset();
    }
	
	void Update () {
        foreach (var particleInstance in _pool)
        {
            particleInstance.InternalUpdate();
        }
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
        if (Registry == null)
        {
            Debug.LogAssertion("Particles.Registry must be defined");
            return;
        }
        
        // Create the particle registry --
        var sounds = Registry.Particles;
        for (int j = 0; j < sounds.Count; j++)
        {
            var particle = sounds[j];
            _registry.Add(particle.Id.Trim(), particle);
        }

    }

    private ParticleInstance GetInstance(string particleName)
    {
        particleName = particleName.Trim();

        if (string.IsNullOrEmpty(particleName))
            return null;

        // Get the particle from the registry --
        Particle particle;
        _registry.TryGetValue(particleName, out particle);
        if (particle == null)
        {
            Debug.LogWarning("Particle '" + particleName + "' not found in " + Registry.name);
            return null;
        }

        ParticleInstance instance = particle.GetInactiveInstance();

        if (instance == null)
        {
            if (particle.RemainingInstanceSlot)
            {
                var go = new GameObject();
                go.transform.SetParent(transform);
                instance = go.AddComponent<ParticleInstance>();
                instance.Init(particle);
                _pool.AddLast(instance);
            }
        }

        return instance;
    }
    #endregion

    #region Controls
    private void InternalPlayAt(string particleName, Vector3 position)
    {
        var instance = GetInstance(particleName);
        if(instance != null)
        {
            instance.PlayAt(position);
        }        
    }

    private ParticleControls InternalControlledPlay(string particleName)
    {
        var instance = GetInstance(particleName);
        if (instance != null)
        {
            return instance.ControlledPlay();
        }

        return new ParticleControls();
    }

    private ParticleControls InternalControlledPlayAt(string particleName, Vector3 position)
    {
        var instance = GetInstance(particleName);
        if (instance != null)
        {
            return instance.ControlledPlayAt(position);
        }

        return new ParticleControls();
    }
    #endregion

    #region Singleton
    static public void PlayAt(string particleName, Vector3 position)
    {
        if(Instance != null)
            Instance.InternalPlayAt(particleName, position);
    }

    static public ParticleControls ControlledPlay(string particleName)
    {
        return Instance == null ? new ParticleControls() : Instance.InternalControlledPlay(particleName);
    }

    static public ParticleControls ControlledPlayAt(string particleName, Vector3 position)
    {
        return Instance == null ? new ParticleControls() : Instance.InternalControlledPlayAt(particleName, position);
    }

    static public uint GetParticleKey()
    {
        _particleKey++;
        return _particleKey;
    }
    #endregion
}
