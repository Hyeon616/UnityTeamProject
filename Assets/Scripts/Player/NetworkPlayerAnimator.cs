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
        // 상태가 변경되었는지 확인
        bool hasChanged = Vector3.Distance(lastSentPosition, transform.position) > 0.01f ||
                         Quaternion.Angle(lastSentRotation, transform.rotation) > 1f ||
                         lastSentIsRunning != _isRunning ||
                         lastSentIsAction != isAction ||
                         lastSentHealth != _hp;

        // 변경된 경우에만 전송
        if (hasChanged)
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

            // 전송된 상태 저장
            lastSentPosition = transform.position;
            lastSentRotation = transform.rotation;
            lastSentIsRunning = _isRunning;
            lastSentIsAction = isAction;
            lastSentHealth = _hp;
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
        base.OnSkillA(value);
        SendActionEvent("skillA");
    }

    public override void OnSkillB(InputValue value = null)
    {
        if (!isLocalPlayer) return;
        base.OnSkillB(value);
        SendActionEvent("skillB");
    }

    public override void OnDash(InputValue value = null)
    {
        if (!isLocalPlayer) return;
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

    public void UpdateState(Vector3 position, Quaternion rotation, bool isRunning, bool inAction,
         int currentHealth, int maxHealth, int attackPower)
    {
        if (isLocalPlayer) return;

        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10f);
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
                OnClick();
                break;
            case "skillA":
                OnSkillA();
                break;
            case "skillB":
                OnSkillB();
                break;
            case "dash":
                OnDash();
                break;
        }
    }


}
