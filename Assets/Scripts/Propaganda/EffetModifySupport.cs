using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



public class EffetModifySupport : MonoBehaviour, IPropagandaEffect
{
    // Increases or decreases support in certain cities
    public bool _inRebelCities;
    public bool _inLoyalistCities;

    public float _supportPerSecond = 0.1f;
    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if (city.IsRebelCity && _inRebelCities)
            {
                city.RebelSupport += _supportPerSecond * Time.deltaTime;
            }
            else if (!city.IsRebelCity && _inLoyalistCities)
            {
                city.RebelSupport += _supportPerSecond * Time.deltaTime * city._conversionFactor;
            }
        }

    }
}


