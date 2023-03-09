using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class MazeTile : Tile {

    [System.Serializable]
    public struct WeightedSprite {
        public Sprite sprite;
        public float weight;
    }
    
    public enum MazeState {
        Safe,
        Danger,
        Unknown
    }

    public MazeState mazeState;
    public bool exposed;

    public WeightedSprite[] safeSprites;
    public WeightedSprite[] unknownSprites;
    public WeightedSprite[] dangerSprites;

    private Sprite safeSprite;
    private Sprite dangerSprite;
    private Sprite unknownSprite;

    private Sprite SetSprites(WeightedSprite[] spriteSet) {
        float total = 0;
        foreach(WeightedSprite ws in spriteSet) {
            total += ws.weight;
        }
        float check = Random.Range(0, total);
        total = 0;
        for (int i = 0; i < total; i++) {
            total += spriteSet[i].weight;
            if(total > check) {
                return spriteSet[i].sprite;
            }
        }
        return spriteSet[spriteSet.Length - 1].sprite;
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap) {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData) {
        base.GetTileData(position, tilemap, ref tileData);
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/InvisibleMaze/MazeTile")]
    public static void CreateMazeTile() {
        string path = EditorUtility.SaveFilePanelInProject("Save Maze Tile", "New Maze Tile", "Asset", "Save Maze Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MazeTile>(), path);
    }

#endif
}
