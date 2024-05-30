using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DataManager;

public class NetworkPlayerController : MonoBehaviour
{
    public GameObject skillControlObject;
    public SkillControlNetwork skill;
    public Animator _animator;

    private CharacterController _characterController;
    private Vector3 _moveDirection;              // �÷��̾��� �̵� ����
    private bool _isRunning = false;             // �÷��̾ �޸��� �ִ��� ���θ� �����ϴ� �÷���
    private int _skillA = -1;                    // ��ų A ����ó�� ���� ���� ��ų
    private int _skillB = -1;                    // ��ų B �پ �ٸ��콺ó�� ��½�ų 
    public bool isAction = false;                // �÷��̾ �׼��� ���� ������ ���θ� �����ϴ� �÷���
    private float _gravity = -9.81f;             // �߷� ���ӵ�
    private float _velocity;                     // �÷��̾��� ���� �ӵ�
    private static string MyObjectName;          // �÷��̾� ������Ʈ �̸�
    private static string _PlayerName;           // �÷��̾� �̸�  
    private static int _hp;                      // �÷��̾� ü��
    private static int _level;                   // �÷��̾� ����
    private static int _str;                     // �÷��̾� ��
    private static bool isSkillACooldown = false;// ��ų A ��ٿ� ���θ� �����ϴ� �÷���
    private static bool isSkillBCooldown = false;// ��ų B ��ٿ� ���θ� �����ϴ� �÷���
    public float dashCooldownDuration = 5f;      // ��� ��ٿ� ���� �ð�(��)
    private bool isDashCooldown = false;         // ��� ��ٿ� ���¸� �����ϴ� �÷�
    FloatingHealthBar healthBar;
    [SerializeField]
    private Collider WeaponCollider;             // ���� �ݶ��̴�
    private GameObject attack;

    [SerializeField]
    private Canvas _hpCanvas;

    [SerializeField] private PlayerAttackSound playerSound;

    private CinemachineVirtualCamera virtualCamera;

    
    [SerializeField]
    private float speed = 3.5f;

    [SerializeField]
    private float rotationSpeed = 1.5f;

    private Vector3 oldInputPosition;
    private Quaternion oldInputRotation;

