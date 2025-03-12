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
    [SerializeField] private GameObject targetLockUI;
    [SerializeField] private GameObject shell;
    [SerializeField] private Transform character;
    [SerializeField] private Transform attackZone;

    private State currentState = State.Idle;
    private bool canMove = true;
    private bool isRunning = false;
    public GameObject lockedTarget;
    private int currentKill = 0;

    private void OnEnable()
    {
        currentState = State.Idle;
        isRunning = false;
        weapon.gameObject.SetActive(false);
        shell.SetActive(true);
        canMove = true;
        agent.isStopped = false;
        agent.ResetPath();

        InvokeRepeating(nameof(MoveToRandomPosition), 0f, 5f);
    }

    private void FixedUpdate()
    {
        if (currentState == State.Dead) return;

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
        if (currentState == State.Dead) return;

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
        if (currentState == State.Dead) return;

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
            Vector3 targetDirection =
                    new Vector3(
                        lockedTarget.transform.position.x - transform.position.x,
                        0f,
                        lockedTarget.transform.position.z - transform.position.z);

            targetDirection = targetDirection.normalized;
            rb.transform.rotation = Quaternion.LookRotation(targetDirection);

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
        if (currentState == State.Dead) return;

        if (!other.CompareTag("Target")) return;
        if (other.transform.parent.gameObject == lockedTarget)
        {
            lockedTarget = null;
            canMove = true;
            agent.isStopped = false;
            agent.ResetPath();
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

    public void HandleKill()
    {
        targetLockUI.SetActive(false);
        lockedTarget = null;
        currentKill++;

        if (currentKill % 3 == 0)
        {
            character.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            attackZone.localScale += new Vector3(0.2f, 0f, 0.2f);
        }
    }

    public void HandleDeath()
    {
        ResetValues();
        currentState = State.Dead;
        animationController.PlayDeathAnimation();
        Invoke("DeActiveObject", 2f);
    }

    public void HandleEndGame()
    {
        ResetValues();
        DeActiveObject();
    }

    private void ResetValues()
    {
        targetLockUI.SetActive(false);
        canMove = false;
        agent.isStopped = true;
        lockedTarget = null;
        shell.SetActive(false);
    }

    private void DeActiveObject()
    {
        gameObject.SetActive(false);
    }
}
