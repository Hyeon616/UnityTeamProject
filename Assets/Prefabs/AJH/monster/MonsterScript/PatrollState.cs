using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class PatrollState : StateMachineBehaviour
{
    float timer;
    float randomTime;

    //[�߰�] - �߰� ���·� ��ȯ�� �� �÷��̾��� ��ġ�� �ʿ��ؼ� �÷��̾��� Transform�� ���� ���� ����
    //�ٵ� ���߿� ���� ���̰� �ϸ� �÷��̾� �����ϸ� �±� �Ȱ�ġ�� �ؾ� �Ҽ��� ����.
    Transform player;
    Transform WayPoint;
    float chaseRange = 8;

    List<Transform> wayPoints = new List<Transform>();
    NavMeshAgent agent;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //[�߰�] - �÷��̾� transform �� �޾ƿ�
        player = GameObject.FindGameObjectWithTag("Player").transform;
        MonsterInfo info = animator.gameObject.GetComponent<MonsterInfo>();
        WayPoint = info.wayPoint;
        agent = animator.GetComponent<NavMeshAgent>();
        timer = 0;
        randomTime = Random.Range(10f, 15f);

        foreach (Transform t in WayPoint)
            wayPoints.Add(t);

        agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //��ǥ ����
        if (agent.remainingDistance <= agent.stoppingDistance)
            agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);

        timer += Time.deltaTime;
        if (timer > randomTime)
            animator.SetBool("isPatrolling", false);

        //[�߰�] -  ���Ϳ� �÷��̾��� �Ÿ��� vector3.Distance�� ����ϰ� chaseRange�̸��̸� �߰ݻ��·� ����
        float distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance < chaseRange)
            animator.SetBool("isChasing", true);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(agent.transform.position);
    }

}
