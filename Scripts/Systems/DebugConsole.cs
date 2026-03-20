using UnityEngine;
using DarkNaku.Number;

public class DebugConsole : MonoBehaviour
{
    private bool showConsole = false;
    private PlayerStats playerStats;
    private Rect windowRect = new Rect(20, 20, 250, 500);

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.backquoteKey.wasPressedThisFrame) // ` key
        {
            showConsole = !showConsole;
        }
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        windowRect = GUI.Window(999, windowRect, DrawConsoleWindow, "Debug Console");
    }

    private void DrawConsoleWindow(int windowID)
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats == null)
            {
                GUILayout.Label("PlayerStats not found!");
                GUI.DragWindow();
                return;
            }
        }

        GUILayout.BeginVertical();

        // Level Up
        GUILayout.Label($"Level: {playerStats.level}");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Level Up"))
        {
            int expNeeded = playerStats.experienceToNextLevel - playerStats.experience;
            playerStats.AddExperience(expNeeded);
        }
        if (GUILayout.Button("Heal"))
        {
            playerStats.Heal(playerStats.maxHealth);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Gold
        GUILayout.Label($"Gold: {playerStats.gold}");
        if (GUILayout.Button("Add 10,000 Gold"))
        {
            playerStats.gold += new Number(10000f);
            playerStats.NotifyStatsChanged();
        }
        if (GUILayout.Button("Add 1,000,000 Gold"))
        {
            playerStats.gold += new Number(1000000f);
            playerStats.NotifyStatsChanged();
        }

        GUILayout.Space(10);

        // Time Scale
        GUILayout.Label($"Time Scale: {Time.timeScale:F1}x");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("1x")) Time.timeScale = 1f;
        if (GUILayout.Button("2x")) Time.timeScale = 2f;
        if (GUILayout.Button("4x")) Time.timeScale = 4f;
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Kill All Enemies
        if (GUILayout.Button("Kill All Enemies"))
        {
            DungeonSystem dungeon = FindFirstObjectByType<DungeonSystem>();
            if (dungeon != null)
            {
                dungeon.CleanupAllEnemies();
            }
        }
        
        // Next Stage
        if (GUILayout.Button("Next Stage"))
        {
            DungeonSystem dungeon = FindFirstObjectByType<DungeonSystem>();
            if (dungeon != null)
            {
                dungeon.CleanupAllEnemies();
                dungeon.currentStage++;
                dungeon.StartDungeon(); // Restart with next stage
            }
        }
        
        GUILayout.Space(10);

        // Toggle UI
        if (GUILayout.Button("Toggle UI"))
        {
            GameUI gameUI = FindFirstObjectByType<GameUI>();
            if (gameUI != null)
            {
                gameUI.gameObject.SetActive(!gameUI.gameObject.activeSelf);
            }
        }

        GUILayout.Space(10);
        
        // Companion
        GUILayout.Label("Companion");
        if (GUILayout.Button("Max Affinity (Current)"))
        {
            CompanionSystem companionSystem = FindFirstObjectByType<CompanionSystem>();
            if (companionSystem != null)
            {
                string currentId = companionSystem.GetCurrentCompanionId();
                if (!string.IsNullOrEmpty(currentId))
                {
                    companionSystem.SetAffinity(currentId, 100);
                }
            }
        }

        GUILayout.EndVertical();

        GUI.DragWindow();
    }
}
