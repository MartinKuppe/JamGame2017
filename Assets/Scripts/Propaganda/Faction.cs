using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction : MonoBehaviour {

    public List<Propaganda> Propagandas;

	void Awake ()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var propaganda = transform.GetChild(i).GetComponent<Propaganda>();
            if (propaganda != null)
                Propagandas.Add(propaganda);
        }
	}
}
