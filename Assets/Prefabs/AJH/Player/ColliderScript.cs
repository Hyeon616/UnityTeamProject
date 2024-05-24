using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ColliderScript : MonoBehaviour
{
    public event Action<Collider> OnTriggerEnterEvent;  // ���콺 Ŭ�� �� �޺� ���� �̺�Ʈ ����
    public event Action<Collider> SkillTriggerA;        // ��ų A �̺�Ʈ ����
    
    
    private void OnTriggerEnter(Collider other)         // ��ų,�޺� ���� �ϴ� ���Ϳ� �浹���� �̺�Ʈ �߻�
    {
        //Debug.Log("���� üũ2");
        if(other != null && other.name != "player")
        {
            OnTriggerEnterEvent?.Invoke(other);
            SkillTriggerA?.Invoke(other);
            
        }
    }
}