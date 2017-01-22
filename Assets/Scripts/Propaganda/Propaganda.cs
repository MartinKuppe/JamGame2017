using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propaganda : MonoBehaviour
{
    public Sprite Poster;
    public string Name;
    public float Duration = 10;
    public float CooldownDuration = 1;

    public static uint SoundEffect = 0;

    [TextArea]
    public string Description;

    private ParticleControls _controls;
    private float _time = 0;

    private State _state = State.Idle;
    public enum State
    {
        Idle, 
        Using,
        Cooldown
    }

    private IPropagandaEffect[] _effects;

    public float Overlay
    {
        get
        {
            switch (_state)
            {
                case State.Using:
                    return Mathf.Clamp01(_time / Duration);
                case State.Cooldown:
                    return 1.0f - Mathf.Clamp01(_time / CooldownDuration);
                case State.Idle:
                default:
                    return 0;
            }
        }
    }

    public bool Usable { get { return _state == State.Idle; } }

    void Awake()
    {
        _effects = GetComponents<IPropagandaEffect>();
        SoundEffect = 0;
    }

    void Start()
    {
        StartCoroutine(DelaySoundInit());
    }

    private IEnumerator DelaySoundInit()
    {
        yield return new WaitForSeconds(0.5f);

        if (SoundEffect == 0)
        {
            SoundEffect = SoundSystem.Play("Propaganda");
            SoundSystem.Pause(SoundEffect);
        }
    }

    void Update()
    {
        var location = PropagandaEmitter.GetLocation();
        _controls.MoveTo(location);
        
        switch (_state)
        {
            case State.Using:
                _time += Time.deltaTime;

                Trigger(location);

                if (_time > Duration)
                {
                    _controls.Stop();
                    _time = 0;
                    _state = State.Cooldown;
                    PropagandaDescription.SetOngoingPropaganda(false);
                    SoundSystem.Pause(SoundEffect);
                }
                break;
            case State.Cooldown:
                _time += Time.deltaTime;

                if (_time > CooldownDuration)
                {
                    _state = State.Idle;
                }
                break;
            case State.Idle:
            default:
                break;
        }
    }

    public void Trigger()
    {
        if(_state == State.Idle)
        {
            PropagandaDescription.SetOngoingPropaganda(true);
            SoundSystem.Resume(SoundEffect);
            _state = State.Using;
            _time = 0;

            _controls = Particles.ControlledPlay(name);

            Trigger(PropagandaEmitter.GetLocation());
            SoundSystem.Play("P_" + name);
        }
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
