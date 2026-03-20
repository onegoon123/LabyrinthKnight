using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stage Theme", menuName = "Game/Stage Theme Data")]
public class StageThemeData : ScriptableObject
{
    [Header("테마 설정")]
    public string themeName;
    
    [Header("타일맵 설정")]
    public TileBase floorTile;
    
    [Header("등장 적 목록")]
    [Tooltip("이 테마에서 등장할 적들의 템플릿 데이터")]
    public List<EnemyData> enemyTemplates;
}
