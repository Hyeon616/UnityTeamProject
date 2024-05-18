using UnityEngine;
using System.Collections;
public interface IBossState
{
    void Enter(Boss boss);   // ���¿� ������ �� ȣ��Ǵ� �޼���
    void Execute(Boss boss); // ���°� Ȱ��ȭ�� ���� �� ������ ȣ��Ǵ� �޼���
    void Exit(Boss boss);    // ���¿��� ���� �� ȣ��Ǵ� �޼���
}
public class Boss : MonoBehaviour
{
    private IBossState currentState; // ���� ����
    private IBossState previousState; // ���� ����
    public float Health { get; private set; } // ������ ü��
    public float GimmickThreshold1 { get; private set; } // ��� �Ӱ谪 1
    public float GimmickThreshold2 { get; private set; } // ��� �Ӱ谪 2
    public float GimmickThreshold3 { get; private set; } // ��� �Ӱ谪 3
    private Animator animator; // �ִϸ�����
    public bool IsUsingLaser { get; private set; } // ������ ��� ����

    /*���1 start*/
    public GameObject laserEffectPrefab; // ������ ����Ʈ ������
    public float laserDistance = 10f; // ������ �Ÿ�
    public LayerMask floorLayerMask; // �ٴ� ���̾� ����ũ
    /*���1 end*/
    void Start()
    {
        GimmickThreshold1 = 30f; // ü�� �Ӱ谪 ����
        GimmickThreshold2 = 50f;
        GimmickThreshold3 = 70f;
        animator = GetComponent<Animator>();
        ChangeState(new NormalState()); // �ʱ� ���¸� Normal ���·� ����
    }

    void Update()
    {
        currentState?.Execute(this); // �� ������ ���� ������ Execute �޼��� ����
    }


    public void ChangeState(IBossState newState)
    {
        previousState = currentState; // ���� ���� ����
        currentState?.Exit(this); // ���� ���� ����
        currentState = newState; // ���ο� ���� ����
        currentState.Enter(this); // ���ο� ���� ����
    }

    public void SetAnimation(string animationName)
    {
        Debug.Log("Setting animation to: " + animationName);
        animator.Play(animationName); // ������ �ִϸ��̼� ���
    }

    public void OnGimmickAnimationEvent(string eventName)
    {
        Debug.Log("Animation Event Triggered: " + eventName);
        if (eventName == "StartLaser")
        {
            StartLaser(GameObject.FindGameObjectWithTag("Player").transform.position); // ������ ����
        }
        else if (eventName == "ThrowRock")
        {
            ThrowRock(GameObject.FindGameObjectWithTag("Player").transform.position); // ���� ������
        }
        else if (eventName == "AnimationComplete")
        {
            SetAnimation("Idle"); // �ִϸ��̼� �Ϸ� �� Idle �ִϸ��̼� ����
        }
    }

    public void StartLaser(Vector3 position)
    {
        Debug.Log("Starting Laser at Position: " + position);
        IsUsingLaser = true; // ������ ��� ���� ����
        // ������ ���� ���� ����
    }

    public void UpdateLaserPosition(Vector3 position)
    {
        if (IsUsingLaser)
        {
            Debug.Log("Updating Laser Position to: " + position);
            // ������ ��ġ ������Ʈ ���� ����
        }
    }

    public void FireLaser()
    {
        Debug.Log("Firing Laser"); // ������ �߻� �α�
        // ������ �߻� ���� ����
    }

    public void StopLaser()
    {
        Debug.Log("Stopping Laser");
        IsUsingLaser = false; // ������ ��� ���� ����
        // ������ ���� ���� ����

        // ���� ����� �ٽ� �����ϱ� ���� �ڷ�ƾ ����
        StartCoroutine(WaitAndReExecuteGimmick());
    }

