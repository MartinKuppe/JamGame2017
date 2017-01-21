using System.Collections.Generic;
using SwissArmyKnife;
using UnityEngine;
using UnityEngine.UI;

public class PropagandaButtonsPanel : Singleton<PropagandaButtonsPanel>
{
    public GameObject ButtonPrefab;
    public List<PropagandaButton> Buttons = new List<PropagandaButton>();

    private ScrollRect _scrollRect;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    public void CreateButtons(Faction faction)
    {
        Reset();

        foreach (var propaganda in faction.Propagandas)
        {
            var go = Instantiate(ButtonPrefab);
            go.transform.SetParent(_scrollRect.content, false);
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
        Buttons.Clear();

        foreach (Transform child in _scrollRect.content)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
