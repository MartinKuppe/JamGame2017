using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ParticleRegistry", menuName = "Particles/ParticleRegistry", order = 2)]
public class ParticleRegistry : ScriptableObject
{
    public List<Particle> Particles = new List<Particle>();
}
