using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using UnityEngine.Rendering.Universal;
/****************************************************************************** 
 [ �ڵ� ���� ��� ���� ]
 - ��ų �� ����Ʈ�� StartBossCoroutine(stage1.IDangerStart(this), 10f) �̷������� ����� �ڷ�ƾ�� ����ð��� ���� �Ѱܼ� �ڷ�ƾ�� ����� �� �ְ� ó��
 - GimikAgain(); ����� ����� �ϴ� �Լ�
 - Danger ���� ���ӽð��� DangerLine ������ �ν����Ϳ��� Time ������ ����

 [ �� �� ��ġ�� �����ؾ� �� �׸�� ���� �� ]
 - �����ʿ��� ������ ��ź�Ǵ� ������Ʈ�� "Wall" ���̾� ���� (���� ����� �� ��)
 - public string[] playerTags3 = { "Player2", "Player3", "Player4","Player5" }; ���� ������ ���� �� �±׿� �°� ���̾��Ű �÷��̾� �±׸� �����ؾ� ��  
 - ������ ���� ��  [SerializeField] public bool bosssRoomStartCheck; true�� �����ϴ� ���� ���� �� �־����
 - ���� ������Ʈ�� Boss �±� �ޱ�
  
****************************************************************************** */

/*  �������̽�  */
public interface IBossState
{
   
    void Enter(Boss boss);                                             // ���¿� ������ �� ȣ��Ǵ� �޼���
    void Execute(Boss boss);                                           // ���°� Ȱ��ȭ�� ���� �� ������ ȣ��Ǵ� �޼���
    void Exit(Boss boss);                                              // ���¿��� ���� �� ȣ��Ǵ� �޼���
    string ToString();
}

/*  ����  */
public class Boss : MonoBehaviour
{
    [SerializeField] public bool bosssRoomStartCheck;
    [SerializeField] public float currentHealth { get; private set; }                   // ���� ü�� 

    public List<GameObject> players = new List<GameObject>();
    public string[] playerTags5 = { "Player2", "Player3", "Player4", "Player5" ,"Player"};

    private IBossState currentState;                                   // ���� ����
    public string previousState;
    public List<Vector3> setDangerPosition = new List<Vector3>();      // ���1 : ���Ͱ� �����ϴ� ���� ������ ����Ʈ�� �־�� (��ų �� �� ������ ���ư��� �ؼ�)
    public float fixHealth { get; private set; }                       // ���� ü�� ����
    public int Stage2Hp { get; private set; }               // ��� �Ӱ谪 1
    public int Stage3Hp { get; private set; }               // ��� �Ӱ谪 2
    public int Stage4Hp { get; private set; }               // ��� �Ӱ谪 3
    private Animator animator;                                         // �ִϸ�����
    public bool IsUsingLaser { get; private set; }                     // ������ ��� ����

    private List<Coroutine> runningCoroutines = new List<Coroutine>(); // Running coroutine references!!!!

/*  �ʱ�ȭ  */
    void Start()
    {
        bosssRoomStartCheck = false;                                   // ������ Ȱ���� �ڵ����� �ϰ� ���� �ʵ��� �ʱ�ȭ 
        fixHealth = 10000f;
        currentHealth = fixHealth;
        Stage2Hp = 90;                                       // ü�� �Ӱ谪 ����
        Stage3Hp = 70;
        Stage4Hp = 30;
        animator = GetComponent<Animator>();
        ChangeState(new NoState());                                    // �ʱ� ���¸� Normal ���·� ����
    }

    void Update()
    {
        currentState?.Execute(this);                                   // �� ������ ���� ������ Execute �޼��� ����
    }

