using UnityEngine;
using UnityEditor;

public class SetDiffuseShader : ScriptableObject {

	[MenuItem ("Component/LostLand/Set Diffuse Shader")]
	static void SetDiffuseShaderFunction() 
	{
		//Debug.Log("[AssignNormalMaps] Start");			
		
		GameObject[] allObjects = (GameObject[])Editor.FindObjectsOfType(typeof(GameObject));
		foreach(GameObject obj in allObjects)
		{
			if( (Renderer)obj.GetComponent(typeof(Renderer)) != null )
			if( (obj.name.Contains("Low")) || (obj.name.Contains("Medium")) || (obj.name.Contains("High")) )
			{
				obj.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Mobile/Diffuse");
				char[] delim1 = { '-' };
				string[] texname1 = obj.GetComponent<Renderer>().sharedMaterial.name.Split( delim1 );
				string tex_fullname = texname1[0] + "-_baked.png";
				//Debug.Log( "Baked: " + normal_fullname );
				
				// Set the normal map.
				Texture main_tex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/LostLand/TerrainMesh/Textures/"+tex_fullname, typeof(Texture));
				
				if(main_tex)
				{
					obj.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", main_tex);
				}
			}
		}
		
		Debug.Log("[SetDiffuseShaderFunction] End");
	}
}