    private IEnumerator WaitAndReExecuteGimmick()
    {
        IBossState currentGimmickState = currentState; // ���� ���� ����
        float waitTime = 3f; // ��� �ð� ���� (�� ����)
        Debug.Log("Waiting for " + waitTime + " seconds before re-executing the gimmick.");
        yield return new WaitForSeconds(waitTime); // ������ �ð� ���

        // ���°� ������� �ʾ��� ��쿡�� ���� ��� ���¸� �ٽ� ����
        if (currentState == currentGimmickState)
        {
            ChangeState(currentState); // ���� ���·� �ٽ� ����
        }
    }

    public void ThrowRock(Vector3 targetPosition)
    {
        Debug.Log("Throwing Rock at Position: " + targetPosition);
        // ���� ������ ���� ����
    }

    public void CheckHealthAndChangeState()
    {
        // ü�� üũ �� ���� ��ȯ ����
        if (Health < GimmickThreshold1)
        {
            ChangeState(new GimmickState3()); // ���� ���� �Ӱ谪���� üũ
        }
        else if (Health < GimmickThreshold2)
        {
            ChangeState(new GimmickState2());
        }
        else if (Health < GimmickThreshold3)
        {
            ChangeState(new GimmickState1());
        }
        else
        {
            ChangeState(new NormalState());
        }
    }
}
public class NormalState : IBossState
{
    public void Enter(Boss boss)
    {
        Debug.Log("Entering Normal State");
        boss.SetAnimation("Idle"); // Idle �ִϸ��̼� ����
    }

    public void Execute(Boss boss)
    {
        Debug.Log("test");
        FireLasersInEightDirections(boss);
        //        boss.CheckHealthAndChangeState(); // ü�� üũ �� ���� ����
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Normal State");
    }
    void FireLasersInEightDirections(Boss boss)
    {
        // 8���� ���� ����
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };

        foreach (Vector3 direction in directions)
        {
            RaycastHit hit;
            // Raycast�� ����Ͽ� �ٴڿ� ��� ���� ã��
            if (Physics.Raycast(boss.transform.position, direction, out hit, boss.laserDistance, boss.floorLayerMask))
            {
                Debug.Log("Laser hit at: " + hit.point);

                // ������ ����Ʈ ����
                Object.Instantiate(boss.laserEffectPrefab, hit.point, Quaternion.identity);
            }
            else
            {
                Debug.Log("Laser did not hit anything in direction: " + direction);
            }
        }
    }

}
public class GimmickState1 : IBossState
{
    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        Debug.Log("Entering Gimmick State 1");
        boss.SetAnimation("Gimmick1"); // Gimmick1 �ִϸ��̼� ����
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // �÷��̾��� Transform ��������
    }

    public void Execute(Boss boss)
    {
        // �ǽð����� �÷��̾��� ��ġ�� �����ϰ� �ʿ� �� ������ �߻� ���
        if (boss.IsUsingLaser)
        {
            boss.UpdateLaserPosition(playerTransform.position); // ������ ��ġ ������Ʈ
            boss.FireLaser(); // ������ �߻�
        }
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Gimmick State 1");
        boss.StopLaser(); // ���� ���� �� ������ ����
    }
}

public class GimmickState2 : IBossState
{
    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        Debug.Log("Entering Gimmick State 2");
        boss.SetAnimation("Gimmick2"); // Gimmick2 �ִϸ��̼� ����
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // �÷��̾��� Transform ��������
    }

    public void Execute(Boss boss)
    {
        // �ʿ� �� �߰� ���� �߰�
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Gimmick State 2");
        boss.StopLaser(); // ���� ���� �� ������ ����
    }
}
public class GimmickState3 : IBossState
{
    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        Debug.Log("Entering Gimmick State 3");
        boss.SetAnimation("Gimmick3"); // Gimmick3 �ִϸ��̼� ����
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // �÷��̾��� Transform ��������
    }

    public void Execute(Boss boss)
    {
        // �ʿ� �� �߰� ���� �߰�
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Gimmick State 3");
        boss.StopLaser(); // ���� ���� �� ������ ����
    }
}