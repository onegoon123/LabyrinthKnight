using UnityEngine;
using DG.Tweening;

/// <summary>
/// 동료 캐릭터의 행동을 제어하는 컨트롤러
/// 플레이어와 유사한 전투 AI를 가지며, 플레이어를 따라다닙니다.
/// </summary>
public class CompanionController : MonoBehaviour, ICombatTarget
{
    [Header("동료 데이터")]
    public CompanionData companionData;
    
    [Header("현재 상태")]
    public int currentHealth;
    public int currentLevel = 1; // 현재 레벨 (플레이어 레벨에 동기화)
    public bool isAlive = true;
    
    private Transform playerTransform;
    private PlayerController playerController; // [추가] 플레이어 컨트롤러 참조
    private Enemy currentTarget;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // 애니메이터
    public HealthBar healthBar; // 체력바
    
    // 애니메이션 설정
    private const string ANIM_PARAM_IS_MOVING = "IsMoving";
    private const string ANIM_PARAM_ATTACK_TRIGGER = "AttackTrigger";
    private const string ANIM_PARAM_ATTACK_SPEED = "AttackSpeed";
    private const string ANIM_PARAM_MOVE_SPEED = "MoveSpeed";
    
    // 전투 관련
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private float currentAttackCooldown = 1f;
    private float baseAttackAnimationDuration = 0.5f; // 기본 공격 애니메이션 지속 시간
    private float currentAttackAnimationDuration = 0.5f;
    private bool isWaitingForAttackAnimation = false; // 공격 애니메이션 완료 대기 중
    
    // AI 관련
    private float lastSearchTime = 0f;
    private Enemy[] cachedEnemies = new Enemy[0];
    private float lastEnemyCacheTime = 0f;
    private float enemyCacheInterval = 0.2f;
    
    // 애니메이션 상태 추적
    private bool wasMoving = false;
    private bool isMovingFrame = false; // 현재 프레임 이동 여부
    
    private Color originalColor;

