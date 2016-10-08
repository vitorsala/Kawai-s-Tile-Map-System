using UnityEngine;
using System.Collections;

public enum TileProperties {
    Collision, Trigger, None
}

/// <summary>
/// Class designed for Custom Data for each tile. <para />
/// This is the class you must change if you want to add custom functionality.
/// </summary>
public class TileData : MonoBehaviour {

    /// <summary>
    /// Property of an tile.
    /// </summary>
    public TileProperties property = TileProperties.None;

    /// <summary>
    /// Grid position based on tile map's grid space
    /// </summary>
    [HideInInspector] public Vector2 position;

    public TileData(TileProperties property, Vector2 position) {
        this.property = property;
        this.position = position;
    }
}