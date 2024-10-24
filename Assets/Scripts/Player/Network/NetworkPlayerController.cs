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
    private Vector3 _moveDirection;              
    private bool _isRunning = false;             
    private int _skillA = -1;                    
    private int _skillB = -1;                    
    public bool isAction = false;                
    private float _gravity = -9.81f;             
    private float _velocity;                     
    private static string MyObjectName;          
    private static string _PlayerName;           
    private static int _hp;                      
    private static int _level;                   
    private static int _str;                     
    private static bool isSkillACooldown = false;
    private static bool isSkillBCooldown = false;
    public float dashCooldownDuration = 5f;      
    private bool isDashCooldown = false;         
    FloatingHealthBar healthBar;
    [SerializeField]
    private Collider WeaponCollider;             
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

    IEnumerator SkillCooldown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);    


            if (isSkillACooldown)                   
            {
                yield return new WaitForSeconds(5f);
                isSkillACooldown = false;           
            }


            if (isSkillBCooldown)                   
            {
                yield return new WaitForSeconds(5f);
                isSkillBCooldown = false;           
            }
        }
    }


    public void TakeDamage(int damageAmout)
    {
        
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

    void ApplyGravity()
    {

        if (!_characterController.isGrounded)
        {
            _velocity += _gravity * Time.deltaTime;
        }
        else
        {
            _velocity = 0f;
        }
        
    }

    #region SEND_MESSAGE

    void Move(Vector3 movementInput)
    {
        Vector3 movement = new Vector3(movementInput.x, 0f, movementInput.z);
        
        _isRunning = movement.magnitude > 0;

        bool hasControl = (movement != Vector3.zero);
        if (hasControl)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            _characterController.Move(movement * Time.deltaTime);
            _animator.SetBool("isRunning", _isRunning);
        }
        else
        {
            _animator.SetBool("isRunning", false); 
        }
    }

    public void Dash()
    {
        if (skill.getSkillTimes[0] > 0)
            return;

        Vector3 dashDirection = transform.forward; 
        playerSound.Dash();
        float dashDistance = 5f;  
        float dashDuration = 0.2f; 

        
        Vector3 dashDestination = transform.position + dashDirection * dashDistance;

        StartCoroutine(MovePlayerToPosition(transform.position, dashDestination, dashDuration));

        skill.isHideSkills[0] = true;
        skill.getSkillTimes[0] = skill.skillTimes[0];
    }

    IEnumerator MovePlayerToPosition(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
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



    public void SkillA()
    {
        
        _animator.SetInteger("skillA", 0);
        _animator.Play("ChargeSkillA_Skill");
        playerSound.SkillA();
      
    }

    public void SkillB()
    {
        
        if (skill.getSkillTimes[2] > 0) return;
        StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f)); 
        playerSound.SkillB();
    }

    public void Click()
    {
        _animator.SetTrigger("onWeaponAttack");
        playerSound.BaseAttack();
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
