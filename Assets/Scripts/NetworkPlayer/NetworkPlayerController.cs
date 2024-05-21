using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static DataManager;

public class NetworkPlayerController : NetworkBehaviour
{
    public GameObject skillControlObject;
    public SkillControlNetwork skill;
    public Animator _animator;

    private CharacterController _characterController;
    //private Vector3 _moveDirection;              // �÷��̾��� �̵� ����
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

    [SerializeField]
    private Canvas _hpCanvas;

    private CinemachineVirtualCamera virtualCamera;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {

            transform.position = new Vector3(-17, 1, -154);

            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

            if (virtualCamera != null)
            {
                // Virtual Camera�� Follow �� Look At �ʵ带 ���� �÷��̾�� �����մϴ�
                virtualCamera.Follow = transform;
            }
        }

    }

    private void Awake()
    {

        _characterController = GetComponent<CharacterController>();
        MyObjectName = gameObject.name;          // �÷��̾� ������Ʈ�� �̸� ��������
                                                 // DataManager�� ����Ͽ� �÷��̾� ������ ��������
        _animator = GetComponent<Animator>();
    }

    void Start()
    {

        GameObject hpObject = Instantiate(PrefabReference.Instance.hpBarPrefab);
        hpObject.transform.SetParent(_hpCanvas.transform);
        healthBar = hpObject.GetComponentInChildren<FloatingHealthBar>();
        healthBar.SetTarget(transform);

        if (skillControlObject != null)
        {
            skill = skillControlObject.GetComponent<SkillControlNetwork>();
        }

        StartCoroutine(SkillCooldown());         // ��ų ��ٿ��� �����ϴ� �ڷ�ƾ ����

    }
    // ��ų ��ٿ��� �����ϴ� �ڷ�ƾ
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
    private static void SetPlayerData(PlayerData playerData)
    {
        _PlayerName = playerData.name;
        _hp = playerData.hp;
        _level = playerData.level;
        _str = playerData.str;
    }

    void Update()
    {
        if (!IsOwner) return;
        //Vector2 movementInput = playerControls.Player.Move.ReadValue<Vector2>();
        if (IsServer && IsLocalPlayer)
        {

            Move();
            ApplyGravity();
            if (isAction) return; //�������̰ų� 2����ų �ߵ����� �� ĳ���̵� X�ϱ� ���� return
            Dash();
            SkillA();
            SkillB();
            Click();
            SkillClick();
        }
        else if (IsLocalPlayer)
        {
            MoveServerRPC();
            DashServerRPC();
            SkillAServerRPC();
            SkillBServerRPC();
            ClickServerRPC();
            SkillClickServerRPC();
            ApplyGravityServerRPC();
        }
    }

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

    void Move()
    {

        // �Է� ���� ���� ��������
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        movement.y = 0f;
        _isRunning = movement.magnitude > 0;

        bool hasControl = (movement != Vector3.zero);
        if (hasControl)
        {

            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            _characterController.Move(movement * 6f * Time.deltaTime);// ĳ���͸� �̵���ŵ�ϴ�.
            _animator.SetBool("isRunning", _isRunning);// �ٱ� ���¸� �����մϴ�.
        }
        else
        {
            _animator.SetBool("isRunning", false); // �̵����� ���� ���� �ٱ� ���� ����
        }


    }

    public void Dash()
    {

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!skill.isHideSkills[0])
            {
                skill.HideSkillSetting(0);
                return;
            }

            if (skill.getSkillTimes[0] > 0)
                return;
            Vector3 dashDirection = transform.forward; // �÷��̾ ���� �ִ� �������� ���
            float dashDistance = 5f;  // ��� �Ÿ�
            float dashDuration = 0.2f; // ��� ���� ��

            // ��� ������ ��ġ ���
            Vector3 dashDestination = transform.position + dashDirection * dashDistance;

            // �÷��̾��� ��ġ�� ������ �̵��Ͽ� ��� ����
            StartCoroutine(MovePlayerToPosition(transform.position, dashDestination, dashDuration));

        }

        // ���⿡ ��� �ִϸ��̼� ����� ���� �߰� �۾��� �߰� ����
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

    //��ý�ų Shift ���� ���濹��

    public void SkillA()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!skill.isHideSkills[1])
            {
                skill.HideSkillSetting(1);
                return;
            }
            _animator.SetInteger("skillA", 0);// ��ų A �ִϸ��̼� ���
            _animator.Play("ChargeSkillA_Skill"); // ��ų A ���� �ִϸ��̼� ���
        }
    }

    public void SkillB()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!skill.isHideSkills[2])
            {
                skill.HideSkillSetting(2);
                return;
            }
            if (skill.getSkillTimes[2] > 0) return;
            StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f));
        }
    }

    public void Click()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            _animator.SetTrigger("onWeaponAttack");
    }

    public void SkillClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
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

    [ServerRpc]
    private void MoveServerRPC()
    {
        Move();
    }

    [ServerRpc]
    private void DashServerRPC()
    {
        Dash();
    }

    [ServerRpc]
    private void SkillAServerRPC()
    {
        SkillA();
    }
    [ServerRpc]
    private void SkillBServerRPC()
    {
        SkillB();
    }
    [ServerRpc]
    private void ClickServerRPC()
    {
        Click();
    }

    [ServerRpc]
    private void SkillClickServerRPC()
    {
        SkillClick();
    }
    [ServerRpc]
    private void ApplyGravityServerRPC()
    {
        ApplyGravity();
    }
}
