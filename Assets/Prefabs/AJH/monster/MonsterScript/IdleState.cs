using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : StateMachineBehaviour
{
    float timer;

    //[추격] - 추격 상태로 전환될 때 플레이어의 위치가 필요해서 플레이어의 Transform을 받을 변수 선언
    //근데 나중에 서버 붙이고 하면 플레이어 복제하면 태그 안겹치게 해야 할수도 있음.
    Transform targetPlayer;
    float chaseRange = 8;
    bool isPlayerSpawned = false;

    private void OnPlayerSpawned()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
            isPlayerSpawned = true;
        }
    }


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0;
        GameManager.OnPlayerSpawnCompleted += OnPlayerSpawned;

        //[추격] - 플레이어 transform 값 받아옴        
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
            isPlayerSpawned = true;
        }

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        if (timer > 5) animator.SetBool("isPatrolling",true);

        // 플레이어가 아직 null인 경우 다시 시도해서 찾기
        //if (targetPlayer == null)
        //{
        //    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        //    if (playerObject != null)
        //    {
        //        targetPlayer = playerObject.transform;
        //    }
        //}

        // 플레이어가 있는 경우에만 거리 계산 및 추격
        if (isPlayerSpawned && targetPlayer != null)
        {
            float distance = Vector3.Distance(targetPlayer.position, animator.transform.position);
            if (distance < chaseRange)
            {
                animator.SetBool("isChasing", true);
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.OnPlayerSpawnCompleted -= OnPlayerSpawned;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
