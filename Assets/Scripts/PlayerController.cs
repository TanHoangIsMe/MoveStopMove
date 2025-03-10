using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Joystick joystick;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AnimationController animationController;
    [SerializeField] private float speed;

    private float moveHorizontal;
    private float moveVertical;
    private Vector3 direction;
    private State currentState = State.Idle;

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
            if (currentState != State.Idle)
            {
                animationController.PlayIdleAnimation();
                currentState = State.Idle;
            }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Enemy") && currentState == State.Idle)
        {
            animationController.PlayAttackAnimation();
            currentState = State.Attack;
        }
    }
}
