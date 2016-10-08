using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor {
	public enum ToolType{
		brush, bucket, eraser
	}

	TileMap map;
	TileBrush brush;
	ToolType type = ToolType.brush;
	Vector2 scrollPosition = Vector2.zero;

    public override void OnInspectorGUI() {
        EditorGUILayout.BeginVertical();

		EditorGUI.BeginChangeCheck();
        Vector2 newSize = EditorGUILayout.Vector2Field("Grid Size:", map.size);
		if(EditorGUI.EndChangeCheck() && newSize.x >= 1 && newSize.y >= 1) {
			Undo.RecordObject(map, "Tile Map size changed");
			EditorUtility.SetDirty(map);
//			 Desabilitado por questão de problemas...
//            if(newSize.x < map.size.x || newSize.y < map.size.y) CleanOutOfBoundsTiles(newSize);

            map.size.x = Mathf.Floor(newSize.x);
            map.size.y = Mathf.Floor(newSize.y);
        }


		EditorGUI.BeginChangeCheck();
        newSize = EditorGUILayout.Vector2Field("Tile Size:", map.tileSize);

		if(EditorGUI.EndChangeCheck() && newSize.x > 0 && newSize.y > 0) {
			Undo.RecordObject(map, "Tile size changed");
			EditorUtility.SetDirty(map);

            map.tileSize = newSize;
        }

		EditorGUILayout.BeginHorizontal();
		if(type == ToolType.brush) GUI.color = Color.cyan;
		if(GUILayout.Button("Brush")){
			type = ToolType.brush;
		}
		GUI.color = Color.white;

		if(type == ToolType.bucket) GUI.color = Color.cyan;
		if(GUILayout.Button("Bucket")){
			type = ToolType.bucket;
		}
		GUI.color = Color.white;

		if(type == ToolType.eraser) GUI.color = Color.cyan;
		if(GUILayout.Button("Eraser")){
			type = ToolType.eraser;
		}
		GUI.color = Color.white;

		EditorGUILayout.EndHorizontal();

		if(GUILayout.Button("Fill Tile Map")){
			Undo.RecordObject(map, "Tile Map Filled");
			EditorUtility.SetDirty(map);
			ClearMap();
			FillMap();
		}

		if(GUILayout.Button("Clear out of bounds tiles")){
			Undo.RecordObject(map, "Out of bounds tiles cleared");
			EditorUtility.SetDirty(map);
			CleanOutOfBoundsTiles(map.size);
		}

		if(GUILayout.Button("Clear Map")) {
			Undo.RecordObject(map, "Tile Map cleared");
			EditorUtility.SetDirty(map);

			if(EditorUtility.DisplayDialog("This action will clear all tiles in Tile Map.", "Are you sure?", "YES", "NO")) {
				ClearMap();
			}
		}

        //EditorGUI.BeginChangeCheck();
        //int newIntSize = EditorGUILayout.IntField("Prefab Quantity", map.prefabs.Length);
        //if(EditorGUI.EndChangeCheck() && newIntSize > 0) {
        //    Undo.RecordObject(map, "Prefab Quatity changed");
        //    EditorUtility.SetDirty(map);

        //    Array.Resize(ref map.prefabs, newIntSize);
        //}

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for(var i = 0; i < map.prefabs.Length; i++) {
            EditorGUI.BeginChangeCheck();
            GameObject changed = (GameObject)EditorGUILayout.ObjectField("Prefab " + i, map.prefabs[i], typeof(GameObject), false);
            if(EditorGUI.EndChangeCheck()) {
                map.prefabs[i] = changed;
                if(map.prefabs[i] == null && i != map.prefabs.Length - 1) {
                    for(int j = i; j < map.prefabs.Length - 1; j++) {
                        map.prefabs[j] = map.prefabs[j + 1];
                    }
                    Array.Resize(ref map.prefabs, map.prefabs.Length - 1);
                    i--;
                }
                else if(map.prefabs[i] != null) {
                    if(i == map.prefabs.Length - 1) {
                        Array.Resize(ref map.prefabs, map.prefabs.Length + 1);
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.HelpBox("Add a Game Object into \"None\" to create a new field.\n" +
            "Set a GameObject to \"None\" to remove a field.", MessageType.Info);
        if(map.prefabs.Length == 1 && map.prefabs[0] == null) {
            EditorGUILayout.HelpBox("Missing GameObject Reference!", MessageType.Error);
        }

		EditorGUILayout.EndVertical();

	}

	void OnSceneGUI(){
        if(ValidatePrefabList())    UpdateBrush();
	}

	void OnEnable(){
		map = target as TileMap;
        if(ValidatePrefabList())    CreateBrush();
	}

	void OnDisable(){
        DestroyBrush();
	}

	//BRUSH
	public void CreateBrush(){
		if(brush == null) {
			
			var oldBrush = map.transform.Find(TileBrush.brushGOName);
			while(oldBrush != null) {
				DestroyImmediate(oldBrush.gameObject);
				oldBrush = map.transform.Find(TileBrush.brushGOName);
			}

			brush = new GameObject(TileBrush.brushGOName).AddComponent<TileBrush>();
			var comp = brush.gameObject.AddComponent<SpriteRenderer>();

			brush.gameObject.transform.SetParent(map.transform);
			brush.gameObject.tag = TileBrush.brushGOTag;
			brush.size = map.tileSize;
			brush.ChangeBrush(map.prefabs[0]);
			map.brush = brush;

			comp.sortingLayerName  = "Selected";
	
		}
	}

	public void UpdateBrush(){
        if(ValidatePrefabList()) {
            CreateBrush();

            Vector2 mPos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mPos);
            Vector2 pos = map.transform.InverseTransformPoint(ray.origin);

            Vector2 tileIndex = map.TileIndexForPos(pos);

            if(tileIndex.x != -1) {
                brush.gameObject.SetActive(true);
                brush.transform.localPosition = map.TilePosForIndex(tileIndex);

                ProcessEvent(tileIndex);
            }
            else {
                brush.gameObject.SetActive(false);
            }
        }
	}

	public void DestroyBrush(){
		if(brush != null) {
            var go = brush.gameObject;
			brush = null;
            DestroyImmediate(go);
		}
	}

	// Events
	void ProcessEvent(Vector2 tileIndex){
		if(Event.current.shift) {
			GameObject go = GameObject.Find("tile_" + tileIndex.x + "," + tileIndex.y);
			switch(type) {
			    case ToolType.brush:
				    if(go != null) {
					    DestroyImmediate(go);
				    }

                    brush.CreateSelectedCopy(tileIndex);

				    break;
			    case ToolType.bucket:
				    BucketFill(tileIndex);
				    break;
			    case ToolType.eraser:
				    DestroyImmediate(go);
				    break;
			}
		}
//		else if(Event.current.Equals(Event.KeyboardEvent("#f")) || Event.current.Equals(Event.KeyboardEvent("#F"))) {
//			ClearMap();
//			FillMap();
//		}
		else if(Event.current.alt) {
			GameObject go = GameObject.Find("tile_" + tileIndex.x + "," + tileIndex.y);
            DestroyImmediate(go);
		}
	}

    // Others
    bool ValidatePrefabList() {
        return map.prefabs.Length > 1 && map.prefabs[0] != null;
    }

    /**
     *  Método para remover as tiles que estão fora do tilemap,
     *  depois que ele sofreu uma mudança de tamanho
     */
    void CleanOutOfBoundsTiles(Vector2 newSize) {
        for(var i = 0; i < map.transform.childCount; i++) {
            var child = map.transform.GetChild(i);
			if(child.gameObject.name != TileBrush.brushGOName) {
                Vector2 tileIndex = map.TileIndexForPos(child.localPosition);
                if(tileIndex.x > newSize.x - 1 || tileIndex.y > newSize.y - 1) {
                    DestroyImmediate(child.gameObject);
                    i--;
                }
            }
        }
    }

	void ClearMap(){
		CleanOutOfBoundsTiles(Vector2.zero);
	}

	void FillMap(){
		var size = map.size;
		var tsize = map.tileSize;

		for(var i = 0; i < map.size.x * map.size.y; i++) {
			int x = (int) (i % size.x);
			int y = (int) Mathf.Floor(i / size.x);

            brush.CreateSelectedCopy(new Vector2(x,y), new Vector3(x * tsize.x + tsize.x / 2, y * tsize.y + tsize.y / 2));
		}
	}

	void BucketFill(Vector2 tileIndex){
		GameObject go = GameObject.Find("tile_" + tileIndex.x + "," + tileIndex.y);
		GameObject selectedPrefab = brush.GetComponent<TileBrush>().GetSelectedPrefab();
		GameObject targetGO = null;
		GameObject prefab = null;
		GameObject targetPrefab = null;
		if(go != null){
			prefab = (GameObject) PrefabUtility.GetPrefabParent(go);
		}

		if(prefab != null && prefab.Equals(selectedPrefab)) return;

		Queue queue = new Queue();

		var size = map.size;
		var tsize = map.tileSize;

		queue.Enqueue(tileIndex);

		Vector2 index;
		while(queue.Count > 0) {
			
			index = (Vector2) queue.Dequeue();
			go = GameObject.Find("tile_" + index.x + "," + index.y);
			DestroyImmediate(go);

            brush.CreateSelectedCopy(index, new Vector3(index.x * tsize.x + tsize.x / 2, index.y * tsize.y + tsize.y / 2));

			Vector2[] nextIndexes = new Vector2[]{
				new Vector2(index.x - 1, index.y),
				new Vector2(index.x + 1, index.y),
				new Vector2(index.x, index.y - 1),
				new Vector2(index.x, index.y + 1)
			};

			foreach(Vector2 nextIndex in nextIndexes) {
				if(nextIndex.x >= 0 && nextIndex.x < size.x && nextIndex.y >= 0 && nextIndex.y < size.y) {
					targetGO = GameObject.Find("tile_" + nextIndex.x + "," + nextIndex.y);

					if(targetGO == null && prefab == null) {
						queue.Enqueue(new Vector2(nextIndex.x, nextIndex.y));
					}
					else {
						targetPrefab = (GameObject) PrefabUtility.GetPrefabParent(targetGO);
						if(targetPrefab != null && prefab != null && prefab.Equals(targetPrefab)) {
							queue.Enqueue(new Vector2(nextIndex.x, nextIndex.y));
						}
					}
				}
			}
		}
	}
}
