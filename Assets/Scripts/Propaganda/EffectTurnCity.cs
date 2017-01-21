using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



public class EffectTurnCity : MonoBehaviour, IPropagandaEffect
{
    // takes over a loyalist city with full rebel support

    public float _probabilityFactor = 1.0f;

    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if ( !city.IsRebelCity && city.RebelFlags == 3 && Random.value < Time.deltaTime * _probabilityFactor )
            {
                city._occupyingFaction = Affiliation.Rebels;
            }
        }

    }
}

