using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ColliderScript : MonoBehaviour
{
/* 1. �̺�Ʈ ���� */
    // ���콺 Ŭ�� �� �޺� ���� �̺�Ʈ ����
    public event Action<Collider> OnTriggerEnterEvent;

    // 1�� ��ų �̺�Ʈ ����
    public event Action<Collider> SkillTriggerA;

/* 2. ��ų,�޺� ���� �ϴ� ���Ϳ� �浹���� �̺�Ʈ �߻�*/
    private void OnTriggerEnter(Collider other)
    {
        if(other != null && other.name != "player")
        {
            OnTriggerEnterEvent?.Invoke(other);
            SkillTriggerA?.Invoke(other);
        }

    }
}