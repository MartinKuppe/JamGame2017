using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIndoctrination : MonoBehaviour, IPropagandaEffect
{
    public void OnPropaganda(Vector3 Location)
    {
        Debug.Log("Effect_Indoctrination");
    }
}
