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
        // 무기의 콜라이더 스크립트를 찾습니다.
        weaponColliderScript = animator.GetComponentInChildren<ColliderScript>();

        // 충돌 이벤트를 처리하는 메서드를 설정합니다.
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
        // 충돌 이벤트를 처리하는 메서드를 제거합니다.
        weaponColliderScript.OnTriggerEnterEvent -= OnTriggerEnterEventHandler;

        // 충돌 플래그를 초기화합니다.
        hasCollided = false;

    }

    private void OnTriggerEnterEventHandler(Collider otherCollider)
    {
        
        // 충돌이 처음 감지될 때만 처리합니다.
        if (!hasCollided)
        {
            Debug.Log(otherCollider.gameObject.name);
            if (otherCollider.CompareTag("Lazer_point"))
            {
                Debug.Log("포탑" + otherCollider.transform.position.x);
                Debug.Log("플레이어" + playerAnimator.transform.position.x);

                // Rotate the turret to face the player
                RotateTurretTowardsPlayer(otherCollider.transform);
            }
            //Debug.Log("콤보 공격중.. " + otherCollider.gameObject.name);
            if (otherCollider.gameObject.tag == "Monster")
            {
                
                otherCollider.GetComponent<MonsterInfo>().TakeDamage(10);
                
            }
            //파괴 되는 오브젝트 조건 추가(준후)
            else if (otherCollider.gameObject.name == "DestroyBox")
            {
                otherCollider.GetComponent<BoxInfo>().BoxDamaged(1);
            } else if (otherCollider.gameObject.tag == "Boss")
            {
                otherCollider.GetComponent<Boss>().TakeDamage(playerAnimator.getstr);
            }
            //==
            hasCollided = true;

            // 여기에서 충돌을 처리하는 코드를 추가하세요.
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