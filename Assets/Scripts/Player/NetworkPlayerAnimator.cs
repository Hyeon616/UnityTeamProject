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

    public void Initialize(string id, bool isLocal)
    {
        playerId = id;
        isLocalPlayer = isLocal;

        // 로컬 플레이어가 아닌 경우 입력 비활성화
        if (!isLocal)
        {
            enabled = false;
            this.enabled = true; 
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
        var stateData = new
        {
            action = "player_state",
            playerId = playerId,
            position = transform.position,
            rotation = transform.rotation,
            isRunning = _isRunning,
            isAction = isAction,
            hp = _hp
        };

        await ServerConnector.Instance.SendMessage(JsonConvert.SerializeObject(stateData));
    }

    // 입력 처리 메서드 오버라이드
    public override void OnMove(InputValue value)
    {
        if (!isLocalPlayer) return;
        base.OnMove(value);
    }

    public override void OnDash(InputValue value = null)
    {
        if (!isLocalPlayer) return;
        base.OnDash(value);
        SendActionEvent("dash");
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

    // 원격 플레이어 상태 업데이트
    public void UpdateRemoteState(Vector3 position, Quaternion rotation, bool isRunning, bool inAction, int hp)
    {
        if (isLocalPlayer) return;

        transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10f);
        _isRunning = isRunning;
        isAction = inAction;
        _hp = hp;

        _animator.SetBool("isRunning", isRunning);
    }

    // 원격 플레이어 액션 실행
    public void ExecuteRemoteAction(string actionName)
    {
        if (isLocalPlayer) return;

        switch (actionName)
        {
            case "attack":
                _animator.SetTrigger("onWeaponAttack");
                break;
            case "skillA":
                _animator.SetInteger("skillA", 0);
                _animator.Play("ChargeSkillA_Skill");
                StartCoroutine(SkillASlashLoop());
                break;
            case "skillB":
                StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f));
                break;
            case "dash":
                StartCoroutine(MovePlayerToPosition(
                    transform.position,
                    transform.position + transform.forward * 5f,
                    0.2f));
                break;
        }
    }
}