    public event System.Action<CompanionController> OnCompanionDefeated;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Animator 가져오기
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 플레이어 찾기
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerTransform = playerStats.transform;
            playerController = playerStats.GetComponent<PlayerController>(); // [추가]
        }
        
        // AI 동기화 방지를 위한 랜덤 오프셋
        lastSearchTime = Time.time + Random.Range(0f, 0.5f);
        
        // 초기화 (이미 Initialize가 호출되었을 수 있으므로 체크)
        if (companionData != null && currentHealth <= 0)
        {
            currentHealth = companionData.GetMaxHealth(currentLevel);
            currentAttackCooldown = 1f / companionData.GetAttackSpeed(currentLevel);
            
            // 체력바 초기화
            InitializeHealthBar();
        }
    }
    
    private void OnDisable()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
    }

    private void Update()
    {
        if (!isAlive || playerTransform == null) return;
        
        isMovingFrame = false; // 프레임 시작 시 초기화

        // 플레이어와의 거리 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > companionData.followDistance)
        {
            // 플레이어가 너무 멀면 추적
            FollowPlayer();
        }
        else
        {
            // 플레이어 근처에 있으면 전투
            HandleCombat();
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    private void FollowPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * companionData.followSpeed * Time.deltaTime;
        
        isMovingFrame = true;

        // 스프라이트 방향 전환
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    private void HandleCombat()
    {
        // 적 탐색
        if (currentTarget == null || !currentTarget.isAlive || !currentTarget.gameObject.activeInHierarchy)
        {
            // 타겟이 없거나 죽었거나 비활성화되면 새 타겟 찾기
            currentTarget = null;
            
            if (Time.time - lastSearchTime >= companionData.searchInterval)
            {
                FindNearestEnemy();
                lastSearchTime = Time.time;
            }
        }
        
        if (currentTarget != null && currentTarget.isAlive && currentTarget.gameObject.activeInHierarchy)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget <= companionData.attackRange)
            {
                // 공격 범위 내
                StopMoving();
                
                if (CanAttack())
                {
                    AttackTarget();
                }
            }
            else if (!isAttacking)
            {
                // 적에게 이동
                MoveToTarget();
            }
        }
        else
        {
            StopMoving();
        }
    }
    
    private void FindNearestEnemy()
    {
        UpdateEnemyCache();
        
        Enemy nearestEnemy = null;
        float nearestScore = float.MaxValue;
        
        // 플레이어의 현재 타겟 가져오기 [추가]
        Enemy playerTarget = playerController != null ? playerController.GetCurrentTarget() : null;
        
        foreach (Enemy enemy in cachedEnemies)
        {
            if (enemy == null || !enemy.isAlive) continue;
            
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            
            // 점수 계산 (낮을수록 우선)
            float score = distance;
            
            // 플레이어가 치고 있는 적은 우선순위 낮춤 (점수 증가) - 타겟 분산
            // 즉, 거리가 2.5배 먼 적으로 간주함. 주변에 다른 적이 있으면 그 쪽으로 감.
            if (enemy == playerTarget)
            {
                score *= 2.5f;
            }
            
            if (score < nearestScore)
            {
                nearestEnemy = enemy;
                nearestScore = score;
            }
        }
        
        currentTarget = nearestEnemy;
    }
    
    private void UpdateEnemyCache()
    {
        if (Time.time - lastEnemyCacheTime < enemyCacheInterval) return;
        
        lastEnemyCacheTime = Time.time;
        cachedEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }
    
    private void MoveToTarget()
    {
        if (currentTarget == null) return;
        
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += direction * companionData.moveSpeed * Time.deltaTime;
        
        isMovingFrame = true;

        // 스프라이트 방향 전환
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }
    
    private void StopMoving()
    {
        // Transform 이동 방식에서는 별도의 정지 로직이 필요 없음
    }
    
    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= currentAttackCooldown && !isAttacking && !isWaitingForAttackAnimation;
    }
    
    private void AttackTarget()
    {
        if (currentTarget == null || !CanAttack()) return;
        
        isAttacking = true;
        isWaitingForAttackAnimation = true;
        lastAttackTime = Time.time;
        
        // 공격 애니메이션 지속 시간 계산
        if (companionData != null)
        {
            currentAttackAnimationDuration = baseAttackAnimationDuration / companionData.GetAttackSpeed(currentLevel);
        }
        
        // 타겟 바라보기
        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        
        StopMoving();
        
        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger(ANIM_PARAM_ATTACK_TRIGGER);
        }
        
        // 데미지는 애니메이션 이벤트에서 처리됩니다 (OnAttackHit 메서드)
        
        // 공격 애니메이션 시간 후 공격 상태 해제
        Invoke(nameof(EndAttack), currentAttackAnimationDuration);
    }
    
    private void EndAttack()
    {
        isAttacking = false;
        isWaitingForAttackAnimation = false;
    }
    
    // 애니메이션 이벤트에서 호출되는 메서드
    // Unity 애니메이터에서 공격 애니메이션의 원하는 프레임에 이벤트를 추가하여 호출
    public void OnAttackHit()
    {
        if (currentTarget != null && companionData != null)
        {
            // 적에게 데미지 주기
            currentTarget.TakeDamage(companionData.GetAttackPower(currentLevel));
        }
        // 공격 파티클 생성
        if (companionData != null && companionData.attackParticlePrefab != null && currentTarget != null)
        {
            SpawnAttackParticle(currentTarget.transform.position);
        }
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 공격 속도에 따른 애니메이션 속도 설정
        if (companionData != null)
        {
            float attackSpeedMultiplier = companionData.GetAttackSpeed(currentLevel);
            animator.SetFloat(ANIM_PARAM_ATTACK_SPEED, attackSpeedMultiplier);
        }
        
        // 공격 중이 아닐 때만 이동/대기 애니메이션 처리
        if (!isAttacking && !isWaitingForAttackAnimation)
        {
            // Move 또는 Idle 상태 설정
            if (isMovingFrame)
            {
                // Move 상태로 전환
                if (!wasMoving)
                {
                    animator.SetBool(ANIM_PARAM_IS_MOVING, true);
                    wasMoving = true;
                }
                
                // 이동 속도에 따른 애니메이션 속도 조절
                if (companionData != null)
                {
                    // 간단히 1로 설정하거나 실제 속도 비례
                    animator.SetFloat(ANIM_PARAM_MOVE_SPEED, 1f);
                }
            }
            else
            {
                // Idle 상태로 전환
                if (wasMoving)
                {
                    animator.SetBool(ANIM_PARAM_IS_MOVING, false);
                    animator.SetFloat(ANIM_PARAM_MOVE_SPEED, 0f);
                    wasMoving = false;
                }
            }
        }
        else
        {
            // 공격 중일 때는 이동 애니메이션 비활성화
            if (wasMoving)
            {
                animator.SetBool(ANIM_PARAM_IS_MOVING, false);
                animator.SetFloat(ANIM_PARAM_MOVE_SPEED, 0f);
                wasMoving = false;
            }
        }
    }
    
    // ICombatTarget 인터페이스 구현
    public Transform GetTransform()
    {
        return transform;
    }
    
    public bool IsAlive()
    {
        return isAlive;
    }
    
    public void TakeDamage(int damage)
    {
        if (!isAlive) return;
        
        int actualDamage = Mathf.Max(1, damage - companionData.GetDefense(currentLevel));
        currentHealth = Mathf.Max(0, currentHealth - actualDamage);
        
        // 체력바 업데이트
        UpdateHealthBar();
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 피격 효과
            if (spriteRenderer != null)
            {
                spriteRenderer.DOKill();
                spriteRenderer.color = originalColor;
                spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
        }
    }
    
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return companionData != null ? companionData.GetMaxHealth(currentLevel) : 100;
    }
    
    public string GetName()
    {
        return companionData != null ? companionData.characterName : "동료";
    }
    
    private void Die()
    {
        isAlive = false;
        StopMoving();
        
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
        OnCompanionDefeated?.Invoke(this);
        
        // 사망 처리 (비활성화 또는 삭제)
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 공격 파티클을 생성합니다.
    /// </summary>
    /// <param name="targetPosition">공격 대상의 위치</param>
    private void SpawnAttackParticle(Vector3 targetPosition)
    {
        if (companionData == null || companionData.attackParticlePrefab == null)
        {
            return;
        }
        
        // 1. 공격 방향 계산
        Vector2 attackDirection = (targetPosition - transform.position).normalized;
        
        // 2. 파티클 생성 위치 (타겟 위치)
        Vector3 spawnPosition = targetPosition;
        
        // 3. 방향에 따른 회전 각도 계산
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        
        // 4. 파티클 풀에서 생성
        if (AttackParticlePool.Instance != null)
        {
            AttackParticlePool.Instance.SpawnParticle(
                companionData.attackParticlePrefab, 
                spawnPosition, 
                Quaternion.Euler(0f, 0f, angle), 
                1f
            );
        }
        else
        {
            // 폴백: 풀이 없으면 일반 생성
            GameObject particle = Instantiate(companionData.attackParticlePrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));
            Destroy(particle, 1f);
        }
    }
    
    /// <summary>
    /// 동료 초기화
    /// </summary>
    public void Initialize(CompanionData data, int level = 1)
    {
        companionData = data;
        currentLevel = level; // 레벨 설정
        currentHealth = data.GetMaxHealth(currentLevel);
        isAlive = true;
        currentAttackCooldown = 1f / data.GetAttackSpeed(currentLevel);

        if (animator == null) animator = GetComponent<Animator>();
        // 애니메이션 컨트롤러 설정
        if (animator != null && data.companionAnimatorController != null)
        {
            animator.runtimeAnimatorController = data.companionAnimatorController;
        }
        
        // 스프라이트 초기화
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = Color.white;
            originalColor = spriteRenderer.color;
        }
        
        // 체력바 초기화
        InitializeHealthBar();
    }
    
    /// <summary>
    /// 체력바 초기화
    /// </summary>
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
        if (healthBar != null && companionData != null)
        {
            healthBar.Initialize(transform, companionData.GetMaxHealth(currentLevel), currentHealth);
        }
    }
    
    /// <summary>
    /// 체력바 업데이트
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBar != null && companionData != null)
        {
            healthBar.UpdateHealth(currentHealth, companionData.GetMaxHealth(currentLevel));
        }
    }

    /// <summary>
    /// 동료의 스탯을 새로운 레벨에 맞춰 업데이트합니다.
    /// </summary>
    public void UpdateStats(int newLevel)
    {
        if (companionData == null) return;

        currentLevel = newLevel;
        
        // 최대 체력 증가분만큼 현재 체력도 회복시켜줄지, 아니면 비율 유지할지 결정
        // 여기서는 레벨업 시 풀피로 회복 (플레이어와 동일)
        currentHealth = companionData.GetMaxHealth(currentLevel);
        
        // 공격 속도 갱신
        float newAttackSpeed = companionData.GetAttackSpeed(currentLevel);
        currentAttackCooldown = 1f / newAttackSpeed;
        
        // 체력바 갱신
        if (healthBar != null)
        {
            healthBar.Initialize(transform, companionData.GetMaxHealth(currentLevel), currentHealth);
        }
        
        Debug.Log($"[CompanionController] Stats updated to Level {currentLevel}. HP: {currentHealth}, Atk: {companionData.GetAttackPower(currentLevel)}");
    }
}
