using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIndoctrination : MonoBehaviour, IPropagandaEffect
{
    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        string debug = "Effect_Indoctrination on ";

        foreach (var city in targets)
        {
            debug += city.gameObject.name + " ";
        }

        Debug.Log(debug);

    }
}
