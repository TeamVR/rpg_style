using UnityEngine;
using UnityEditor;

public class PrepareLightmapping : ScriptableObject {

	[MenuItem ("Component/LostLand/Prepare to Lightmapping")]
	static void PrepareLightmappingScript() 
	{
		//Debug.Log("[AfterLightmappingScript] Start");			
		
		GameObject[] allObjects = (GameObject[])Editor.FindObjectsOfType(typeof(GameObject));
		foreach(GameObject obj in allObjects)
		{
			// First assign lightmaps to medium and low terrain zones
			if( (Renderer)obj.GetComponent(typeof(Renderer)) != null ) 
			{
				// Set Cast Shadows and Receive Shadows flags on all foliage layer
				if( obj.layer == LayerMask.NameToLayer("Foliage") )
				{
					obj.GetComponent<Renderer>().castShadows = true;
					obj.GetComponent<Renderer>().receiveShadows = true;
				}
			}
			
		}
		
		Debug.Log("[PrepareLightmappingScript] End");
	}
}

