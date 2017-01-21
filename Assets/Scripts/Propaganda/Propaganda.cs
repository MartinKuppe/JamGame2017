using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propaganda : MonoBehaviour
{
    public Sprite Poster;
    public string Name;

    [TextArea]
    public string Description;

    private IPropagandaEffect[] _effects;

    void Awake()
    {
        _effects = GetComponents<IPropagandaEffect>();
    }

    public void Trigger()
    {
        Trigger(PropagandaEmitter.GetLocation());
    }

    private void Trigger(Vector3 location)
    {
        Particles.PlayAt(name, location);

        for (int i = 0; i < _effects.Length; i++)
        {
            _effects[i].OnPropaganda(location, PropagandaEmitter.GetNearCities());
        }
    }
}

public interface IPropagandaEffect
{
    void OnPropaganda(Vector3 Location, List<City> targets);
}
