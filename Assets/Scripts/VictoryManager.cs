using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SwissArmyKnife;



///=================================================================================================================
///                                                                                                       <summary>
///  VictoryManager manages victory and defeat.              											 </summary>
///
///=================================================================================================================
public class VictoryManager : Singleton<VictoryManager> 
{
    public GameObject _defeatPanel;
    public Text _defeatText;

    public void GameOver( string message )
    {
        _defeatPanel.SetActive(true);
        _defeatText.text = message;
    }
}
