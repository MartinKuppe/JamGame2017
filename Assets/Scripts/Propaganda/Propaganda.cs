using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propaganda : MonoBehaviour
{
    public Sprite Image;
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
        Trigger(new Vector3()); // TODO Get coordinates
    }

    private void Trigger(Vector3 location)
    {
        for (int i = 0; i < _effects.Length; i++)
        {
            _effects[i].OnPropaganda(location);
        }
    }
}

public interface IPropagandaEffect
{
    void OnPropaganda(Vector3 Location);
}
