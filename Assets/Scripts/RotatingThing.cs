using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



///=================================================================================================================
///                                                                                                       <summary>
///  RotatingThing is a MonoBehaviour that does important stuff. 											 </summary>
///
///=================================================================================================================
public class RotatingThing : MonoBehaviour 
{
    public Vector3 _eulerAngles;

    public void Update()
    {
        transform.Rotate(Time.deltaTime * _eulerAngles);
    }
}
