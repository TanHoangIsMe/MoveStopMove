using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [SerializeField] GameObject owner;
    [SerializeField] private float speed = 5f;      
    [SerializeField] private float spinSpeed = 360f;

    private Vector3 startPoint;
    public Vector3 endPoint;

    private EnemyController enemyController;
    private PlayerController playerController;

    private void Awake()
    {
        enemyController = owner.GetComponent<EnemyController>();
        playerController = owner.GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        startPoint = transform.parent.position + Vector3.up * 1.5f;
        endPoint += Vector3.up * 1.5f;
        StartCoroutine(MoveAndSpin());
    }

    private IEnumerator MoveAndSpin()
    {
        float distance = Vector3.Distance(startPoint, endPoint);
        float travelTime = distance / speed;
        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            transform.position = Vector3.Lerp(startPoint, endPoint, elapsed / travelTime);

            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target") && other.transform.parent.gameObject != owner)
        {
            if (playerController != null) playerController.HandleKill();
            else enemyController.HandleKill();

            EnemyController enemy = other.transform.parent.GetComponent<EnemyController>();
            if(enemy != null) enemy.HandleDeath();
            else other.transform.parent.GetComponent<PlayerController>().HandleDeath();

            if(GameplayController.instance != null)
                GameplayController.instance.CheckWinCondition();

            gameObject.SetActive(false);
        }
    }
}
