using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private float speed;
    [SerializeField] private WeaponController weapon;
    [SerializeField] private Transform character;
    [SerializeField] private Transform attackZone;
    [SerializeField] private GameObject shell;
    [SerializeField] private GameObject billboard;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private Material originalMaterial;
    [SerializeField] private Material transparentMaterial;

    public GameObject onHandWeapon;
    private float moveHorizontal;
    private float moveVertical;
    private Vector3 direction;
    public State currentState = State.Idle;
    private bool isRunning = false;
    public GameObject lockedTarget;
    private int currentKill = 0;
    private float countdown = 2f;
    private Vector3 lastPosition;
    public bool isColliding = false;

    private void OnEnable()
    {
        DeathController.OnEnemyDeath += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        DeathController.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void FixedUpdate()
    {
        if (currentState == State.Win || currentState == State.Dead) return;

        if (!isColliding) lastPosition = transform.position;
        else rb.MovePosition(lastPosition);

        // get joystick input value
        moveHorizontal = joystick.Horizontal;
        moveVertical = joystick.Vertical;

        // create direction value
        direction = new Vector3(moveHorizontal, 0, moveVertical);

        if (direction.magnitude > 0.01f)
        {
            direction = direction.normalized;

            // move player
            rb.MovePosition(rb.position + direction * speed * Time.deltaTime);

            // rotate player to move direction
            rb.transform.rotation = Quaternion.LookRotation(direction);

            if (currentState != State.Run)
            {
                // play run animation
                animationController.PlayRunAnimation();
                currentState = State.Run;
            }
        }
        else
        {
            if (currentState != State.Idle && currentState != State.Attack)
            {
                animationController.PlayIdleAnimation();
                currentState = State.Idle;
            }
            else if (lockedTarget != null && !isRunning)
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

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Win || currentState == State.Dead) return;
        if (lockedTarget != null) return;

        if (other.CompareTag("Target"))
        {
            GameObject target = other.transform.parent.gameObject;
            target.transform.GetChild(1).gameObject.SetActive(true);
            lockedTarget = target;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentState == State.Win || currentState == State.Dead) return;
        if (other.CompareTag("Target") && lockedTarget == null)
        {
            GameObject target = other.transform.parent.gameObject;
            target.transform.GetChild(1).gameObject.SetActive(true);
            lockedTarget = target;
        }

        if (other.CompareTag("Obstacle"))
        {
            other.GetComponent<Renderer>().material = transparentMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState == State.Win || currentState == State.Dead) return;
        if (other.CompareTag("Obstacle")) other.GetComponent<Renderer>().material = originalMaterial;
        if (!other.CompareTag("Target")) return;
        GameObject target = other.transform.parent.gameObject;
        if (target == lockedTarget)
        {
            target.transform.GetChild(1).gameObject.SetActive(false);
            lockedTarget = null;
        }
    }

    private IEnumerator Attack()
    {
        animationController.PlayAttackAnimation();
        currentState = State.Attack;

        yield return new WaitForSeconds(0.12f);

        if (!weapon.gameObject.activeSelf && currentState == State.Attack)
        {
            isRunning = true;
            onHandWeapon.SetActive(false);

            weapon.endPoint = lockedTarget.transform.position;
            weapon.gameObject.SetActive(true);
        }
    }

    public void HandleKill()
    {
        lockedTarget = null;
        onHandWeapon.SetActive(true);
        currentKill ++;
        killText.text = currentKill.ToString();
        
        if(currentKill % 3 == 0)
        {
            character.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            attackZone.localScale += new Vector3(0.2f, 0f, 0.2f);
            speed += 0.2f;
        }
    }

    public void HandleDeath()
    {
        lockedTarget = null;
        onHandWeapon.SetActive(false);
        billboard.SetActive(false);
        shell.SetActive(false);
        attackZone.gameObject.SetActive(false);
        currentState = State.Dead;
        animationController.PlayDeathAnimation();
        Invoke("DeActiveObject", 2f);
    }

    private void DeActiveObject()
    {
        gameObject.SetActive(false);
        SceneManager.LoadScene(0);
    }

    public void HandleWin()
    {
        if (currentState == State.Dead) return;

        currentState = State.Win;
        rb.transform.rotation = Quaternion.LookRotation(Vector3.back);
        animationController.PlayWinAnimation();
    }

    private void HandleEnemyDeath(EnemyController deadEnemy)
    {
        if (lockedTarget != null && deadEnemy == lockedTarget)
            lockedTarget = null;
    }
}
