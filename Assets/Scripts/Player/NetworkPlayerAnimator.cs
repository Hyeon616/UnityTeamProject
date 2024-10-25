using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerAnimator : playerAnimator
{
    private bool isLocalPlayer;
    private string playerId;
    private float syncInterval = 0.1f; // 동기화 간격
    private float lastSyncTime;

    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;
    private bool lastSentIsRunning;
    private bool lastSentIsAction;
    private int lastSentHealth;


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

    private void Start()
    {
        base.Start();
        if (isLocalPlayer)
        {
            // UserData에서 스탯 초기화
            _hp = UserData.Instance.Character.MaxHealth;
            _str = UserData.Instance.Character.AttackPower;
        }


    }


    protected new void Update()
    {
        if (!isLocalPlayer)
        {

            base.ApplyGravity();
            return;
        }

        base.Update();

        // 위치와 상태 동기화
        if (Time.time - lastSyncTime >= syncInterval)
        {
            SendPlayerState();
            lastSyncTime = Time.time;
        }
    }

    private async void SendPlayerState()
    {
        bool positionChanged = Vector3.Distance(lastSentPosition, transform.position) > 0.01f;
        bool rotationChanged = Quaternion.Angle(lastSentRotation, transform.rotation) > 1f;
        bool stateChanged = lastSentIsRunning != _isRunning || lastSentIsAction != isAction;

        // 위치, 회전 또는 상태가 변경되었을 때만 서버에 전송
        if (positionChanged || rotationChanged || stateChanged)
        {
            var position = new { x = transform.position.x, y = transform.position.y, z = transform.position.z };
            var rotation = new { x = transform.rotation.x, y = transform.rotation.y, z = transform.rotation.z, w = transform.rotation.w };

            var stateData = new
            {
                action = "player_state",
                playerId = playerId,
                position = position,
                rotation = rotation,
                isRunning = _isRunning,
                isAction = isAction,
                currentHealth = _hp,
                maxHealth = UserData.Instance.Character.MaxHealth,
                attackPower = UserData.Instance.Character.AttackPower
            };

            await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(stateData));

            // 마지막 전송 상태 업데이트
            lastSentPosition = transform.position;
            lastSentRotation = transform.rotation;
            lastSentIsRunning = _isRunning;
            lastSentIsAction = isAction;
        }
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
        base.OnClick();
        SendActionEvent("attack");
    }



    private async void SendActionEvent(string actionName)
    {
        var actionData = new
        {
            action = "player_action",
            playerId = playerId,
            actionName = actionName
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(actionData));
    }

    public void UpdateState(Vector3 position, Quaternion rotation, bool isRunning, bool inAction, int currentHealth, int maxHealth, int attackPower)
    {
        if (isLocalPlayer) return;

        // 위치와 회전을 빠르게 반영하기 위해 보간 비율을 조정
        transform.position = Vector3.Lerp(transform.position, position, 0.2f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);

        _isRunning = isRunning;
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
        _animator.SetTrigger("onWeaponAttack");

        // 공격 이펙트
        attackEvent("1");

        if (playerSound != null) playerSound.BaseAttack();
        yield return new WaitForSeconds(0.5f);


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
            attack.transform.Find("Slash").gameObject.SetActive(true);
            yield return new WaitForSeconds(2.9f);
            attack.transform.Find("Slash").gameObject.SetActive(false);
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

        // 이펙트들
        SkillBEffectGround();
        yield return new WaitForSeconds(0.27f);
        SkillBEffectWeapon();
        yield return new WaitForSeconds(0.9f);
        SkillBEffectExplosion();

        // 사운드
        if (playerSound != null) playerSound.SkillB();

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
