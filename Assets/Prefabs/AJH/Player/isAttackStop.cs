using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class isAttackStop : StateMachineBehaviour
{
    ColliderScript weaponColliderScript;
    playerAnimator playerAnimator;
    public float rotationStep = 2f;
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
        if (stateInfo.IsTag("a"))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("p_Idle") || animator.GetCurrentAnimatorStateInfo(0).IsName("p_run") || animator.GetCurrentAnimatorStateInfo(0).IsName("ChargeSkillA_Skill"))
            {
                playerAnimator.isAction = false;

            }
        }
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
            if (otherCollider.CompareTag("Lazer_point"))
            {
                Debug.Log("��ž" + otherCollider.transform.position.x);
                Debug.Log("�÷��̾�" + playerAnimator.transform.position.x);

                // Rotate the turret to face the player
                RotateTurretTowardsPlayer(otherCollider.transform);
            }
            //Debug.Log("�޺� ������.. " + otherCollider.gameObject.name);
            if (otherCollider.gameObject.tag == "Monster")
            {
                
                otherCollider.GetComponent<MonsterInfo>().TakeDamage(10);

            }
            //�ı� �Ǵ� ������Ʈ ���� �߰�(����)
            else if (otherCollider.gameObject.name == "DestroyBox")
            {
                otherCollider.GetComponent<BoxInfo>().BoxDamaged(1);
            } else if (otherCollider.gameObject.tag == "Boss")
            {
                otherCollider.GetComponent<Boss>().TakeDamage(playerAnimator.getstr);
            }
            //=======================================================================(�����۾� ���� ��)
            hasCollided = true;

            // ���⿡�� �浹�� ó���ϴ� �ڵ带 �߰��ϼ���.
        }
    }
    private void RotateTurretTowardsPlayer(Transform turretTransform)
    {
        float currentYRotation = turretTransform.eulerAngles.y;
        float targetYRotation;

        if (playerAnimator.transform.position.x < turretTransform.position.x)
        {
            // Player is on the left side, rotate counterclockwise
            targetYRotation = currentYRotation - rotationStep;
        }
        else
        {
            // Player is on the right side, rotate clockwise
            targetYRotation = currentYRotation + rotationStep;
        }

        // Ensure the rotation is within 0-360 degrees
        if (targetYRotation < 0)
            targetYRotation += 360;
        else if (targetYRotation > 360)
            targetYRotation -= 360;

        // Apply the new rotation
        turretTransform.rotation = Quaternion.Euler(0, targetYRotation, 0);
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