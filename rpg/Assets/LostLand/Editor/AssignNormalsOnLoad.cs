using UnityEngine;
using System.Collections;
using UnityEditor;

public class ApplyNormalsOnLoad : AssetPostprocessor 
{
	
   	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
   	{
        foreach (string i in importedAssets)
        {
			// *******************************************
			// THIS SCRIPT IS DEPRECATED!!!!
			// Please, use AssignNormals.cs instead 
			// *******************************************
			return;
			if( i.Contains(".mat") && i.Contains("terrainzone") )
			{
				//Debug.Log("[ApplyTerrainShader] Is material");
				Material mat = (Material)AssetDatabase.LoadAssetAtPath(i, typeof(Material));
				if(mat)
				{
					//Debug.Log( "Assign normal map to zone:" + mat.name );
					
					//Debug.Log("[ApplyTerrainShader] Change shader");
					mat.shader = Shader.Find("Mobile/Bumped Diffuse");
					char[] delim1 = { '-' };
					string[] normal1 = mat.name.Split( delim1 );
					string normal_fullname = normal1[0] + "-_normalmap.png";
					Debug.Log( "NormalName: " + normal_fullname );
					
					// Set the normal map.
					Texture normal_tex = (Texture)AssetDatabase.LoadAssetAtPath("Assets/JurassicTerrain/TerrainMesh/NormalMaps/"+normal_fullname, typeof(Texture));
					
					if(normal_tex)
					{
						mat.SetTexture("_BumpMap", normal_tex);
						//Debug.Log("[ApplyTerrainShader] Apply detail textures");
					}
				}
			}
        }
		//Debug.Log( "Detail texture assigned" );
   	}	
}
