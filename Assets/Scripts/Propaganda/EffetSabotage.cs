using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



///=================================================================================================================
///                                                                                                       <summary>
///  EffetSabotage is an effet to reduce enemy troops depending on rebel support.						 </summary>
///
///=================================================================================================================
public class EffetSabotage : MonoBehaviour, IPropagandaEffect
{
    // Kill soldiers
    public float _idealKillsPerSecond = 0.5f;
    public void OnPropaganda(Vector3 Location, List<City> targets)
    {
        foreach (City city in targets)
        {
            if ( !city.IsRebelCity)
            {
                city.Sabotage(city.RebelSupport * _idealKillsPerSecond * Time.deltaTime);
            }
        }
    }
}
