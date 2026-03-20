using UnityEngine;
using Naninovel;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    [SerializeField] private bool playStartDialogue = false;
    [SerializeField] private Object naniScript;
    private async void Start()
    {
        if (playStartDialogue)
        {
            await RuntimeInitializer.Initialize();

            var vars = Engine.GetService<ICustomVariableManager>();
            // Editor 테스트 시에는 GameObject 이름 대신 임시 이름 사용 가능, 혹은 그대로 유지
            vars.SetVariableValue("PlayerName", new(name));

            var player = Engine.GetService<IScriptPlayer>();
            if (naniScript != null)
                await player.LoadAndPlay(naniScript.name);

            var inputManager = Engine.GetService<IInputManager>();
            inputManager.ProcessInput = true;
        }
    }
#endif
    private string currentCompanionId;

    public void SetCurrentCompanionId(string id)
    {
        currentCompanionId = id;
    }

    public void StartCompanionStory(string storySuffix)
    {
        if (string.IsNullOrEmpty(currentCompanionId))
        {
            Debug.LogError("No companion ID set!");
            return;
        }
        // 예: companion_default_Story1
        // 스크립트 이름 규칙에 따라 수정 가능. 여기서는 언더스코어로 연결.
        string scriptName = $"{currentCompanionId}_{storySuffix}";
        Debug.Log($"Starting companion story: {scriptName}");
        PlayStory(scriptName);
    }

    public async void PlayStory(string scriptName, string playerNameOverride = null)
    {
        var saveSystem = FindFirstObjectByType<GameSaveSystem>();
        var name = playerNameOverride;

        if (string.IsNullOrEmpty(name))
        {
            name = (saveSystem != null && saveSystem.playerStats != null) ? saveSystem.playerStats.playerName : "Player";
        }

        // 페이드 아웃 후 씬 전환
        SceneFadeManager.Instance.LoadSceneWithFade("story");
        
        // ... (나머지 동일)

        // 씬 로드 대기
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == "story");

        await RuntimeInitializer.Initialize();

        var vars = Engine.GetService<ICustomVariableManager>();
        vars.SetVariableValue("PlayerName", new(name));

        var player = Engine.GetService<IScriptPlayer>();
        await player.LoadAndPlay(scriptName);

        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;
    }
}
