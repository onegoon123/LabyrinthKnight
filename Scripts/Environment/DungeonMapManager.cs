using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonMapManager : MonoBehaviour
{
    [Header("타일맵 참조")]
    public Tilemap floorTilemap;
    
    [Header("맵 설정")]
    public Vector2Int mapSize = new Vector2Int(30, 30); // 맵 크기
    
    public void GenerateMap(StageThemeData theme)
    {
        if (floorTilemap == null)
        {
            Debug.LogError("[DungeonMapManager] Floor Tilemap is not assigned!");
            return;
        }
        
        // 기존 타일 초기화
        floorTilemap.ClearAllTiles();
        
        if (theme == null || theme.floorTile == null)
        {
            Debug.LogWarning("[DungeonMapManager] Theme or Floor Tile is missing.");
            return;
        }
        
        // 맵 생성 (중심을 0,0으로)
        int startX = -mapSize.x / 2;
        int startY = -mapSize.y / 2;
        int endX = startX + mapSize.x;
        int endY = startY + mapSize.y;
        
        // 바닥 타일 배치 (BoxFill 대신 반복문 사용으로 좌표 정확도 보장)
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                floorTilemap.SetTile(new Vector3Int(x, y, 0), theme.floorTile);
            }
        }
    }
}
