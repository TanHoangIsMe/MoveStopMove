using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float moveRadius;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private Rigidbody rb;

    private State currentState = State.Idle;

    private void Start()
    {
        InvokeRepeating(nameof(MoveToRandomPosition), 0f, 3f); 
    }

    private void FixedUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                if (currentState != State.Idle)
                {
                    // play idle animation
                    animationController.PlayIdleAnimation();
                    currentState = State.Idle;
                }
            }
        }
    }

    private void MoveToRandomPosition()
    {
        Vector3 randomPos = RandomNavSphere(transform.position, moveRadius, -1);
        agent.SetDestination(randomPos);
        randomPos = randomPos.normalized;
        rb.transform.rotation = Quaternion.LookRotation(randomPos);

        if(currentState != State.Run)
        {
            // play run animation
            animationController.PlayRunAnimation();
            currentState = State.Run;
        }
    }

    private Vector3 RandomNavSphere(Vector3 origin, float radius, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += origin;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, radius, layermask);
        return hit.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && currentState == State.Idle)
        {
            animationController.PlayAttackAnimation();
            currentState = State.Attack;
        }
    }
}
