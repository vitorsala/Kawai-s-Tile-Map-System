using UnityEngine;
using UnityEditor;

public class TileBrush : MonoBehaviour {
	public static string brushGOName = "TMEBrush";
	public static string brushGOTag = "EditorOnly";

	public Vector2 size = Vector2.one;

    private GameObject selected;

    public GameObject GetSelectedPrefab() {
        return selected;
    }

    public void ChangeBrush(GameObject obj) {
        if(obj == null) return;
        Texture2D prev = AssetPreview.GetAssetPreview(obj);
        if(prev != null) {
            SpriteRenderer render = gameObject.GetComponent<SpriteRenderer>();
            render.sprite = Sprite.Create(prev, new Rect(0, 0, prev.width, prev.height), new Vector2(0.5f, 0.5f));
            render.sortingOrder = 1000;
            float sx = (size.x / render.sprite.bounds.size.x);
            float sy = (size.y / render.sprite.bounds.size.y);
            transform.localScale = new Vector3(sx, sy, 1);
            selected = obj;
        }
    }

    public GameObject CreateSelectedCopy(Vector2 tileIndex, Vector3 position) {
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(selected);
        go.name = "tile_" + tileIndex.x + "," + tileIndex.y;
        go.transform.SetParent(this.transform.parent);
        go.transform.localPosition = new Vector3(position.x, position.y);

        SpriteRenderer render = go.GetComponent<SpriteRenderer>();
        float sx = (size.x / render.sprite.bounds.size.x);
        float sy = (size.y / render.sprite.bounds.size.y);
        go.transform.localScale = new Vector3(sx, sy, 1);

        TileData td = go.GetComponent<TileData>();
        if(td == null) {
            td = go.AddComponent<TileData>();
            td.property = TileProperties.None;
        }
        td.position = tileIndex;

        return go;
    }

    public GameObject CreateSelectedCopy(Vector2 tileIndex) {
        if(enabled)
            return CreateSelectedCopy(tileIndex, this.transform.localPosition);
        else
            return null;
    }
}
