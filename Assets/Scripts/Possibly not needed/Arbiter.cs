using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;



///=================================================================================================================
///                                                                                                       <summary>
///  Arbiter is a singleton providing strategic informations, such as the list of cities.   			 </summary>
///
///=================================================================================================================
public class Arbiter : MonoBehaviour 
{
    public static Arbiter Instance { get; private set; }
    public City[] _cities;

    void Awake()
    {
        Instance = this;
        _cities = FindObjectsOfType<City>();
    }


}

