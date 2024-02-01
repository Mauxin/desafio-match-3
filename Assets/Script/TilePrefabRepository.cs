using UnityEngine;

[CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
public class TilePrefabRepository : ScriptableObject
{
    public TileView[] tileTypePrefabList;
    
    public TileView CreateTile(int type)
    {
        return Instantiate(tileTypePrefabList[type]);
    }
}