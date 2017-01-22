using SwissArmyKnife;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropagandaHover : Singleton<PropagandaHover>
{
    public float Distance = 100;
    public float Duration = 0.2f;
    public AnimationCurve Curve;

    private bool _active = false;
    private float _time = 0;
    private Vector3 _lastPosition;
    private Vector3 _nextPosition;
    private Text _text;

    void Start()
    {
        Hide();
        _text = GetComponentInChildren<Text>();
    }

    void Update () {
		if(_active)
        {
            _time += Time.deltaTime;
            var percent = Curve.Evaluate(Mathf.Clamp01(_time / Duration));

            transform.position = Vector3.Lerp(_lastPosition, _nextPosition, percent);

            if(_time > Duration)
            {
                _active = false;
            }
        }
	}

    public void SetHover(Propaganda propaganda, Vector3 position)
    {
        position.x -= Distance;
        _text.text = propaganda.Name;
        Move(position);
    }

    public void Hide()
    {
        var position = transform.position;
        position.x += 2 * Distance;
        Move(position);
    }

    private void Move(Vector3 location)
    {
        _lastPosition = transform.position;
        _nextPosition = location;
        _time = 0;
        _active = true;
    }
}
