using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;


///=================================================================================================================
///                                                                                                       <summary>
///  EffetDeserteurs is a MonoBehaviour that does important stuff. 											 </summary>
///
///=================================================================================================================
public class EffetDeserteurs : MonoBehaviour, IPropagandaEffect
{
    // Make soldiers desert
    public float _idealDesertsPerSecond = 1.0f;
    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if (!city.IsRebelCity && city.IsFrontCity())
            {
                city.Desert(city.RebelSupport * _idealDesertsPerSecond * Time.deltaTime);
            }
        }
    }
}

