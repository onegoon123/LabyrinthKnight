using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 시 페이드 인/아웃 효과를 관리하는 싱글톤 시스템
/// </summary>
public class SceneFadeManager : MonoBehaviour
{
    [Header("페이드 설정")]
    [Tooltip("페이드 지속 시간 (초)")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("씬 시작 시 자동 페이드 인")]
    public bool autoFadeInOnStart = true;
    
    [Header("UI 참조")]
    public Image fadeImage;
    
    private static SceneFadeManager instance;
    public static SceneFadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SceneFadeManager>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        if (autoFadeInOnStart)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeInCoroutine());
        }
    }
    
    /// <summary>
    /// 페이드 아웃 (검게 변함)
    /// </summary>
    public void FadeOut(System.Action onComplete = null)
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeOutCoroutine(onComplete));
    }
    
    /// <summary>
    /// 페이드 인 (밝게 변함)
    /// </summary>
    public void FadeIn(System.Action onComplete = null)
    {
        fadeImage.gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine(onComplete));
    }
    
    /// <summary>
    /// 씬을 페이드 아웃 후 로드하고 페이드 인
    /// </summary>
    public void LoadSceneWithFade(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        fadeImage.gameObject.SetActive(true);
        if (this != null && gameObject != null)
        {
            StartCoroutine(LoadSceneWithFadeCoroutine(sceneName, mode));
        }
    }
    
    private IEnumerator FadeOutCoroutine(System.Action onComplete = null)
    {
        fadeImage.raycastTarget = true;
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
        onComplete?.Invoke();
    }
    
    private IEnumerator FadeInCoroutine(System.Action onComplete = null)
    {
        fadeImage.raycastTarget = true;
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 0);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        fadeImage.color = targetColor;
        fadeImage.raycastTarget = false;
        onComplete?.Invoke();
    }
    
    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName, LoadSceneMode mode)
    {
        // 페이드 아웃
        yield return StartCoroutine(FadeOutCoroutine());
        
        // 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 씬이 완전히 로드된 후 약간 대기
        yield return new WaitForSeconds(0.1f);
        
        // 페이드 인
        yield return StartCoroutine(FadeInCoroutine());
    }
    
    /// <summary>
    /// 페이드 이미지의 알파 값을 직접 설정 (즉시)
    /// </summary>
    public void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }
}