    public void ChangeState(IBossState newState)
    {
        previousState = currentState?.ToString();                      // ���� ���¸� ���ڿ��� ����

        currentState?.Exit(this);                                      // ���� ���� ����
        currentState = newState;                                       // ���ο� ���� ����
        currentState.Enter(this);                                      // ���ο� ���� ����
    }
    /*  ���� ü�� ���� �ý���  */
    public void TakeDamage(int damageAmout)
    {
        currentHealth -= damageAmout;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
        }
        else
        {
            Debug.Log("���� ���� ���ϴ���");
        }
    }
    /*  ���� �ִϸ��̼� ����(����)  */
    public void SetAnimation(string animationName)
    {
        animator.Play(animationName);                                  // ������ �ִϸ��̼� ���
    }
/*  �ִϸ��̼� �̺�Ʈ ȣ�� �Լ�(����)  */
    public void OnGimmickAnimationEvent(string eventName)
    {
        if (eventName == "Stage1_start")
        {
            if (currentState is Stage1 stage1)
            {   // 1-1. �ڷ�ƾ ������ ���� ���� �ڷ�ƾ �Լ��� StartBossCoroutine���� �ѱ�
                StartBossCoroutine(stage1.IDangerStart(this), 3f);     // �ڷ�ƾ ���� �� 2�� �� ����
            }
        }
        else if (eventName == "Stage1_end")
        {
            if (currentState is Stage1 stage1)
            {   // 2-1. �ڷ�ƾ ������ ���� ���� �ڷ�ƾ �Լ��� StartBossCoroutine���� �ѱ�
                StartBossCoroutine(stage1.IDangerEnd(this), 5f);       // �ڷ�ƾ ���� �� 2�� �� ����
            }
        }
        else if (eventName == "StartLaser")
        {
            StartLaser(GameObject.FindGameObjectWithTag("Player").transform.position); // ������ ����
        }
        else if (eventName == "ThrowRock")
        {
            ThrowRock(GameObject.FindGameObjectWithTag("Player").transform.position); // ���� ������
        }
        else if (eventName == "AnimationComplete")
        {
            SetAnimation("Idle");                                        // �ִϸ��̼� �Ϸ� �� Idle �ִϸ��̼� ����
        }
    }

/*  �ڷ�ƾ(����)  */
    // 1-2 , 2-2
    public void StartBossCoroutine(IEnumerator coroutine, float stopAfterSeconds)
    {   // ���⼭ ���۽�Ŵ
        Coroutine startedCoroutine = StartCoroutine(coroutine);
        // �����Ų �ڷ�ƾ��? �迭�� �߰���Ŵ
        runningCoroutines.Add(startedCoroutine);
        // ������ ������״� �ڷ�ƾ�� ����Ÿ�̸� ������Ŵ
        StartCoroutine(StopCoroutineAfterDelay(startedCoroutine, stopAfterSeconds));
    }

    private IEnumerator StopCoroutineAfterDelay(Coroutine coroutine, float delay)
    {
        // ����Ÿ�̸� �ð���ŭ ��ٸ�
        yield return new WaitForSeconds(delay);
        // �ڷ�ƾ ����
        StopCoroutine(coroutine);
        runningCoroutines.Remove(coroutine);
        Debug.Log("Coroutine stopped after " + delay + " seconds");
    }

    /*  ���� ���� üũ (����)  */
    public void CheckHealthAndChangeState(bool chk)
    {          
        switch (previousState) //���� �ν��Ͻ��� �ƴ� ���� �ν��Ͻ��� üũ
        {
            case ("NoState"):
                {
                    if (fixHealth <= Stage2Hp) // �� ���������� NoState �̰� ���� ü���� 90 ���Ϸ� ��Ҵٸ� Stage2�� �Ѿ �� ����
                    {
                        ChangeState(new Stage2()); // ���� ���� �Ӱ谪���� üũ
                    }
                    else
                    {

                        if (chk) {
                            Debug.Log("���¸� ������ؿ�..");
                            ChangeState(new Stage1());
                            
                                }
                    }
                    break;
                }
            case ("Stage1"):
                {
                    if (fixHealth <= Stage2Hp) // �� ���������� NoState �̰� ���� ü���� 90 ���Ϸ� ��Ҵٸ� Stage2�� �Ѿ �� ����
                    {
                        ChangeState(new Stage2()); // ���� ���� �Ӱ谪���� üũ
                    }
                    else
                    {

                        if (chk)
                        {
                            Debug.Log("���¸� ������ؿ�..");
                            ChangeState(new Stage1());

                        }
                    }
                    break;
                }
            case ("Stage2"):
                { 
                    if (fixHealth <= Stage3Hp) // �� ���������� Stage2 �̰� ���� ü���� 90 ���Ϸ� ��Ҵٸ� Stage3�� �Ѿ �� ����
                    {
                        ChangeState(new Stage3()); // ���� ���� �Ӱ谪���� üũ
                    }
                    else
                    {

                        /*if ("Ÿ��...ä������ Stage1����..") {
                            Debug.Log()
                        } */
                    }
                    break;
                }
        }

    }

