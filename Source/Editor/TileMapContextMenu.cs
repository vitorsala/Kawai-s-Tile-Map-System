using UnityEngine;
using UnityEditor;
using System.Collections;

public class TileMapContextMenu {

	[MenuItem ("GameObject/Tile Map", false, 0)]
	static void CreateTileMap(){
		GameObject ob = new GameObject("Tile Map");
		ob.transform.position = Vector3.zero;
		ob.AddComponent<TileMap>();
	}
}
