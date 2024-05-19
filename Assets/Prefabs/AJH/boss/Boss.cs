using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;

//GimikAgain(); ��� �����
//��  Danger ��� : �� ������Ʈ�� Wall Layer�߰��� �����, �����ʿ��� �׳� ���̶�� �ǴܵǴ°� �� Wall ���̾� ���� �ؾ���
// public string[] playerTags3 = { "Player2", "Player3", "Player4","Player5" }; ���� ������ ���� �� �±׵��� �÷��̾ �� �߰� �Ǿ��־�� ��
// ������ ���� ��   [SerializeField] public bool bosssRoomStartCheck; true�� �����ϴ� ���� ���� �� �־����

/*  �������̽�  */
public interface IBossState
{
    void Enter(Boss boss);                                          // ���¿� ������ �� ȣ��Ǵ� �޼���
    void Execute(Boss boss);                                        // ���°� Ȱ��ȭ�� ���� �� ������ ȣ��Ǵ� �޼���
    void Exit(Boss boss);                                           // ���¿��� ���� �� ȣ��Ǵ� �޼���
}
public class Boss : MonoBehaviour
{
/*  ����  */
    [SerializeField] public bool bosssRoomStartCheck;
    
    public List<GameObject> players = new List<GameObject>();
    public string[] playerTags5 = { "Player2", "Player3", "Player4","Player5" };   

    
    private IBossState currentState;                                // ���� ����
    private IBossState previousState;                               // ���� ����

    public List<Vector3> setDangerPosition = new List<Vector3>();   // ���1 ���ݹ��� ���� ��ó����
    
    public float Health { get; private set; }                       // ������ ü��
    public float GimmickThreshold1 { get; private set; }            // ��� �Ӱ谪 1
    public float GimmickThreshold2 { get; private set; }            // ��� �Ӱ谪 2
    public float GimmickThreshold3 { get; private set; }            // ��� �Ӱ谪 3
    private Animator animator;                                      // �ִϸ�����
    public bool IsUsingLaser { get; private set; }                  // ������ ��� ����

/*  �ʱ�ȭ  */
    void Start()
    {
        bosssRoomStartCheck = false;                                // ������ Ȱ���� �ڵ����� �ϰ� ���� �ʵ��� �ʱ�ȭ 
        Health = 100f;        
        GimmickThreshold1 = 30f;                                    // ü�� �Ӱ谪 ����
        GimmickThreshold2 = 50f;
        GimmickThreshold3 = 70f;
        animator = GetComponent<Animator>();
        ChangeState(new NoState());                                 // �ʱ� ���¸� Normal ���·� ����
    }

    void Update()
    {
        currentState?.Execute(this);                                // �� ������ ���� ������ Execute �޼��� ����
    }


    public void ChangeState(IBossState newState)
    {
        previousState = currentState;                               // ���� ���� ����
        currentState?.Exit(this);                                   // ���� ���� ����
        currentState = newState;                                    // ���ο� ���� ����
        currentState.Enter(this);                                   // ���ο� ���� ����
    }

/*  ���� �ִϸ��̼� ����(����)  */
    public void SetAnimation(string animationName)
    {
        animator.Play(animationName);                               // ������ �ִϸ��̼� ���
    }
     
/*  �ִϸ��̼� �̺�Ʈ ȣ�� �Լ�(����)  */
    public void OnGimmickAnimationEvent(string eventName)
    {
        if (eventName == "Stage1_start")
        {
            if (currentState is Stage1 stage1)
            {
                // 1-1. �ڷ�ƾ ������ ���� ����  �ڷ�ƾ �Լ��� Start bossCoroutine���� �ѱ�
                StartBossCoroutine(stage1.IDangerStart(this));
            }
        }
        else if (eventName == "Stage1_end")
        {
            if (currentState is Stage1 stage1)
            {
                StartBossCoroutine(stage1.IDangerEnd(this));
            }
        }
        else if(eventName == "StartLaser")
        {
            StartLaser(GameObject.FindGameObjectWithTag("Player").transform.position); // ������ ����
        }
        else if (eventName == "ThrowRock")
        {
            ThrowRock(GameObject.FindGameObjectWithTag("Player").transform.position); // ���� ������
        }
        else if (eventName == "AnimationComplete")
        {
            SetAnimation("Idle");                                                     // �ִϸ��̼� �Ϸ� �� Idle �ִϸ��̼� ����
        } 
    }
/*  �ڷ�ƾ ����, ���� (����)  */

    // 1-2 �Ű������� ���� �ڷ�ƾ�� ����
    public void StartBossCoroutine(IEnumerator coroutine)
    {
        Debug.Log("test2");
        StartCoroutine(coroutine);
    }

    public void StopBossCoroutine(IEnumerator coroutine)
    {
        StopCoroutine(coroutine);
    }

/*  ���� ���� üũ (����)  */
    public void CheckHealthAndChangeState()
    {
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
    }
/*  ���2 ����  */
    #region #Stage2
    public void StartLaser(Vector3 position)
    {
        Debug.Log("Starting Laser at Position: " + position);
        IsUsingLaser = true;                                       // ������ ��� ���� ����
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
        Debug.Log("Firing Laser");                                  // ������ �߻� �α�
        // ������ �߻� ���� ����
    }