/*  ���2 ����  */
    #region Stage2
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
    }

   

    public void ThrowRock(Vector3 targetPosition)
    {
        Debug.Log("Throwing Rock at Position: " + targetPosition);
        // ���� ������ ���� ����
    }
    #endregion
}

/* ���� ��� ���� */
public class NoState : IBossState
{
    public bool timer = false;
    public bool isChange = false;
    public void Enter(Boss boss)
    {
        Debug.Log("1");
        boss.SetAnimation("Idle1");
        if(boss.previousState != null) boss.StartBossCoroutine(changeClass(this,5f), 10f);
    }

    public void Execute(Boss boss)
    {
        Debug.Log("�⺻1ſ���");
        if (boss.bosssRoomStartCheck) boss.ChangeState(new Stage1());
        if (boss.previousState != null) boss.CheckHealthAndChangeState(isChange);
    }

    public void Exit(Boss boss)
    {
        
    }
    public override string ToString()
    {
        return "NoState";
    }
    public IEnumerator changeClass(NoState noteState, float endTime)
    {
        Debug.Log("Ÿ�̸� on...");
        yield return new WaitForSeconds(endTime);
        Debug.Log("5�� �������....");
        isChange = true;
    }
}

/* ���� Stage1 Class */
public class Stage1 : IBossState
{
    
    private GameObject activeDangerLine;

    public void Enter(Boss boss)
    {
        boss.bosssRoomStartCheck = false;
        foreach (string tag in boss.playerTags5)
        {
            GameObject player = GameObject.FindGameObjectWithTag(tag);
            if (player != null)
            {
                boss.players.Add(player);
            }
        }
        boss.SetAnimation("Stage1"); // Idle �ִϸ��̼� ����
    }

    public void Execute(Boss boss)
    {
    }

    public void Exit(Boss boss)
    {
        boss.players.Clear();
    }
    public override string ToString()
    {
        return "Stage1";
    }
    // 1-3 IDangerStart �ڷ�ƾ ����
    public IEnumerator IDangerStart(Boss boss)
    {
        boss.transform.LookAt(boss.players[Random.Range(0, boss.players.Count)].transform);
        DangerLineStart(boss);
        yield return null;
    }
    // 1-4 �ڷ�ƾ�� ���ư��� �Ʒ� �Լ��� ���� ��
    void DangerLineStart(Boss boss)
    {
        bool charge = true;
        foreach (GameObject player in boss.players)
        {
            if (player != null)
            {
                if (charge)
                {
                    
                    GameObject chargeEffect = PoolManager.Instance.GetPoolObject(PoolObjectType.DangerChage);   // charge Ǯ ������                    
                    chargeEffect.GetComponent<DangerCharge>().poolinfo = chargeEffect;                          // ������ Ǯ ����� ��ȯ�ϱ� ���� ������ Ǯ���� DangerCharge�� ����
                    chargeEffect.transform.position = boss.transform.position;                                  // ������ Ǯ ��ġ�� ���� ��ġ�� ����
                    chargeEffect.SetActive(true);                                                               // Pool On
                    charge = false;                                                                             // 1���� ����ϸ� ��
                }
                GameObject activeDangerLine = PoolManager.Instance.GetPoolObject(PoolObjectType.DangerLine);
                DangerLine dangerLineComponent = activeDangerLine.GetComponent<DangerLine>();

                if (dangerLineComponent != null)
                {
                    Vector3 direction = (player.transform.position - boss.transform.position).normalized;
                    direction.y = 0f; // y  �������� �ٴ��� �հ� �������� ������ �־ 0 ����

                    float extendLength = 5f;
                    Vector3 extendedEndPosition = player.transform.position + direction * extendLength;
                    boss.setDangerPosition.Add(extendedEndPosition);
                    dangerLineComponent.EndPosition = extendedEndPosition;
                    activeDangerLine.transform.position = boss.transform.position;
                    activeDangerLine.SetActive(true);
                    //���⼭�� �ڷ�ƾ ����ð� ���� �־ ����
                    boss.StartBossCoroutine(ReturnDangerLineToPool(activeDangerLine, dangerLineComponent.GetComponent<TrailRenderer>().time), dangerLineComponent.GetComponent<TrailRenderer>().time);
                }
            }
        }
    }

