using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using TMPro;

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
    [SerializeField] private GameObject billboard;
    [SerializeField] private TextMeshProUGUI killText;

    private State currentState = State.Idle;
    private bool canMove = true;
    private bool isRunning = false;
    public GameObject lockedTarget;
    public GameObject onHandWeapon;
    private int currentKill = 0;
    private float countdown = 2f;
    private float timeInAttackZone;

    private void OnEnable()
    {
        if (IndicatorsManager.Instance != null) IndicatorsManager.Instance.RegisterEnemy(transform);
        DeathController.OnEnemyDeath += HandleEnemyDeath;
        currentState = State.Idle;
        isRunning = false;
        weapon.gameObject.SetActive(false);
        onHandWeapon.SetActive(true);
        billboard.SetActive(true);
        shell.SetActive(true);
        canMove = true;
        agent.isStopped = false;
        agent.ResetPath();

        InvokeRepeating(nameof(MoveToRandomPosition), 0f, 5f);
    }

    private void OnDisable()
    {
        if (IndicatorsManager.Instance != null) IndicatorsManager.Instance.UnregisterEnemy(transform);
        DeathController.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void FixedUpdate()
    {
        if (currentState == State.Dead) return;
        if (currentState == State.Attack) return;

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

        if (isRunning)
        {
            countdown -= Time.deltaTime;

            if (countdown <= 0)
            {
                onHandWeapon.SetActive(true);
                isRunning = false;
                currentState = State.Idle;
                countdown = 2f;
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
            currentState = State.Idle;
            animationController.PlayIdleAnimation();
            timeInAttackZone = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentState == State.Dead) return;

        if (other.CompareTag("Target") && lockedTarget != null && !isRunning)
        {
            timeInAttackZone += Time.deltaTime; 

            if (timeInAttackZone >= 0.5f) StartCoroutine(Attack());
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
            StartCoroutine(ClearTargetAfterDelay());
    }

    private IEnumerator ClearTargetAfterDelay()
    {
        yield return new WaitForSeconds(0.2f); 
        lockedTarget = null;
        canMove = true;
        agent.isStopped = false;
        agent.ResetPath();
        timeInAttackZone = 0f;
    }

    private IEnumerator Attack()
    {
        animationController.PlayAttackAnimation();
        currentState = State.Attack;

        Vector3 targetDirection =
                    new Vector3(
                        lockedTarget.transform.position.x - transform.position.x,
                        0f,
                        lockedTarget.transform.position.z - transform.position.z);

        targetDirection = targetDirection.normalized;
        rb.transform.rotation = Quaternion.LookRotation(targetDirection);

        yield return new WaitForSeconds(0.12f);

        if (!weapon.gameObject.activeSelf)
        {
            isRunning = true;
            onHandWeapon.SetActive(false);

            weapon.endPoint = lockedTarget.transform.position;
            weapon.gameObject.SetActive(true);
        }
    }

    public void HandleKill()
    {
        targetLockUI.SetActive(false);
        lockedTarget = null;
        currentKill++;
        killText.text = currentKill.ToString();

        if (currentKill % 3 == 0)
        {
            character.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            attackZone.localScale += new Vector3(0.2f, 0f, 0.2f);
        }
    }

    private void HandleEnemyDeath(EnemyController deadEnemy)
    {
        if (lockedTarget != null && deadEnemy == lockedTarget)
            lockedTarget = null;
    }

    public void HandleDeath()
    {
        ResetValues();
        DeathController.NotifyEnemyDeath(this);
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
        shell.SetActive(false);
        onHandWeapon.SetActive(false);
        billboard.SetActive(false);
    }

    private void DeActiveObject()
    {
        gameObject.SetActive(false);
    }
}
