using UnityEngine;
using System.Collections;

//GimikAgain(); ��� �����
//��  Danger ��� : �� ������Ʈ�� Wall Layer�߰��� �����, �����ʿ��� �׳� ���̶�� �ǴܵǴ°� �� Wall ���̾� ���� �ؾ���
public interface IBossState
{
    void Enter(Boss boss);   // ���¿� ������ �� ȣ��Ǵ� �޼���
    void Execute(Boss boss); // ���°� Ȱ��ȭ�� ���� �� ������ ȣ��Ǵ� �޼���
    void Exit(Boss boss);    // ���¿��� ���� �� ȣ��Ǵ� �޼���
}
public class Boss : MonoBehaviour
{
    /* Player */
    [SerializeField] public Transform Target_Temp;
    /* Player End*/

    private IBossState currentState; // ���� ����
    private IBossState previousState; // ���� ����
    public float Health { get; private set; } // ������ ü��
    public float GimmickThreshold1 { get; private set; } // ��� �Ӱ谪 1
    public float GimmickThreshold2 { get; private set; } // ��� �Ӱ谪 2
    public float GimmickThreshold3 { get; private set; } // ��� �Ӱ谪 3
    private Animator animator; // �ִϸ�����
    public bool IsUsingLaser { get; private set; } // ������ ��� ����

    void Start()
    {
        Health = 100f;
        
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

    public void StartBossCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
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

    public void GimikAgain()
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
        float waitTime = 10f; // ��� �ð� ���� (�� ����)
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

/********************Normar*********************************/
public class NormalState : IBossState
{
    public void Enter(Boss boss)
    {
        Debug.Log("Entering Normal State");
        boss.SetAnimation("Idle"); // Idle �ִϸ��̼� ����
        boss.StartBossCoroutine(SomeCoroutine(boss)); // �ڷ�ƾ ����
    }

    public void Execute(Boss boss)
    {
        Debug.Log("test");
        //        boss.CheckHealthAndChangeState(); // ü�� üũ �� ���� ����
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Normal State");
    }
    private IEnumerator SomeCoroutine(Boss boss)
    {
        yield return null;

        yield return new WaitForSeconds(4f);
        //boss.transform.LookAt(boss.transform.position);
        DangerMarkerShoot();

        yield return new WaitForSeconds(2f);
       /**//* Shoot();*/
    }
    void DangerMarkerShoot()
    {
        /*Vector3 NewPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        Physics.Raycast(NewPosition, transform.forward, out RaycastHit hit, 30f, layerMask);

        if (hit.transform.CompareTag("Wall"))
        {
            GameObject DangerMarkerClone = Instantiate(DangerMarker, NewPosition, transform.rotation);
            DangerMarkerClone.GetComponent<DangerLine>().EndPosition = hit.point;
        }*/
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
        boss.GimikAgain(); // ���� ���� �� ������ ����
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
        boss.GimikAgain(); // ���� ���� �� ������ ����
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
        boss.GimikAgain(); // ���� ���� �� ������ ����
    }
}