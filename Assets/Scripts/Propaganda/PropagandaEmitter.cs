using SwissArmyKnife;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaEmitter : Singleton<PropagandaEmitter> {

    public float Range = 5;

    [SerializeField]
    private List<City> _cities = new List<City>();

	void Update () {
        for (int i = 0; i < _cities.Count; i++)
        {
            var city = _cities[i];
            float distance = (city.Position - transform.position).magnitude;

            city.SetHighlighted(distance <= Range);
        }
	}

    public static Vector3 GetLocation()
    {
        return Instance != null ? Instance.transform.position : new Vector3();
    }

    public static void RegisterCity(Vector3 position)
    {
        if (Instance == null)
            return;

        var city = new City();
        city.Position = position;
        Instance._cities.Add(city);
    }

    [Serializable]
    public class City
    {
        public Vector3 Position;
        public bool Active = false;
        public ParticleControls controls;
        
        public void SetHighlighted(bool active)
        {
            if (active == Active)
                return;

            if(active)
            {
                controls = Particles.ControlledPlayAt("Highlight", Position);
            }
            else
            {
                controls.Stop();
            }

            Active = active;
        }
    }
}
