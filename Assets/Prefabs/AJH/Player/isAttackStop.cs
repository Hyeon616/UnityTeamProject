using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isAttackStop : StateMachineBehaviour
{
    ColliderScript weaponColliderScript;
    playerAnimator playerAnimator;
    bool hasCollided = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        playerAnimator = animator.GetComponent<playerAnimator>();
        playerAnimator.isAction = true;
        // ������ �ݶ��̴� ��ũ��Ʈ�� ã���ϴ�.
        weaponColliderScript = animator.GetComponentInChildren<ColliderScript>();

        // �浹 �̺�Ʈ�� ó���ϴ� �޼��带 �����մϴ�.
        weaponColliderScript.OnTriggerEnterEvent += OnTriggerEnterEventHandler;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerAnimator = animator.GetComponent<playerAnimator>();
        playerAnimator.isAction = false;
        // �浹 �̺�Ʈ�� ó���ϴ� �޼��带 �����մϴ�.
        weaponColliderScript.OnTriggerEnterEvent -= OnTriggerEnterEventHandler;

        // �浹 �÷��׸� �ʱ�ȭ�մϴ�.
        hasCollided = false;

    }

    private void OnTriggerEnterEventHandler(Collider otherCollider)
    {
        
        // �浹�� ó�� ������ ���� ó���մϴ�.
        if (!hasCollided)
        {
            Debug.Log(otherCollider.gameObject.name);
            //Debug.Log("�޺� ������.. " + otherCollider.gameObject.name);
            if(otherCollider.gameObject.tag == "Monster")
            {
                
                otherCollider.GetComponent<MonsterInfo>().TakeDamage(10);

            }
            hasCollided = true;

            // ���⿡�� �浹�� ó���ϴ� �ڵ带 �߰��ϼ���.
        }
    }
}
/*
public class isAttackStop : StateMachineBehaviour
{
    playerAnimator playerAnimator;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("hi");
        playerAnimator = animator.GetComponent<playerAnimator>();
        playerAnimator.isAction = true;
        playerAnimator.EnableWeapon();

    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        playerAnimator = animator.GetComponent<playerAnimator>();
        playerAnimator.isAction = false;
        playerAnimator.DisableWeapon();

    }
}
*/