using System.Collections.Generic;
using SwissArmyKnife;
using UnityEngine;

public class PropagandaButtonsPanel : Singleton<PropagandaButtonsPanel>
{
    public GameObject ButtonPrefab;
    public List<PropagandaButton> Buttons = new List<PropagandaButton>();

    public void CreateButtons(Faction faction)
    {
        Reset();

        foreach (var propaganda in faction.Propagandas)
        {
            var go = Instantiate(ButtonPrefab);
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(1, 1, 1);
            var button = go.GetComponent<PropagandaButton>();
            button.Init(propaganda);
            Buttons.Add(button);
        }

        Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(Buttons[0].Button.gameObject);
    }

    public void DisableOthers(PropagandaButton active)
    {
        foreach (var button in Buttons)
        {
            if (button == active) continue;

            button.Disable();
        }
    }

    private void Reset()
    {
        while (transform.childCount > 0)
            Destroy(transform.GetChild(0).gameObject);

        Buttons.Clear();
    }
}
