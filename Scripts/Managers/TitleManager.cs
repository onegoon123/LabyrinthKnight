using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nicknamePanel;  // 닉네임 입력 패널
    public TMP_InputField nicknameInput;
    public Button startButton;        // 닉네임 확정 버튼
    public Button touchAreaButton;    // 화면 전체를 덮는 투명 버튼 (Touch to Start)

    [Header("Settings")]
    public string gameSceneName = "GameScene";   // 게임 플레이 씬 이름
    public string prologueScriptName = "Prologue"; // 프롤로그 Naninovel 스크립트 이름

    private GameSaveSystem saveSystem;
    private DialogueManager dialogueManager;

    private void Start()
    {
        saveSystem = FindFirstObjectByType<GameSaveSystem>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // 초기 상태: 타이틀 패널만 보임
        if (nicknamePanel != null) nicknamePanel.SetActive(false);
        
        if (touchAreaButton != null)
        {
            touchAreaButton.onClick.RemoveAllListeners();
            touchAreaButton.onClick.AddListener(OnTouchScreen);
        }
        
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnNicknameConfirmed);
        }
    }

    // 화면 터치 시 호출
    public void OnTouchScreen()
    {
        // 저장 데이터가 있는지 확인
        bool hasSave = saveSystem != null && saveSystem.HasSaveFile();

        if (hasSave)
        {
            // 기존 유저: 게임 씬으로 바로 이동
            Debug.Log("저장된 데이터 있음. 게임 시작.");
            LoadGameScene();
        }
        else
        {
            // 신규 유저: 닉네임 설정 화면 표시
            Debug.Log("신규 유저. 닉네임 설정 화면 표시.");
            if (nicknamePanel != null) nicknamePanel.SetActive(true);
        }
    }

    // 닉네임 입력 완료 시 호출
    public void OnNicknameConfirmed()
    {
        if (nicknameInput == null || string.IsNullOrWhiteSpace(nicknameInput.text))
        {
            Debug.LogWarning("닉네임을 입력해주세요.");
            return;
        }

        string nickname = nicknameInput.text;
        Debug.Log($"닉네임 설정: {nickname}");

        // 1. 닉네임 저장 및 파일 생성 (PlayerStats 의존성 제거)
        if (saveSystem != null)
        {
            saveSystem.CreateNewSaveFile(nickname);
        }
        
        // [치트] 닉네임이 "테스트"인 경우 테스트 모드 활성화
        if (nickname == "테스트")
        {
            PlayerPrefs.SetInt("IsTestMode", 1);
            Debug.Log("테스트 모드가 활성화되었습니다 (PlayerPrefs).");
        }
        else
        {
            PlayerPrefs.SetInt("IsTestMode", 0);
        }
        PlayerPrefs.Save();
        
        // 2. 프롤로그 재생
        if (dialogueManager != null)
        {
            // DialogueManager가 프롤로그 씬으로 이동 후 스크립트 재생
            // 주의: 프롤로그 스크립트의 마지막에 @loadScene GameScene 명령어가 있어야 함
            dialogueManager.PlayStory(prologueScriptName, nickname);
        }
        else
        {
            // 대화 매니저가 없으면 바로 게임 시작 (예외 처리)
            LoadGameScene();
        }
    }

    private void LoadGameScene()
    {
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.LoadSceneWithFade(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
