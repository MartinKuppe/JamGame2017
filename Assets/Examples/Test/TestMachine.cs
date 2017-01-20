using UnityEngine;
using System.Collections;
using Spaghetti;

public class TestMachine : SpaghettiMachine 
{
	
	public TextAsset   mTestGraph;
	
	// Use this for initialization
	void Start () 
	{
		LoadFromTextAsset( mTestGraph );
	}
	
}
