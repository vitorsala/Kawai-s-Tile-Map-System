using UnityEngine;
using UnityEditor;
using System.Collections;

public class TilePickerWindow : EditorWindow {

	private TileMap map;
	private Vector2 scrollPosition = Vector2.zero;
	public int selectedID = 0;

	[MenuItem ("Window/Tile Picker")]
	static void init(){
		TilePickerWindow tp = (TilePickerWindow)EditorWindow.GetWindow(typeof(TilePickerWindow));
		tp.Show();
	}

	void OnGUI(){
		if(map == null || map.prefabs.Length == 0 || map.brush == null)
			return;

		GameObject[] prefabList = map.prefabs;
		Vector2 size = map.tileSize * 70;
		Vector2 offset = new Vector2(10, 10);


		int limX = (int) Mathf.Floor(position.width / size.x);
		int qtdY = (int) Mathf.Floor(prefabList.Length / limX);
		Rect viewPort = new Rect(0, 0, position.width, position.height);
		Rect contentSize = new Rect(0, 0, position.width, qtdY * size.y + offset.y);

		scrollPosition = GUI.BeginScrollView(viewPort, scrollPosition, contentSize);

		for(var i = 0; i < prefabList.Length && prefabList[i] != null; i++) {
			
			var x = (i % limX) * size.x + offset.x;
			var y = (i / limX) * size.y + offset.y;
			Texture2D texture = AssetPreview.GetAssetPreview(prefabList[i]);

            if(selectedID == i) GUI.color = Color.cyan;
            else GUI.color = Color.white;

			if(GUI.Button(new Rect(x, y, size.x, size.y), texture)) {
				selectedID = i;
				map.brush.ChangeBrush(map.prefabs[selectedID]);
			}
		}
		GUI.EndScrollView();
	}

	void OnSelectionChange(){
        if(Selection.activeGameObject != null &&
            Selection.activeGameObject.GetType() == typeof(GameObject) &&
            Selection.activeGameObject.GetComponent<TileMap>() != null) {

            map = Selection.activeGameObject.GetComponent<TileMap>();
        }
        else {
            map = null;
        }
		Repaint();
	}
}
