using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



public class EffetBattleForce : MonoBehaviour, IPropagandaEffect
{
    // Modifies the battle force
    public Affiliation _affiliation;
    public float _attackForce = 1.0f;
    public float _defenseForce = 1.0f;

    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if (city._occupyingFaction == _affiliation )
            {
                // City belonging to the specified faction
                city.ForceOfDefenders = _defenseForce;
            }
            else
            {
                // City belonging to the other faction
                city.ForceOfAttackers = _attackForce;
            }
        }

    }
}
