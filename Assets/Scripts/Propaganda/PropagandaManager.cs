using SwissArmyKnife;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagandaManager : Singleton<PropagandaManager>
{
    public Faction CurrentFaction;

    void Start()
    {
        if(CurrentFaction != null)
            PropagandaPanel.Instance.CreateButtons(CurrentFaction);
    }
}