    public void GimikAgain()
    {
        Debug.Log("Stopping Laser");
        IsUsingLaser = false;                                        // ������ ��� ���� ����
        // ������ ���� ���� ����

                                                                     // ���� ����� �ٽ� �����ϱ� ���� �ڷ�ƾ ����
        StartCoroutine(WaitAndReExecuteGimmick());
    }

    private IEnumerator WaitAndReExecuteGimmick()
    {
        IBossState currentGimmickState = currentState;               // ���� ���� ����
        float waitTime = 10f;                                        // ��� �ð� ���� (�� ����)
        Debug.Log("Waiting for " + waitTime + " seconds before re-executing the gimmick.");
        yield return new WaitForSeconds(waitTime);                   // ������ �ð� ���

                        
        if (currentState == currentGimmickState)                     // ���°� ������� �ʾ��� ��쿡�� ���� ��� ���¸� �ٽ� ����
        {
            ChangeState(currentState);                               // ���� ���·� �ٽ� ����
        }
    }

    public void ThrowRock(Vector3 targetPosition)
    {
        Debug.Log("Throwing Rock at Position: " + targetPosition);
        // ���� ������ ���� ����
    }
    #endregion

}

/*  ���� ��� ����  */
public class NoState : IBossState
{
    public void Enter(Boss boss)
    {
        boss.SetAnimation("Idle1");
    }

    public void Execute(Boss boss)
    {
        if (boss.bosssRoomStartCheck) boss.ChangeState(new Stage1());
    }

    public void Exit(Boss boss)
    {
    }
}

/*  ���� Stage1 Class  */
public class Stage1 : IBossState
{
    private GameObject activeDangerLine;


    public void Enter(Boss boss)
    {
        foreach (string tag in boss.playerTags5)
        {

            GameObject player = GameObject.FindGameObjectWithTag(tag);
            if (player != null)
            {
                boss.players.Add(player);
            }
            
        }
        boss.SetAnimation("Stage1"); // Idle �ִϸ��̼� ����
        //boss.StartBossCoroutine(SomeCoroutine(boss)); // �ڷ�ƾ ����
    }

    public void Execute(Boss boss)
    {
        Debug.Log("test");
        //boss.CheckHealthAndChangeState(); // ü�� üũ �� ���� ����
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Exiting Normal State");
    }
   
    // 1-3 ��ŸƮ �ڷ�ƾ���� ������ �������� ���� ��
    public IEnumerator IDangerStart(Boss boss)
    {
        yield return null;
        boss.transform.LookAt(boss.players[Random.Range(0, boss.players.Count)].transform);
        DangerLineStart(boss); 
    } 

    // 1-4 �ڷ�ƾ�� ���ư��� �Ʒ� �Լ��� ���� ��
    void DangerLineStart(Boss boss)
    { 
        foreach (GameObject player in boss.players)
        {
            if (player != null)
            {
                GameObject activeDangerLine = PoolManager.Instance.GetPoolObject(PoolObjectType.DangerLine);

                DangerLine dangerLineComponent = activeDangerLine.GetComponent<DangerLine>();
              
                if (dangerLineComponent != null)
                {
                    Vector3 direction = (player.transform.position - boss.transform.position).normalized;
                    float extendLength = 5f;
                    Vector3 extendedEndPosition = player.transform.position + direction * extendLength;
                    boss.setDangerPosition.Add(extendedEndPosition);
                    dangerLineComponent.EndPosition = extendedEndPosition;
                    activeDangerLine.transform.position = boss.transform.position;
                    activeDangerLine.SetActive(true);
                    boss.StartBossCoroutine(ReturnDangerLineToPool(activeDangerLine, dangerLineComponent.GetComponent<TrailRenderer>().time));

                }
            }
        }
    }
    // DangerLine Ǯ ��ȯ �Լ� ����
    public IEnumerator ReturnDangerLineToPool(GameObject dangerLine, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.Instance.CoolObject(dangerLine, PoolObjectType.DangerLine);
        dangerLine.SetActive(false);
        DangerLine dangerLineComponent = dangerLine.GetComponent<DangerLine>();
        if (dangerLineComponent != null)
        {
            dangerLineComponent.cleartr(); // Call the method to clear the TrailRenderer
        }
    }


    public IEnumerator IDangerEnd(Boss boss)
    {
       DangerLineEnd(boss);

        yield return null;

    }
    void DangerLineEnd(Boss boss)
    {
        foreach (Vector3 setDangerPositions in boss.setDangerPosition)
        {
            if (setDangerPositions != null)
            {
                GameObject activeDangerAttack = PoolManager.Instance.GetPoolObject(PoolObjectType.DangerAttack);
                //DangerLine dangerLineComponent = activeDangerLine.GetComponent<DangerLine>();

                //if (dangerLineComponent != null)
                //{
                activeDangerAttack.transform.position = boss.transform.position;
                activeDangerAttack.transform.LookAt(setDangerPositions);
                Vector3 eulerAngles = activeDangerAttack.transform.rotation.eulerAngles;
                eulerAngles.x = 0;

                // Apply the modified rotation
                activeDangerAttack.transform.rotation = Quaternion.Euler(eulerAngles);
                activeDangerAttack.SetActive(true);
                    //boss.StartBossCoroutine(ReturnDangerLineToPool(activeDangerLine, dangerLineComponent.GetComponent<TrailRenderer>().time));

                //}
            }
        }
    }
}

/*  ���� ����On Stage2  */
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

/*  ���� ����On Stage3  */
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

/*  ���� ����On Stage4  */
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