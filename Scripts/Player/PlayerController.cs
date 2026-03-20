using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float attackRange = 2f;

    [Header("공격 설정")]
    public float baseAttackCooldown = 1f; // 기본 공격 쿨다운 (attackSpeed가 1.0일 때)
    public float lastAttackTime = 0f;
    public bool isAttacking = false;
    public float baseAttackAnimationDuration = 0.5f; // 기본 공격 애니메이션 지속 시간

    [Header("AI 설정")]
    public bool isAutoMode = true;
    public float searchInterval = 0.5f;
    public float lastSearchTime = 0f;

    [Header("성능 최적화")]
    private Enemy[] cachedEnemies = new Enemy[0];
    private float lastEnemyCacheTime = 0f;
    public float enemyCacheInterval = 0.2f; // 적 목록 캐시 간격
    public float distanceCheckInterval = 0.1f; // 거리 체크 간격
    private float lastDistanceCheckTime = 0f;
    private float cachedDistanceToTarget = float.MaxValue;

    [Header("안전장치")]
    private int frameCount = 0;
    private const int MAX_FRAMES_PER_UPDATE = 5; // 한 번에 최대 처리할 프레임 수

    [Header("애니메이션 설정")]
    private const string ANIM_PARAM_IS_MOVING = "IsMoving";
    private const string ANIM_PARAM_ATTACK_TRIGGER = "AttackTrigger";
    private const string ANIM_PARAM_ATTACK_SPEED = "AttackSpeed";
    private const string ANIM_PARAM_MOVE_SPEED = "MoveSpeed";

    private PlayerStats playerStats;
    private DungeonSystem dungeonSystem;
    private Enemy currentTarget;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerAttackParticles attackParticles; // 공격 파티클 시스템

    private Color originalColor;

    // 애니메이션 상태 추적
    private bool wasMoving = false;
    private float currentAttackCooldown = 1f;
    private float currentAttackAnimationDuration = 0.5f;
    private bool isWaitingForAttackAnimation = false; // 공격 애니메이션 완료 대기 중

    public event System.Action<Enemy> OnAttackEnemy;
    public event System.Action OnPlayerMove;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        if (playerStats != null)
        {
            playerStats.OnDamageTaken += HandleDamageTaken;
            playerStats.OnDeath += HandleDeath;
        }

        dungeonSystem = FindFirstObjectByType<DungeonSystem>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attackParticles = GetComponent<PlayerAttackParticles>(); // 파티클 컴포넌트 가져오기

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDamageTaken -= HandleDamageTaken;
            playerStats.OnDeath -= HandleDeath;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
    }

    private void HandleDamageTaken(int damage)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.color = originalColor;
            spriteRenderer.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }

    private void HandleDeath()
    {
        StopMoving();
        isAutoMode = false; // 자동 전투 중지

        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            Sequence deathSequence = DOTween.Sequence();
            deathSequence.Append(spriteRenderer.DOColor(Color.red, 0.2f));
            deathSequence.Append(spriteRenderer.DOFade(0f, 0.5f));
            deathSequence.OnComplete(() =>
            {
                dungeonSystem?.HandlePlayerDefeated();
            });
        }
        else
        {
            dungeonSystem?.HandlePlayerDefeated();
        }
    }

    private bool isMovingFrame = false; // 현재 프레임에서 이동했는지 여부

    private void Update()
    {
        if (!playerStats.IsAlive()) return;

        // 안전장치: 프레임 카운터로 무한 루프 방지
        frameCount++;
        if (frameCount > MAX_FRAMES_PER_UPDATE)
        {
            frameCount = 0;
            return;
        }

        isMovingFrame = false; // 프레임 시작 시 초기화

        if (isAutoMode)
        {
            HandleAutoCombat();
        }

        UpdateAnimation();

        frameCount = 0; // 정상 완료 시 리셋
    }

    private void HandleAutoCombat()
    {
        // 적을 찾거나 현재 타겟이 유효한지 확인 (성능 최적화: 간격 제한)
        if (currentTarget == null || !currentTarget.isAlive || !currentTarget.gameObject.activeInHierarchy)
        {
            // 타겟이 없거나 죽었거나 비활성화되면 새 타겟 찾기
            currentTarget = null;

            if (Time.time - lastSearchTime >= searchInterval)
            {
                FindNearestEnemy();
            }
        }

        if (currentTarget != null)
        {
            // 주기적으로 거리 체크 (성능 최적화)
            if (Time.time - lastDistanceCheckTime >= distanceCheckInterval)
            {
                cachedDistanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
                lastDistanceCheckTime = Time.time;
            }

            if (cachedDistanceToTarget <= attackRange)
            {
                // 사정거리 내에 있으면 공격
                if (CanAttack())
                {
                    AttackTarget();
                }
                else
                {
                    // 공격 쿨다운 중이면 정지
                    StopMoving();
                }
            }
            else if (isAttacking == false)
            {
                // 사정거리 밖에 있으면 이동
                MoveToTarget();
            }
        }
        else
        {
            // 타겟이 없으면 정지
            StopMoving();
        }
    }

    private void FindNearestEnemy()
    {
        if (Time.time - lastSearchTime < searchInterval) return;

        lastSearchTime = Time.time;

        // 캐시된 적 목록 사용 (성능 최적화)
        UpdateEnemyCache();

        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in cachedEnemies)
        {
            if (enemy == null || !enemy.isAlive) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance < nearestDistance)
            {
                nearestEnemy = enemy;
                nearestDistance = distance;
            }
        }

        currentTarget = nearestEnemy;
    }

    private void UpdateEnemyCache()
    {
        // 캐시 업데이트 간격 체크
        if (Time.time - lastEnemyCacheTime < enemyCacheInterval) return;

        lastEnemyCacheTime = Time.time;
        cachedEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }

    private void StopMoving()
    {
        // Transform 이동 방식에서는 별도의 정지 로직이 필요 없음
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 현재 타겟으로의 선 표시
        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }

    private void MoveToTarget()
    {
        if (currentTarget == null) return;

        // 타겟 방향으로 이동
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        isMovingFrame = true; // 이동 플래그 설정

        // 스프라이트 방향 전환 (2D)
        if (direction.x > 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // 오른쪽
        }
        else if (direction.x < 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = true; // 왼쪽
        }

        OnPlayerMove?.Invoke();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;

        // 공격 속도에 따른 애니메이션 속도 설정
        if (playerStats != null)
        {
            float attackSpeedMultiplier = playerStats.attackSpeed;
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

                // 이동 속도에 따른 애니메이션 속도 조절 (간단히 1로 설정하거나 moveSpeed 비례)
                animator.SetFloat(ANIM_PARAM_MOVE_SPEED, 1f);
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

    private bool CanAttack()
    {
        // 공격 쿨다운 계산 (attackSpeed에 따라 조절)
        if (playerStats != null)
        {
            currentAttackCooldown = baseAttackCooldown / playerStats.attackSpeed;
        }
        else
        {
            currentAttackCooldown = baseAttackCooldown;
        }
        
        return Time.time - lastAttackTime >= currentAttackCooldown && !isAttacking && !isWaitingForAttackAnimation;
    }
    
    private void AttackTarget()
    {
        if (currentTarget == null || !CanAttack()) return;
        
        isAttacking = true;
        isWaitingForAttackAnimation = true;
        lastAttackTime = Time.time;
        
        // 공격 애니메이션 지속 시간 계산 (attackSpeed에 따라 조절)
        if (playerStats != null)
        {
            currentAttackAnimationDuration = baseAttackAnimationDuration / playerStats.attackSpeed;
        }
        else
        {
            currentAttackAnimationDuration = baseAttackAnimationDuration;
        }
        
        // 타겟을 바라보도록 스프라이트 방향 설정
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        if (direction.x > 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = false; // 오른쪽
        }
        else if (direction.x < 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = true; // 왼쪽
        }
        
        // 이동 정지
        StopMoving();
        
        // 공격 애니메이션 트리거 (한 번만 재생)
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
        if (currentTarget != null && playerStats != null)
        {
            // 적에게 데미지 주기
            currentTarget.TakeDamage(playerStats.attackPower);
            
            OnAttackEnemy?.Invoke(currentTarget);
        }
        // 공격 파티클 생성
        if (attackParticles != null && currentTarget != null)
        {
            attackParticles.SpawnAttackParticle(currentTarget.transform.position);
        }
    }

    public void SetAutoMode(bool enabled)
    {
        isAutoMode = enabled;
        
        if (!enabled)
        {
            StopMoving();
            currentTarget = null;
        }
    }
    
    public void SetTarget(Enemy target)
    {
        currentTarget = target;
    }
    
    public Enemy GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool IsMoving()
    {
        return isMovingFrame;
    }

}