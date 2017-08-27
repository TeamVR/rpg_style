using UnityEngine;
using UnityEditor;

public class AssignTerrainLOD : ScriptableObject {

	[MenuItem ("Component/LostLand/Assign Terrain LOD")]
	static void AssignTerrainLODScripts() 
	{
		Debug.Log("[AssignTerrainLODScripts] Start");			
		
		GameObject[] allObjects = (GameObject[])Editor.FindObjectsOfType(typeof(GameObject));
		foreach(GameObject obj in allObjects)
		{
			if( (Renderer)obj.GetComponent(typeof(Renderer)) != null )
			if( (obj.name.Contains("Low")) || (obj.name.Contains("Medium")) || (obj.name.Contains("High")) )
			{
				// Remove the script, we will reassign it in order to reset the script variables.
				TerrainLOD lod_script = obj.GetComponent(typeof(TerrainLOD)) as TerrainLOD;
				MeshCollider mc = obj.GetComponent( typeof(MeshCollider) ) as MeshCollider;
				while(lod_script)
				{
					Object.DestroyImmediate(lod_script);
					lod_script = obj.GetComponent(typeof(TerrainLOD)) as TerrainLOD;
				}
				// удалим Mesh collider
				while( mc )
				{
					Object.DestroyImmediate(mc);
					mc = obj.GetComponent( typeof(MeshCollider) ) as MeshCollider;					
				}
				// hide LOD1 and LOD2 meshes (for editor purposes)
				((MeshRenderer)obj.GetComponent(typeof(MeshRenderer))).enabled = false;
				
				if( obj.name.Contains("High") ) 
				{
					obj.AddComponent(typeof(TerrainLOD));
					Debug.Log("[AssignTerrainLODScripts] Add LOD script to " + obj.name);
					//Debug.Log ("Mesh name: " + m.name);
					obj.AddComponent(typeof(MeshCollider));
					// Enable Highest LOD mesh
					((MeshRenderer)obj.GetComponent(typeof(MeshRenderer))).enabled = true;
				}				
				
				// нормируем имена компонентов
				Renderer ren = (Renderer)obj.GetComponent(typeof(Renderer));
				Debug.Log("Mat name: " + ren.sharedMaterial.name );
				Debug.Log("Obj name: " + obj.name);
				char[] delim1 = { '(', ')' };
				char[] delim2 = { ' ' };
				string[] number = ren.sharedMaterial.name.Split( delim1 );
				string[] objname = obj.name.Split( delim2 );
				obj.name = objname[0] + ' ' + number[1];
				Debug.Log ("New Obj name: " + obj.name);
			
			}
		}
		
		Debug.Log("[AssignTerrainLODScripts] End");
	}
}