    public ParticleSystem skillAEffect;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        
    }


    private void Start()
    {
        //if (IsClient && IsOwner)
        //{
        //    if (transform.Find("EffectParents").gameObject != null)
        //    {
        //        attack = transform.Find("EffectParents").gameObject; // �� �÷��̾� ������Ʈ ������ ����Ʈ �θ� ������Ʈ�� ã���ϴ�.
        //    }
        //    if (skillControlObject != null)
        //    {
        //        skill = skillControlObject.GetComponent<SkillControlNetwork>();
        //    }
        //    // ��ų ��ٿ��� �����ϴ� �ڷ�ƾ ����
        //    StartCoroutine(SkillCooldown());
        //}
    }
    void Update()
    {
       
    }


    IEnumerator SkillCooldown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);     // 1�ʸ��� üũ    


            if (isSkillACooldown)                    // ��ų A ��ٿ� ���� Ȯ�� �� ó��
            {
                yield return new WaitForSeconds(5f); //5�ʰ� ���
                isSkillACooldown = false;            // ��ٿ� ����
            }


            if (isSkillBCooldown)                    // ��ų B�� ��ٿ��� Ȱ��ȭ�Ǿ� �ִ� ���
            {
                yield return new WaitForSeconds(5f); // 5�ʰ� ���
                isSkillBCooldown = false;            // ��ٿ� ����
            }
        }
    }

    // �÷��̾� ������ ���� �Լ�
    //private static void SetPlayerData(PlayerData playerData)
    //{
    //    _PlayerName = playerData.name;
    //    _hp = playerData.hp;
    //    _level = playerData.level;
    //    _str = playerData.str;
    //}

    //private void HandleInput()
    //{
    //    if (Input.GetKeyDown(KeyCode.LeftShift))
    //    {
    //        if (IsOwner)
    //            DashServerRPC();
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        if (IsOwner)
    //            SkillAServerRPC();
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        if (IsOwner)
    //            SkillBServerRPC();
    //    }
    //    if (Input.GetKeyDown(KeyCode.Mouse0))
    //    {
    //        if (IsOwner)
    //            ClickServerRPC();
    //    }
    //    if (Input.GetKeyDown(KeyCode.Mouse1))
    //    {
    //        if (IsOwner)
    //            SkillClickServerRPC();
    //    }

    //}

    // �÷��̾ ���ظ� ���� �� ȣ��Ǵ� �Լ�

    public void TakeDamage(int damageAmout)
    {
        //Debug.Log($"���� ����!!! Current Hp : {_hp}");
        _hp -= damageAmout;
        if (_hp <= 0)
        {

            _animator.SetTrigger("Die");

        }
        else
        {
            _animator.SetTrigger("hitCharacter");
        }
    }

    // �߷��� �����ϴ� �Լ�
    void ApplyGravity()
    {

        if (!_characterController.isGrounded) // �߷��� �����մϴ�.
        {
            _velocity += _gravity * Time.deltaTime;
        }
        else
        {
            _velocity = 0f;
        }
        //_moveDirection.y = _velocity; // ���� �̵��� �����մϴ�.
    }

    #region SEND_MESSAGE

    void Move(Vector3 movementInput)
    {
        // �Է� ���� ���� ��������
        Vector3 movement = new Vector3(movementInput.x, 0f, movementInput.z);
        //movement.y = 0f;
        _isRunning = movement.magnitude > 0;

        bool hasControl = (movement != Vector3.zero);
        if (hasControl)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            _characterController.Move(movement * Time.deltaTime);// ĳ���͸� �̵���ŵ�ϴ�.
            _animator.SetBool("isRunning", _isRunning);// �ٱ� ���¸� �����մϴ�.
        }
        else
        {
            _animator.SetBool("isRunning", false); // �̵����� ���� ���� �ٱ� ���� ����
        }
    }

    public void Dash()
    {

        //if (!skill.isHideSkills[0])
        //{
        //    skill.HideSkillSetting(0);
        //    return;
        //}

        if (skill.getSkillTimes[0] > 0)
            return;

        Vector3 dashDirection = transform.forward; // �÷��̾ ���� �ִ� �������� ���
        playerSound.Dash();//��� ���� ���
        float dashDistance = 5f;  // ��� �Ÿ�
        float dashDuration = 0.2f; // ��� ���� �ð�

        // ��� ������ ��ġ ���
        Vector3 dashDestination = transform.position + dashDirection * dashDistance;

        // �÷��̾��� ��ġ�� ������ �̵��Ͽ� ��� ����
        StartCoroutine(MovePlayerToPosition(transform.position, dashDestination, dashDuration));

        skill.isHideSkills[0] = true;
        skill.getSkillTimes[0] = skill.skillTimes[0];
    }

    // �÷��̾ ��� ������ ��ġ�� �ε巴�� �̵���Ű�� �ڷ�ƾ
    IEnumerator MovePlayerToPosition(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;// �÷��̾ ��Ȯ�� ��ġ�� �����ϵ��� ����
    }

    
   
    public void attackEvent(string type)
    {
        if (attack != null)
        {
            GameObject effect = attack.transform.Find($"attack{type}").gameObject;
            if (effect.activeSelf)
            {
                effect.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                effect.SetActive(true);
            }
        }
    }

    //��ý�ų Shift ���� ���濹��


    public void SkillA()
    {
        //if (!skill.isHideSkills[1])
        //{
        //    skill.HideSkillSetting(1);
        //    return;
        //}
        _animator.SetInteger("skillA", 0);// ��ų A �ִϸ��̼� ���
        _animator.Play("ChargeSkillA_Skill");
        playerSound.SkillA(); // ��ų A ���� ���
       // SkillAEffectFunction();
    }

    public void SkillB()
    {
        //if (!skill.isHideSkills[2])
        //{
        //    skill.HideSkillSetting(2);
        //    return;
        //}
        if (skill.getSkillTimes[2] > 0) return;
        StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f)); 
        playerSound.SkillB();// ��ųB ���� ���
    }

    public void Click()
    {
        _animator.SetTrigger("onWeaponAttack");
        playerSound.BaseAttack();//�⺻ ���� ���� ���(���)
    }

    public void SkillClick()
    {
        _animator.SetTrigger("onWeaponAttack");

    }

    IEnumerator ActionTimer(string actionName, float time)
    {
        isAction = true;

        if (actionName != "none") _animator.Play(actionName);

        yield return new WaitForSeconds(time);
        isAction = false;
    }
    public void EnableWeapon()
    {
        WeaponCollider.enabled = true;
    }
    public void DisableWeapon()
    {
        WeaponCollider.enabled = false;
    }
    #endregion

    
}
