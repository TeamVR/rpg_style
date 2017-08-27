using UnityEngine;
using UnityEditor;

public class AfterLightmapping : ScriptableObject {

	[MenuItem ("Component/LostLand/After Lightmapping")]
	static void AfterLightmappingScript() 
	{
		//Debug.Log("[AfterLightmappingScript] Start");			
		
		GameObject[] allObjects = (GameObject[])Editor.FindObjectsOfType(typeof(GameObject));
		foreach(GameObject obj in allObjects)
		{
			// First assign lightmaps to medium and low terrain zones
			if( (Renderer)obj.GetComponent(typeof(Renderer)) != null ) 
			{
				if( obj.name.Contains("High") )
				{
					// Get High mesh lightmap index
					int i = obj.GetComponent<Renderer>().lightmapIndex;
					// find Medium and low meshes
					char[] delim = { ' ' };
					string[] objname = obj.name.Split( delim );
					//Debug.Log ("Find what?: " + "Low " + objname[1]);
					
					GameObject lowmesh = GameObject.Find( "Low " + objname[1] );
					lowmesh.GetComponent<Renderer>().lightmapIndex = obj.GetComponent<Renderer>().lightmapIndex;
					lowmesh.GetComponent<Renderer>().lightmapScaleOffset = obj.GetComponent<Renderer>().lightmapScaleOffset;
					
					GameObject mediummesh = GameObject.Find( "Medium " + objname[1] );
					mediummesh.GetComponent<Renderer>().lightmapIndex = obj.GetComponent<Renderer>().lightmapIndex;
					mediummesh.GetComponent<Renderer>().lightmapScaleOffset = obj.GetComponent<Renderer>().lightmapScaleOffset;				
				}
				// Clear Cast Shadows and Receive Shadows flags on all foliage layer
				if( obj.layer == LayerMask.NameToLayer("Foliage") )
				{
					obj.GetComponent<Renderer>().castShadows = false;
					obj.GetComponent<Renderer>().receiveShadows = false;
				}
			}
			
		}
		
		Debug.Log("[AfterLightmappingScript] End");
	}
}

