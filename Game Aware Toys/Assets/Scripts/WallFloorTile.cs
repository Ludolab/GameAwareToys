using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

/// <summary>
/// Code adapted from: https://docs.unity3d.com/2020.2/Documentation/Manual/Tilemap-ScriptableTiles-Example.html
/// </summary>
public class WallFloorTile : Tile {

    [Tooltip("A list of 4 sprites that can be used to fill the space. The indicies should corespond to:" +
        "0=Open Floor" +
        "1=Straight Wall East to West on the North Edge" +
        "2=Outer Corner from South to East" +
        "3=Inner Corder from West to North")]
    public Sprite[] m_Sprites;

    public override void RefreshTile(Vector3Int location, ITilemap tilemap) {
        for (int yd = -1; yd <= 1; yd++) {
            for (int xd = -1; xd <= 1; xd++) {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasWallFloorTile(tilemap, position)) {
                    tilemap.RefreshTile(position);
                }
            }
        }
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) {

        int mask = HasWallFloorTile(tilemap, location + new Vector3Int(-11, 1, 0)) ? 1 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 2 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(1, 1, 0)) ? 4 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 16 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 64 : 0;
            mask += HasWallFloorTile(tilemap, location + new Vector3Int(1, -1, 0)) ? 128 : 0;
        
        int index = GetSpriteIndex((byte)mask);
        if (index >= 0 && index < m_Sprites.Length) {
            tileData.sprite = m_Sprites[index];
            tileData.color = Color.white;
            var m = tileData.transform;
            m.SetTRS(Vector3.zero, GetRotation((byte)mask), Vector3.one);
            tileData.transform = m;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = ColliderType.Sprite;
        }
        else {
            Debug.LogWarning("Not enough sprites in WallAndFloorTile instance");
        }
    }

    /*
     * Assuming a bit mask that corresponds to this layout:
     * | 1 | 2 | 4 |
     * | 8 | X | 16|
     * | 32| 64|128|
     */ 

    private int GetSpriteIndex(byte mask) {
        switch (mask) {
            case 248:
            case 249:
            case 252:
            case 253:

            case 107:
            case 111:
            case 235:
            case 239:

            case 31:
            case 63:
            case 159:
            case 191:

            case 214:
            case 215:
            case 246:
            case 247:
                return 1;

            case 208:
            case 22:
            case 11:
            case 102:
                return 2;

            case 254:
            case 223:
            case 127:
            case 251:
                return 3;

            //When in doubt return the open floor
            default:
                return 0;
        }
    }

    private Quaternion GetRotation(byte mask) {
        switch (mask) {
            case 214:
            case 215:
            case 246:
            case 247:

            case 22:
            case 223:
                return Quaternion.Euler(0, 0, 90);

            case 31:
            case 63:
            case 159:
            case 191:

            case 11:
            case 127:
                return Quaternion.Euler(0, 0, 180);

            case 107:
            case 111:
            case 235:
            case 239:

            case 102:
            case 251:
                return Quaternion.Euler(0, 0, 270);

            default:
                return Quaternion.identity;
        }
    }

    private bool HasWallFloorTile(ITilemap tilemap, Vector3Int position) {
        return tilemap.GetTile(position) == this;
    }


#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/WallAndFloorTile")]
    public static void CreateRoadTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save Wall Floor Tile", "New Wall Floor Tile", "Asset", "Save Wall Floor Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WallFloorTile>(), path);
    }
#endif
}
