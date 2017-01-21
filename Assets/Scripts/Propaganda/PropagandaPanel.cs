using SwissArmyKnife;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaPanel : Singleton<PropagandaPanel>
{
    public GameObject ButtonPrefab;

    private void Reset()
    {
        while (transform.childCount > 0)
            Destroy(transform.GetChild(0).gameObject);
    } 

    public void CreateButtons(Faction faction)
    {
        Reset();

        for (int i = 0; i < faction.Propagandas.Count; i++)
        {
            var go = Instantiate(ButtonPrefab);
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.GetComponent<PropagandaButton>().Init(faction.Propagandas[i]);
        }
    }
}
