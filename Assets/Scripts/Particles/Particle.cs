using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Particle", menuName = "Particles/Particle", order = 1)]
public class Particle : ScriptableObject {

    public string Id;
    public GameObject ParticlePrefab;

    [Header("Settings")]
    [Range(1, 10)]
    public int MaxInstance = 1;

    #region Runtime
    public int InstanceNumber { get { return _instances.Count; } }
    public bool RemainingInstanceSlot { get { return _instances.Count < MaxInstance; } }

    private List<ParticleInstance> _instances;

    void OnEnable()
    {
        _instances = new List<ParticleInstance>(MaxInstance);
    }

    public ParticleInstance GetInactiveInstance()
    {
        for (int i = 0; i < _instances.Count; i++)
        {
            if (_instances[i].Key == 0)
                return _instances[i];
        }

        return null;
    }

    public void AddInstance(ParticleInstance inst)
    {
        _instances.Add(inst);
    }

    #endregion
}