    public IEnumerator ReturnDangerLineToPool(GameObject dangerLine, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.Instance.CoolObject(dangerLine, PoolObjectType.DangerLine);
        dangerLine.SetActive(false);
        DangerLine dangerLineComponent = dangerLine.GetComponent<DangerLine>();
        if (dangerLineComponent != null)
        {
            dangerLineComponent.cleartr(); 
        }
    }

    // 2-3 
    public IEnumerator IDangerEnd(Boss boss)
    {
        DangerLineEnd(boss);
        yield return new WaitForSeconds(3f);
        boss.ChangeState(new NoState());
    }
    // 2-4
    void DangerLineEnd(Boss boss)
    {
        foreach (Vector3 setDangerPositions in boss.setDangerPosition)
        {
            if (setDangerPositions != null)
            {
                GameObject activeDangerAttack = PoolManager.Instance.GetPoolObject(PoolObjectType.DangerAttack);
                activeDangerAttack.transform.position = boss.transform.position;
                activeDangerAttack.transform.LookAt(setDangerPositions);
                Vector3 eulerAngles = activeDangerAttack.transform.rotation.eulerAngles;
                eulerAngles.x = 0;
                activeDangerAttack.transform.rotation = Quaternion.Euler(eulerAngles);
                activeDangerAttack.SetActive(true);
                
            }
        }
        boss.setDangerPosition.Clear();
    }
}

/* ���� ����On Stage2 */
public class Stage2 : IBossState
{
    

    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        boss.SetAnimation("Stage2"); // Gimmick1 �ִϸ��̼� ����
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
        boss.GimikAgain(); // ���� ���� �� ������ ����
    }
    public override string ToString()
    {
        return "Stage2";
    }
}

/* ���� ����On Stage3 */
public class Stage3 : IBossState
{
    

    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        boss.SetAnimation("Stage3"); // Stage3 �ִϸ��̼� ����
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // �÷��̾��� Transform ��������
    }

    public void Execute(Boss boss)
    {
        // �ʿ� �� �߰� ���� �߰�
    }

    public void Exit(Boss boss)
    {
        boss.GimikAgain(); // ���� ���� �� ������ ����
    }
    public override string ToString()
    {
        return "Stage3";
    }
}

/* ���� ����On Stage4 */
public class Stage4 : IBossState
{
    

    private Transform playerTransform; // �÷��̾��� Transform

    public void Enter(Boss boss)
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // �÷��̾��� Transform ��������
    }

    public void Execute(Boss boss)
    {
        // �ʿ� �� �߰� ���� �߰�
    }

    public void Exit(Boss boss)
    {
        boss.GimikAgain(); // ���� ���� �� ������ ����
    }
    public override string ToString()
    {
        return "Stage4";
    }
}