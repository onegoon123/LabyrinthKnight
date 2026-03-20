using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    [Header("적 정보")]
    public EnemyData enemyData; // ScriptableObject 참조
    
    [Header("런타임 상태")]
    public int currentHealth;
    public bool isAlive = true;
    
    [Header("전투 관련")]
    public float attackCooldown = 0f;
    
    [Header("성능 최적화")]
    public float distanceCheckInterval = 0.1f; // 거리 체크 간격
    private float lastDistanceCheckTime = 0f;
    private bool isPlayerInRange = false; // 공격 범위 내
    private bool isPlayerDetected = false; // 탐지 범위 내
    private float cachedDistanceToPlayer = float.MaxValue;
    
    [Header("안전장치")]
    private int frameCount = 0;
    private const int MAX_FRAMES_PER_UPDATE = 3; // 한 번에 최대 처리할 프레임 수
    
    private PlayerStats playerStats;
    private DungeonSystem dungeonSystem;
    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    public HealthBar healthBar;
    
    private Color originalColor;
    private Collider2D col2d;

    // 타겟 시스템 (플레이어와 동료 모두 타겟 가능)
    private ICombatTarget currentTarget;
    private ICombatTarget[] cachedTargets = new ICombatTarget[0];
    private float lastTargetCacheTime = 0f;
    private float targetCacheInterval = 0.2f;
    
    public event System.Action<Enemy> OnEnemyDefeated;
    
    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2d = GetComponent<Collider2D>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Rigidbody2D가 없으면 추가
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0f; // 2D 게임에서 중력 비활성화
            rb2d.freezeRotation = true; // 회전 고정
        }
        
        // HP바 초기화 (InitializeEnemy 이후에 호출됨)
    }
    
    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
    }

    private void InitializeHealthBar()
    {
        // HealthBar 컴포넌트가 없으면 추가
        if (healthBar == null)
        {
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(transform);
            healthBar = healthBarObj.AddComponent<HealthBar>();
        }
        
        // HP바 초기화
        if (healthBar != null && enemyData != null)
        {
            healthBar.Initialize(transform, enemyData.GetMaxHealth(enemyData.level), currentHealth);
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBar != null && enemyData != null)
        {
            healthBar.UpdateHealth(currentHealth, enemyData.GetMaxHealth(enemyData.level));
        }
    }
    
    private void Update()
    {
        if (!isAlive || !playerStats.IsAlive()) return;
        
        // 안전장치: 프레임 카운터로 무한 루프 방지
        frameCount++;
        if (frameCount > MAX_FRAMES_PER_UPDATE)
        {
            frameCount = 0;
            return;
        }
        
        // 주기적으로 거리 체크 (성능 최적화)
        if (Time.time - lastDistanceCheckTime >= distanceCheckInterval)
        {
            // 타겟 유효성 검사
            if (IsValidTarget(currentTarget))
            {
                cachedDistanceToPlayer = Vector2.Distance(transform.position, currentTarget.GetTransform().position);
                isPlayerDetected = cachedDistanceToPlayer <= enemyData.detectionRange;
                isPlayerInRange = cachedDistanceToPlayer <= enemyData.attackRange;
            }
            else
            {
                // 타겟이 없거나 죽었으면 새 타겟 찾기
                FindNearestTarget();
            }
            lastDistanceCheckTime = Time.time;
        }
        
        // 타겟을 바라보도록 스프라이트 방향 설정
        if (IsValidTarget(currentTarget))
        {
            Vector2 direction = (currentTarget.GetTransform().position - transform.position).normalized;
            if (spriteRenderer != null)
            {
                if (direction.x > 0)
                {
                    spriteRenderer.flipX = true; // 오른쪽
                }
                else if (direction.x < 0)
                {
                    spriteRenderer.flipX = false; // 왼쪽
                }
            }
        }
        
        if (isPlayerDetected)
        {
            if (isPlayerInRange)
            {
                // 공격 범위 내에 있으면 공격
                StopMoving();
                
                attackCooldown -= Time.deltaTime;
                
                if (attackCooldown <= 0f)
                {
                    AttackTarget();
                    attackCooldown = 1f / enemyData.GetAttackSpeed(enemyData.level);
                }
            }
            else
            {
                // 탐지 범위 내에 있지만 공격 범위 밖이면 타겟을 쫓아감
                ChaseTarget();
            }
        }
        else
        {
            // 탐지 범위 밖이면 정지
            StopMoving();
        }
        
        frameCount = 0; // 정상 완료 시 리셋
    }
    
    private void ChaseTarget()
    {
        if (!IsValidTarget(currentTarget) || rb2d == null) return;
        
        // 타겟 방향으로 이동
        Vector2 direction = (currentTarget.GetTransform().position - transform.position).normalized;
        rb2d.linearVelocity = direction * enemyData.moveSpeed;
    }
    
    private void StopMoving()
    {
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
        }
    }
    
    private void AttackTarget()
    {
        if (!IsValidTarget(currentTarget)) return;
        
        currentTarget.TakeDamage(enemyData.GetAttackPower(enemyData.level));
    }
    
    /// <summary>
    /// 타겟이 유효한지 확인 (null, GameObject 파괴, IsAlive 체크)
    /// </summary>
    private bool IsValidTarget(ICombatTarget target)
    {
        // Unity Object로서의 null 체크 (파괴된 객체 확인용)
        if (target == null || (target as UnityEngine.Object) == null) return false;
        
        try
        {
            // Transform이 null이면 GameObject가 파괴된 것
            Transform targetTransform = target.GetTransform();
            if (targetTransform == null) return false;
            
            // GameObject가 비활성화되었는지 확인
            if (!targetTransform.gameObject.activeInHierarchy) return false;
            
            // IsAlive 확인
            if (!target.IsAlive()) return false;
            
            return true;
        }
        catch (MissingReferenceException)
        {
            // 객체가 파괴되었는데 접근하려 할 때 발생
            return false;
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        int actualDamage = Mathf.Max(1, damage - enemyData.GetDefense(enemyData.level));
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        // HP바 업데이트
        UpdateHealthBar();
        
        if (currentHealth <= 0)
        {
            Defeat();
        }
        else
        {
            // 피격 효과 (죽지 않았을 때만)
            if (spriteRenderer != null)
            {
                spriteRenderer.DOKill();
                spriteRenderer.color = originalColor;
                spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
        }
    }
    
    private void Defeat()
    {
        isAlive = false;
        StopMoving();
        
        if (col2d != null) col2d.enabled = false;

        // 경험치와 골드 보상
        if (playerStats != null)
        {
            playerStats.AddExperience(enemyData.experienceReward);
            playerStats.gold += enemyData.goldReward;
        }

        // 사망 애니메이션
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            Sequence deathSequence = DOTween.Sequence();
            deathSequence.Append(spriteRenderer.DOColor(Color.red, 0.2f));
            deathSequence.Append(spriteRenderer.DOFade(0f, 0.5f));
            deathSequence.OnComplete(FinalizeDeath);
        }
        else
        {
            FinalizeDeath();
        }
    }
    
    private void FinalizeDeath()
    {
        // 풀링을 위해 비활성화 (Destroy 대신)
        // EnemySpawner2D에서 풀로 반환됨
        gameObject.SetActive(false);
        OnEnemyDefeated?.Invoke(this);
    }

    public void InitializeEnemy(EnemyData data)
    {
        // 필수 참조 재확인 (풀링된 객체의 경우 Start가 다시 호출되지 않으므로)
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
        if (dungeonSystem == null) dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        if (col2d == null) col2d = GetComponent<Collider2D>();
        
        // 데이터 설정
        enemyData = data;
        
        // 풀링을 위한 상태 초기화
        isAlive = true;
        if (col2d != null) col2d.enabled = true;
        
        currentTarget = null;
        lastDistanceCheckTime = -1f; // 즉시 체크하도록 설정
        isPlayerDetected = false;
        isPlayerInRange = false;
        cachedDistanceToPlayer = float.MaxValue;
        
        // 런타임 스탯 초기화
        if (enemyData != null)
        {
            currentHealth = enemyData.GetMaxHealth(enemyData.level);
            
            // 스프라이트 변경
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (enemyData.sprite != null)
                {
                    spriteRenderer.sprite = enemyData.sprite;
                }
                // 색상 초기화 및 저장
                spriteRenderer.DOKill();
                spriteRenderer.color = Color.white; // 기본값으로 리셋
                originalColor = spriteRenderer.color;
            }
        }
        
        // 이동 정지
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
        }
        
        // HP바 초기화
        InitializeHealthBar();
    }
    
    /// <summary>
    /// 가장 가까운 전투 대상(플레이어 또는 동료)을 찾습니다.
    /// </summary>
    private void FindNearestTarget()
    {
        UpdateTargetCache();
        
        ICombatTarget nearestTarget = null;
        float nearestDistance = float.MaxValue;
        
        foreach (ICombatTarget target in cachedTargets)
        {
            if (!IsValidTarget(target)) continue;
            
            float distance = Vector2.Distance(transform.position, target.GetTransform().position);
            
            if (distance < nearestDistance)
            {
                nearestTarget = target;
                nearestDistance = distance;
            }
        }
        
        currentTarget = nearestTarget;
        
        if (currentTarget != null)
        {
            cachedDistanceToPlayer = nearestDistance;
            isPlayerDetected = cachedDistanceToPlayer <= enemyData.detectionRange;
            isPlayerInRange = cachedDistanceToPlayer <= enemyData.attackRange;
        }
        else
        {
            isPlayerDetected = false;
            isPlayerInRange = false;
        }
    }
    
    /// <summary>
    /// 타겟 캐시를 업데이트합니다 (플레이어와 모든 동료).
    /// </summary>
    private void UpdateTargetCache()
    {
        if (Time.time - lastTargetCacheTime < targetCacheInterval) return;
        
        lastTargetCacheTime = Time.time;
        
        // 플레이어와 동료들을 모두 찾기
        var targets = new System.Collections.Generic.List<ICombatTarget>();
        
        // 플레이어 추가
        if (playerStats != null && playerStats is ICombatTarget)
        {
            targets.Add(playerStats as ICombatTarget);
        }
        
        // 모든 동료 추가
        CompanionController[] companions = FindObjectsByType<CompanionController>(FindObjectsSortMode.None);
        foreach (var companion in companions)
        {
            if (companion != null && companion.isAlive)
            {
                targets.Add(companion);
            }
        }
        
        cachedTargets = targets.ToArray();
    }
}

