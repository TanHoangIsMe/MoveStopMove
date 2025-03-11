using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float moveRadius;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private WeaponController weapon;

    private State currentState = State.Idle;
    private bool canMove = true;
    private bool isRunning = false;
    private GameObject lockedTarget;

    private void OnEnable()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        agent.isStopped = false;

        InvokeRepeating(nameof(MoveToRandomPosition), 0f, 5f); 
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
        if (canMove)
        {
            Vector3 randomPos = RandomNavSphere(transform.position, moveRadius, -1);
            agent.SetDestination(randomPos);
            randomPos = randomPos.normalized;
            rb.transform.rotation = Quaternion.LookRotation(randomPos);

            if (currentState != State.Run)
            {
                // play run animation
                animationController.PlayRunAnimation();
                currentState = State.Run;
            }
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

    private void OnTriggerEnter(Collider other)
    {
        if (lockedTarget != null) return;

        if (other.CompareTag("Target"))
        {
            GameObject target = other.transform.parent.gameObject;
            lockedTarget = target;

            canMove = false;
            agent.isStopped = true;
            if (currentState != State.Idle)
            {
                // play idle animation
                animationController.PlayIdleAnimation();
                currentState = State.Idle;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Target") && lockedTarget != null && isRunning)
        {
            if (currentState != State.Idle)
            {
                // play idle animation
                animationController.PlayIdleAnimation();
                currentState = State.Idle;
            }
        }

        if (other.CompareTag("Target") && lockedTarget != null && !isRunning)
        {
            StartCoroutine(Attack());
        }

        if (other.CompareTag("Target") && lockedTarget == null)
        {
            GameObject target = other.transform.parent.gameObject;
            lockedTarget = target;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Target")) return;
        if (other.transform.parent.gameObject == lockedTarget)
        {
            lockedTarget = null;
            canMove = true;
            agent.isStopped = false;

            if (currentState != State.Run)
            {
                // play run animation
                animationController.PlayRunAnimation();
                currentState = State.Run;
            }
        }
    }

    private IEnumerator Attack()
    {
        isRunning = true;

        animationController.PlayAttackAnimation();
        currentState = State.Attack;

        if (!weapon.gameObject.activeSelf)
        {
            weapon.endPoint = lockedTarget.transform.position;
            weapon.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        isRunning = false;
    }
}
