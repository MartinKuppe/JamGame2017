using SwissArmyKnife;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaEmitter : Singleton<PropagandaEmitter> {

    public float Range = 5;

    [SerializeField]
    private List<InternalCity> _cities = new List<InternalCity>();

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
    
    public static List<City> GetNearCities()
    {
        var list = new List<City>();

        if (Instance != null)
        {
            for (int i = 0; i < Instance._cities.Count; i++)
            {
                var city = Instance._cities[i];
                float distance = (city.Position - Instance.transform.position).magnitude;

                if (distance <= Instance.Range)
                {
                    list.Add(city.City);
                }
            }
        }

        return list;        
    }

    public static void RegisterCity(Vector3 position, City city)
    {
        if (Instance == null)
            return;

        Instance._cities.Add(new InternalCity(position, city));
    }

    [Serializable]
    public class InternalCity
    {
        public Vector3 Position;
        public bool Active = false;
        public ParticleControls controls;
        public City City;

        public InternalCity(Vector3 position, City city)
        {
            Position = position;
            this.City = city;
        }

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
