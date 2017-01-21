using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



public class EffetSelfEsteem : MonoBehaviour, IPropagandaEffect
{
    // Modifies the perceived size of own and enemy forces
    public Affiliation _affiliation;
    public float _selfEsteem = 1.0f;

    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if (city._occupyingFaction == _affiliation)
            {
                // City belonging to the specified faction
                city.SelfEsteem = _selfEsteem;
            }
        }

    }
}

