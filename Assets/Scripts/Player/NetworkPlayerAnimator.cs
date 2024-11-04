using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerAnimator : playerAnimator
{
    public bool isLocalPlayer;
    public string playerId;
    private float lastSyncTime;
    private float rotateSpeed = 7f;

    private struct AnimationState
    {
        public bool isRunning;
        public bool isAction;
        public int skillA;
        public string currentTrigger;
        public Dictionary<string, bool> activatedEffects;
    }

    private AnimationState currentState;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private Quaternion targetRotation;
    private Quaternion previousRotation;

    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;
    private bool lastSentIsRunning;
    private bool lastSentIsAction;
    private int lastSentHealth;

    private Vector3 velocityVector = Vector3.zero;
    private float positionSmoothTime = 0.08f; 
    private const float MIN_DISTANCE_THRESHOLD = 0.01f;
    private float syncInterval = 0.1f; // 동기화 간격

    private float moveSpeed = 3f;      // 이동 속도
    private Vector3 currentVelocity;



    private Vector3 networkPosition;
    private Vector3 networkVelocity;
    private Vector3 previousTargetPosition;
    private float lastPacketTime;
    private const float VELOCITY_LERP_SPEED = 10f;
    private Queue<TransformState> positionBuffer = new Queue<TransformState>();
    private const int BUFFER_SIZE = 2;

    private struct TransformState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float timestamp;

        public TransformState(Vector3 pos, Quaternion rot, float time)
        {
            position = pos;
            rotation = rot;
            timestamp = time;
        }
    }


    public void Initialize(string id, bool isLocal)
    {
        playerId = id;
        isLocalPlayer = isLocal;

        // 필요한 컴포넌트들 찾아서 할당
        if (_animator == null)
            _animator = GetComponent<Animator>();
        if (_characterController == null)
            _characterController = GetComponent<CharacterController>();

        GameObject canvasSkill = GameObject.Find("Canvas_Skill");
        if (canvasSkill != null)
        {
            Transform skillsTransform = canvasSkill.transform.Find("Skills");
            if (skillsTransform != null)
            {
                skillControlObject = skillsTransform.gameObject;
                skill = skillControlObject.GetComponent<SkillControl>();
            }
            else
            {
                Debug.LogError("Skills object not found in Canvas_Skill");
            }
        }
        else
        {
            Debug.LogError("Canvas_Skill not found in scene");
        }

        // 로컬 플레이어가 아닌 경우 입력 비활성화
        if (!isLocal)
        {
            // InputSystem 비활성화를 위해 PlayerInput 컴포넌트 찾아서 비활성화
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;

            if (skill != null)
            {
                foreach (var button in skill.hideSkillButtons)
                {
                    button.SetActive(false);
                }
                foreach (var text in skill.textPros)
                {
                    text.SetActive(false);
                }
            }
        }

        lastSentPosition = transform.position;
        lastSentRotation = transform.rotation;
        lastSentIsRunning = _isRunning;
        lastSentIsAction = isAction;
        lastSentHealth = _hp;


    }


    protected new void Update()
    {

        if (!isLocalPlayer)
        {
            // 다른 플레이어의 이동 처리
            if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                // moveSpeed로 목표 지점을 향해 이동
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                // 회전도 부드럽게 처리
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    360f * Time.deltaTime  // 1초에 한바퀴 회전 가능
                );
            }

            base.ApplyGravity();
            return;
        }
        base.Update();

        // 위치와 상태 동기화
        if (Time.time - lastSyncTime >= syncInterval)
        {
            bool shouldSync = Vector3.Distance(lastSentPosition, transform.position) > 0.01f ||
                            Quaternion.Angle(lastSentRotation, transform.rotation) > 1f ||
                            lastSentIsRunning != _isRunning ||
                            lastSentIsAction != isAction;

            if (shouldSync)
            {
                SendPlayerState();
                lastSyncTime = Time.time;

                // 마지막 전송 상태 저장
                lastSentPosition = transform.position;
                lastSentRotation = transform.rotation;
                lastSentIsRunning = _isRunning;
                lastSentIsAction = isAction;
            }
        }
    }

    private async void SendPlayerState()
    {
        var stateData = new
        {
            action = "player_state",
            playerId = playerId,
            position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
            rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
            isRunning = _isRunning,
            isAction = isAction,
            currentHealth = _hp,
            maxHealth = UserData.Instance.Character.MaxHealth,
            attackPower = UserData.Instance.Character.AttackPower
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(stateData));
    }

    // 입력 처리 메서드 오버라이드
    public override void OnMove(InputValue value)
    {
        if (!isLocalPlayer) return;
        base.OnMove(value);
    }



    public override void OnSkillA(InputValue value = null)
    {
        if (!isLocalPlayer) return;
        if (isAction) return;
        if (value != null && !skill.isHideSkills[1])
        {
            skill.HideSkillSetting(1);
            return;
        }
        base.OnSkillA(value);
        SendActionEvent("skillA");
    }

    public override void OnSkillB(InputValue value = null)
    {
        if (!isLocalPlayer) return;
        if (isAction) return;
        if (value != null && !skill.isHideSkills[2])
        {
            skill.HideSkillSetting(2);
            return;
        }
        base.OnSkillB(value);
        SendActionEvent("skillB");
    }

    public override void OnDash(InputValue value = null)
    {
        if (!isLocalPlayer) return;
        if (isAction) return;
        if (value != null && !skill.isHideSkills[0])
        {
            skill.HideSkillSetting(0);
            return;
        }
        base.OnDash(value);
        SendActionEvent("dash");
    }

    public override void OnClick()
    {
        if (!isLocalPlayer) return;
        //if (isAction) return;
        base.OnClick();
        SendActionEvent("attack");
    }



    private async void SendActionEvent(string actionName)
    {
        var actionData = new
        {
            action = "player_action",
            playerId = playerId,
            actionName = actionName,
            position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
            rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
            animation = new
            {
                currentTrigger = actionName,
                skillA = _skillA,
                isAction = isAction
            },
            effects = new
            {
                attackEffect = actionName == "attack",
                dashEffect = actionName == "dash",
                skillAEffect = actionName == "skillA",
                skillBEffect = actionName == "skillB"
            }
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(actionData));
    }

    private async void SendAnimationState()
    {
        var animationData = new
        {
            action = "player_state",
            playerId = playerId,
            position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z },
            rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w },
            animation = new
            {
                isRunning = _isRunning,
                isAction = isAction,
                skillA = _skillA,
                currentTrigger = currentState.currentTrigger
            },
            effects = new Dictionary<string, bool>(),
            stats = new
            {
                currentHealth = _hp,
                maxHealth = UserData.Instance.Character.MaxHealth,
                attackPower = UserData.Instance.Character.AttackPower
            }
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(animationData));
    }

    public void UpdateState(Vector3 position, Quaternion rotation, bool isRunning, bool inAction, int currentHealth, int maxHealth, int attackPower)
    {
        
        if (isLocalPlayer) return;

        // 새로운 목표 위치와 회전 설정
        targetPosition = position;
        targetRotation = rotation;

        // 상태 업데이트
        _isRunning = isRunning;
        isAction = inAction;
        _hp = currentHealth;
        _str = attackPower;

        _animator.SetBool("isRunning", isRunning);
    }

    public void ExecuteAction(string actionName)
    {
        if (isLocalPlayer) return;

        switch (actionName)
        {
            case "attack":
                // 기본 공격
                StartCoroutine(ExecuteAttack());
                break;

            case "skillA":
                // 스킬 A
                StartCoroutine(ExecuteSkillA());
                break;

            case "skillB":
                // 스킬 B
                StartCoroutine(ExecuteSkillB());
                break;

            case "dash":
                // 대시
                StartCoroutine(ExecuteDash());
                break;
        }
    }

    private IEnumerator ExecuteAttack()
    {
        isAction = true;
        _animator.SetTrigger("onWeaponAttack");

        // 공격 이펙트
        attackEvent("1");

        if (playerSound != null) playerSound.BaseAttack();
        yield return new WaitForSeconds(0.5f);

        isAction = false;
    }

    private IEnumerator ExecuteSkillA()
    {
        isAction = true;

        // 애니메이션
        _animator.SetInteger("skillA", 0);
        _animator.Play("ChargeSkillA_Skill");

        // 이펙트
         if (attack != null)
        {
            var slashEffect = attack.transform.Find("Slash").gameObject;
            slashEffect.SetActive(true);
            yield return new WaitForSeconds(2.9f);
            slashEffect.SetActive(false);
        }

        if (playerSound != null)
        {
            playerSound.SkillA();
        }


        // 사운드
        if (playerSound != null) playerSound.SkillA();

        yield return null;
        isAction = false;
    }

    private IEnumerator ExecuteSkillB()
    {
        isAction = true;

        StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f));

       
        SkillBEffectGround();
        yield return new WaitForSeconds(0.27f);
        SkillBEffectWeapon();
        yield return new WaitForSeconds(0.9f);
        SkillBEffectExplosion();

        // 사운드
        if (playerSound != null)
            playerSound.SkillB();

        yield return new WaitForSeconds(1.0f);
        isAction = false;
    }

    private IEnumerator ExecuteDash()
    {
        isAction = true;
        Vector3 dashDestination = transform.position + transform.forward * 5f;

        // 대시 이펙트
        if (attack != null)
        {
            attack.transform.Find("Dash").gameObject.SetActive(true);
        }

        // 이동
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        float duration = 0.2f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, dashDestination, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = dashDestination;

        // 이펙트 종료
        if (attack != null)
        {
            attack.transform.Find("Dash").gameObject.SetActive(false);
        }

        // 사운드
        if (playerSound != null) playerSound.Dash();

        isAction = false;
    }

}
