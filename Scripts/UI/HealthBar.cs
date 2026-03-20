using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("HP바 설정")]
    public Slider healthSlider;
    public Vector3 offset = new Vector3(0, -0.8f, 0); // 캐릭터 밑 위치 오프셋
    public float barWidth = 1.5f; // HP바 너비
    public float barHeight = 0.2f; // HP바 높이
    
    [Header("색상 설정")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    
    private Transform targetTransform; // 추적할 캐릭터 Transform
    private Canvas canvas;
    private Camera mainCamera;
    private bool isInitialized = false;
    
    public void Initialize(Transform target, int maxHealth, int currentHealth)
    {
        targetTransform = target;
        mainCamera = Camera.main;
        
        // World Space Canvas 생성
        CreateHealthBarCanvas();
        
        // HP바 초기화
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            UpdateHealthBarColor(currentHealth, maxHealth);
        }
        
        isInitialized = true;
        healthSlider.gameObject.SetActive(false);
    }

    private void CreateHealthBarCanvas()
    {
        // Canvas가 없으면 생성
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HealthBarCanvas");
            canvasObj.transform.SetParent(transform);
            canvasObj.transform.localPosition = offset;
            
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Canvas 크기 설정
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 1f); // World Space 크기 조절
        }
        
        // HP바 슬라이더가 없으면 생성
        if (healthSlider == null)
        {
            CreateHealthBarSlider();
        }
    }
    
    private void CreateHealthBarSlider()
    {
        // Slider GameObject 생성
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(canvas.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        // 배경
        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundColor;
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5, 5); // 패딩
        fillAreaRect.offsetMax = new Vector2(-5, -5);
        
        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fullHealthColor;
        fillImage.type = Image.Type.Filled;
        
        // Slider 컴포넌트
        healthSlider = sliderObj.AddComponent<Slider>();
        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = fillImage;
        healthSlider.direction = Slider.Direction.LeftToRight;
        healthSlider.minValue = 0;
        healthSlider.value = 1f; // 초기값
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (!isInitialized || healthSlider == null) return;
        healthSlider.gameObject.SetActive(true);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        UpdateHealthBarColor(currentHealth, maxHealth);
    }
    
    private void UpdateHealthBarColor(int currentHealth, int maxHealth)
    {
        if (healthSlider == null || healthSlider.fillRect == null) return;
        
        Image fillImage = healthSlider.fillRect.GetComponent<Image>();
        if (fillImage == null) return;
        
        // HP 비율에 따라 색상 변경
        float healthPercentage = (float)currentHealth / maxHealth;
        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage);
    }
    
    private void LateUpdate()
    {
        if (!isInitialized || targetTransform == null || canvas == null) return;
        
        // 캐릭터를 따라가도록 위치 업데이트
        canvas.transform.position = targetTransform.position + offset;
        
        // 카메라를 항상 바라보도록 회전 (Billboard 효과)
        if (mainCamera != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    private void OnDestroy()
    {
        // Canvas도 함께 파괴
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
    }
}

