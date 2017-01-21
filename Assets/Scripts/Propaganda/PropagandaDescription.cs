using System;
using SwissArmyKnife;
using UnityEngine;
using UnityEngine.UI;

public class PropagandaDescription : Singleton<PropagandaDescription>
{
    public Image Poster;
    public Text Title;
    public Text Description;

    [Header("State")]
    public Propaganda Propaganda;
    public Propaganda WantedPropaganda;

    public float Distance;
    public float Duration = 0.5f;
    public AnimationCurve Curve;

    private uint _animationKey;
    private float _time;
    private float _outsideX;
    private float _insideX;
    private float _targetX;
    private float _lastX;

    private State _state;
    private enum State
    {
        Entering,
        Exiting,
        Inside,
        Outside
    }
    
    void Start ()
    {
        var pos = (transform as RectTransform).anchoredPosition;
        _insideX = pos.x;
        _outsideX = _insideX + Distance;

        // Initial position --
        pos.x = _outsideX;
        (transform as RectTransform).anchoredPosition = pos;
        _state = State.Outside;
    }
	
	void Update () {

        if(Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            if (_state == State.Inside)
            {
                PropagandaDescription.SetPropaganda(null);
            }
        }

		if(_state == State.Entering)
        {
            _time += Time.deltaTime;
            float percent = Mathf.Clamp01(_time / Duration);

            var pos = (transform as RectTransform).anchoredPosition;
            pos.x = Mathf.Lerp(_outsideX, _insideX, Curve.Evaluate(percent));
            (transform as RectTransform).anchoredPosition = pos;

            if(_time > Duration)
            {
                _state = State.Inside;
            }
        }
        else if (_state == State.Exiting)
        {
            _time += Time.deltaTime;
            float percent = Mathf.Clamp01(_time / Duration);

            var pos = (transform as RectTransform).anchoredPosition;
            pos.x = Mathf.Lerp(_insideX, _outsideX, Curve.Evaluate(percent));
            (transform as RectTransform).anchoredPosition = pos; ;

            if (_time > Duration)
            {
                _state = State.Outside;
                Propaganda = null;

                if(WantedPropaganda != null)
                {
                    DefinePropaganda(WantedPropaganda);
                }
            }
        }
    }

    private void TriggerAnimation(bool inside)
    {
        _targetX = inside ? _insideX : _outsideX;
        _lastX = transform.localPosition.x;
    }
    
    private void UpdatePropaganda(Propaganda propaganda)
    {
        switch (_state)
        {
            case State.Entering:
                if (propaganda == Propaganda)
                    break;
                else
                {
                    Reverse();
                    WantedPropaganda = propaganda;
                }
                break;
            case State.Exiting:
                if (propaganda == Propaganda)
                    Reverse();
                else
                    WantedPropaganda = propaganda;
                break;
            case State.Inside:
                if (propaganda == Propaganda)
                    break;
                else
                {
                    _time = 0;
                    WantedPropaganda = propaganda;
                    _state = State.Exiting;
                }
                break;
            case State.Outside:
                if(propaganda != null)
                    DefinePropaganda(propaganda);
                break;
            default:
                break;
        }
    }

    private void Reverse()
    {
        _state = _state == State.Entering ? State.Exiting : State.Entering;
        _time = Duration - _time;
    }

    private void DefinePropaganda(Propaganda propaganda)
    {
        _time = 0;
        _state = State.Entering;

        Propaganda = propaganda;
        WantedPropaganda = null;

        Poster.sprite = propaganda.Poster;
        Title.text = propaganda.Name;
        Description.text = propaganda.Description;
    }

    public static void SetPropaganda(Propaganda propaganda)
    {
        if (Instance != null)
            Instance.UpdatePropaganda(propaganda);
        else PropagandaButtonsPanel.Instance.RefocusControl();
    }
    public void Trigger()
    {
        if (Propaganda != null)
            Propaganda.Trigger();
    }
}
