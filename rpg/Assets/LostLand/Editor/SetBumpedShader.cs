using UnityEngine;
using UnityEditor;

public class SetBumpedShader : ScriptableObject {

	[MenuItem ("Component/LostLand/Set Bumped Shader")]
	static void SetBumpedShaderFunction() 
	{
		//Debug.Log("[AssignNormalMaps] Start");			
		
		GameObject[] allObjects = (GameObject[])Editor.FindObjectsOfType(typeof(GameObject));
		foreach(GameObject obj in allObjects)
		{
			if( (Renderer)obj.GetComponent(typeof(Renderer)) != null )
			if( (obj.name.Contains("Low")) || (obj.name.Contains("Medium")) || (obj.name.Contains("High")) )
			{
				obj.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Mobile/Bumped Diffuse");
				char[] delim1 = { '-' };
				string[] normal1 = obj.GetComponent<Renderer>().sharedMaterial.name.Split( delim1 );
				string normal_fullname = normal1[0] + "-_normalmap.png";
				//Debug.Log( "NormalName: " + normal_fullname );
				
				// Set the normal map.
				Texture normal_tex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/LostLand/TerrainMesh/NormalMaps/"+normal_fullname, typeof(Texture));
				
				if(normal_tex)
				{
					obj.GetComponent<Renderer>().sharedMaterial.SetTexture("_BumpMap", normal_tex);
				}
			}
		}
		
		Debug.Log("[SetBumpedShaderFunction] End");
	}
}

