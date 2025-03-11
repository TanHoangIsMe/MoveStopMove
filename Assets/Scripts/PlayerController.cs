using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private float speed;
    [SerializeField] private WeaponController weapon;

    private float moveHorizontal;
    private float moveVertical;
    private Vector3 direction;
    private State currentState = State.Idle;
    private bool isRunning = false;
    private GameObject lockedTarget;

    private void FixedUpdate()
    {
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
        if (other.CompareTag("Target") && lockedTarget == null)
        {
            GameObject target = other.transform.parent.gameObject;
            target.transform.GetChild(1).gameObject.SetActive(true);
            lockedTarget = target;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Target")) return;
        if (other.transform.parent.gameObject == lockedTarget)
            Debug.Log("out");
        GameObject targetLock = other.transform.parent.gameObject.transform.GetChild(1).gameObject;
        if (targetLock == null || !targetLock.activeSelf) return;

        targetLock.SetActive(false);
        lockedTarget = null;
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
