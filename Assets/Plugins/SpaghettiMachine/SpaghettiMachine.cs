using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace Spaghetti
{
	public class SpaghettiMachine : SpaghettiMachineCore 
	{	
		//================================================================================================
		//
		//   LOADING METHODS
		//
		//================================================================================================
		
		//------------------------------------------------------------------------------------------------
		// LoadFromResources
		//------------------------------------------------------------------------------------------------
		public void LoadFromResources( string strFileName ) 
		{		
			// Cut file extension ."bytes" if any 
			if( strFileName.Length >= 6 && strFileName.Substring( strFileName.Length - 6 ) == ".bytes" )
			{
				strFileName = strFileName.Substring( 0, strFileName.Length - 6 );
			}
			
			TextAsset textAsset= Resources.Load( strFileName  ) as TextAsset;
					
			if( textAsset == null )
			{
				Debug.LogWarning( "File not found: "+strFileName+".bytes. Did you put it into a \"Resources\" folder?" );
				return;
			}			
			
			Debug.Log("First character is \""+((int)(textAsset.text[0]))+"\", category = "+Char.GetUnicodeCategory(textAsset.text[0]) );
			LoadFromString( textAsset.text ); 
		}		
		
		//------------------------------------------------------------------------------------------------
		// LoadFromFile
		//------------------------------------------------------------------------------------------------
		public void LoadFromFile( string strLocalFilePath ) 
		{
			#if UNITY_WEBPLAYER 

				Debug.Log("LoadFromFile: Local files can't be loaded in the web player. Switch to another platform or call another loading method, e.g. LoadFromResources.");
			
			#elif UNITY_FLASH

				Debug.Log("LoadFromFile: Local files can't be loaded in Flash. Switch to another platform or call another loading method, e.g. LoadFromResources.");
			
			#else			
			
				// Append file extension ."bytes" if not there 
				if( strLocalFilePath.Length < 6 || strLocalFilePath.Substring( strLocalFilePath.Length - 6 ) != ".bytes" )
				{
					strLocalFilePath = strLocalFilePath + ".bytes";
				}
				
				if( ! File.Exists( Application.dataPath + "/" + strLocalFilePath ) )
				{
					Debug.LogWarning( "File not found: "+Application.dataPath + "/" + strLocalFilePath  );
					return;
				}			
				
				string strContent = File.ReadAllText( Application.dataPath + "/" + strLocalFilePath );
				LoadFromString( strContent );
			
			#endif
		}
		
		//------------------------------------------------------------------------------------------------
		// LoadFromURL
		//------------------------------------------------------------------------------------------------
		public void LoadFromURL( string strURL ) 
		{	
			StartCoroutine( CoroutineLoadFromURL( strURL ) );
		}		
		
		//------------------------------------------------------------------------------------------------
		// CoroutineLoadFromURL
		//------------------------------------------------------------------------------------------------
		private IEnumerator CoroutineLoadFromURL( string strURL ) 
		{			
			WWW	www  = new WWW( strURL );

			yield return www;			

			if( www.error != null)
			{
				Debug.LogError( "Error while downloading file "+strURL+": "+www.error );
				yield return null;
			}
			
			mGraph = new Graph( this );
			LoadFromString( www.text );	
		}
		
		//------------------------------------------------------------------------------------------------
		// LoadFromTextAsset
		//------------------------------------------------------------------------------------------------
		public void LoadFromTextAsset( TextAsset asset ) 
		{
			LoadFromString( asset.text );
		}		
		
	
		//------------------------------------------------------------------------------------------------
		// LoadFromString
		//------------------------------------------------------------------------------------------------
		public void LoadFromString( string strContent ) 	
		{				
			maLinks = new ArrayList();				
			mGraph = new Graph( this );
			mGraph.ReadFromString( strContent );		
			maPanels = mGraph.GetPanels();
		
			mGraph.PostReadInitialization();
			
			gameObject.SendMessage( "OnGraphLoaded", SendMessageOptions.DontRequireReceiver );
		}		
	}
}
	
