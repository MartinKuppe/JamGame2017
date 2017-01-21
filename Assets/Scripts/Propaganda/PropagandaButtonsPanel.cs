using SwissArmyKnife;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaButtonsPanel : Singleton<PropagandaButtonsPanel>
{
    public GameObject ButtonPrefab;
    public List<PropagandaButton> Buttons = new List<PropagandaButton>();

    private void Reset()
    {
        while (transform.childCount > 0)
            Destroy(transform.GetChild(0).gameObject);

        Buttons.Clear();
    } 

    public void CreateButtons(Faction faction)
    {
        Reset();

        for (int i = 0; i < faction.Propagandas.Count; i++)
        {
            var go = Instantiate(ButtonPrefab);
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(1, 1, 1);
            var button = go.GetComponent<PropagandaButton>();
            button.Init(faction.Propagandas[i]);
            Buttons.Add(button);
        }
    }

    public void DisableOthers(PropagandaButton active)
    {
        foreach (var button in Buttons)
        {
            if (button == active)
                continue;

            button.Disable();
        }
    }
}
