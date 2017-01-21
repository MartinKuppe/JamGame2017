using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propaganda : MonoBehaviour
{
    public Sprite Poster;
    public string Name;
    public float Duration = 10;

    [TextArea]
    public string Description;

    private ParticleControls _controls;
    private bool _active = false;
    private float _time = 0;

    private IPropagandaEffect[] _effects;

    void Awake()
    {
        _effects = GetComponents<IPropagandaEffect>();
    }

    void Update()
    {
        var location = PropagandaEmitter.GetLocation();
        _controls.MoveTo(location);

        if (_active)
        {
            _time += Time.deltaTime;

            Trigger(location);

            if (_time > Duration)
            {
                _controls.Stop();
                _active = false;
            }
        }
    }

    public void Trigger()
    {
        _active = true;
        _time = 0;

        _controls = Particles.ControlledPlay(name);

        Trigger(PropagandaEmitter.GetLocation());
    }

    private void Trigger(Vector3 location)
    {
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
