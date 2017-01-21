using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



public class EffectDraft : MonoBehaviour, IPropagandaEffect
{
    public bool _proportionalToSupport;

    // Drafts soldiers
    public float _soldiersPerSecond = 0.1f;
    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if (city.IsRebelCity)
            {
                if(_proportionalToSupport)
                {
                    city.Draft(city.RebelSupport * _soldiersPerSecond * Time.deltaTime );
                }
                else
                {
                    city.Draft(_soldiersPerSecond * Time.deltaTime);
                }

            }
        }

    }
}