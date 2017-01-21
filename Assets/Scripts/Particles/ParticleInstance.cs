using UnityEngine;
using System.Collections;

public class ParticleInstance : MonoBehaviour {
    
    public uint Key = 0;
    public Particle Particle;

    public State CurrentState {
        get { return _state; }
        set
        {
            _state = value;

            // Update name --
            name = Particle == null ? "Uninitialised" : Particle.Id + "-" + _state;

            // Trigger transitions --
            switch (_state)
            {
                case State.Uninitialised:
                    break;
                case State.Idle:
                    // Disable everything --
                    Key = 0;
                    if(_rootSystem != null)
                        _rootSystem.Stop(withChildren: true);
                    break;
                case State.Playing:
                    // Enable everything --
                    Key = Particles.GetParticleKey();
                    _rootSystem.Play(withChildren: true);
                    break;
                default:
                    break;
            }
        }
    }
    public enum State
    {
        Uninitialised,
        Idle,
        Playing
    }

    public bool IsPlaying { get { return CurrentState == State.Playing; } }

    private State _state = State.Uninitialised;
    private ParticleSystem _rootSystem;

    void Awake()
    {
        if(Particle == null)
        {
            CurrentState = State.Idle;
        }
    }

    public void Init(Particle particle)
    {
        Particle = particle;

        // Instantiate the prefab --
        var go = Instantiate(particle.ParticlePrefab);
        go.transform.SetParent(transform);

        particle.AddInstance(this);

        // Get particle systems --
        _rootSystem = GetComponentInChildren<ParticleSystem>();

        // Update state --
        CurrentState = State.Idle;
    }

    public void InternalUpdate()
    {
        if(Key != 0 && IsPlaying && !_rootSystem.IsAlive(true))
            CurrentState = State.Idle;
    }

    public void PlayAt(Vector3 position)
    {
        CurrentState = State.Playing;
        MoveTo(position);
    }

    public ParticleControls ControlledPlay()
    {
        CurrentState = State.Playing;
        return new ParticleControls(this);
    }

    public ParticleControls ControlledPlayAt(Vector3 position)
    {
        var controls = ControlledPlay();
        MoveTo(position);
        return controls;
    }

    public void Stop()
    {
        CurrentState = State.Idle;
    }

    public void MoveTo(Vector3 position)
    {
        transform.localPosition = position;
    }

    public void RotateTo(Vector3 rotation)
    {
        transform.localEulerAngles = rotation;
    }

    public void RotateTo(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void ScaleTo(Vector3 scale)
    {
        transform.localScale = scale;
    }
}
