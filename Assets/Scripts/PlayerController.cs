using System.Collections;
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

    private float moveHorizontal;
    private float moveVertical;
    private Vector3 direction;
    public State currentState = State.Idle;
    private bool isRunning = false;
    public GameObject lockedTarget;
    private int currentKill = 0;

    private void FixedUpdate()
    {
        if (currentState == State.Win || currentState == State.Dead) return;

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
            if (currentState != State.Idle)
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentState == State.Win || currentState == State.Dead) return;
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
        lockedTarget = null;
        currentKill ++;
        
        if(currentKill % 3 == 0)
        {
            character.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            attackZone.localScale += new Vector3(0.2f, 0f, 0.2f);
        }
    }

    public void HandleDeath()
    {
        lockedTarget = null;
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
}
