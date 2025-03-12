using TMPro;
using UnityEngine;

public class GameplayController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TextMeshProUGUI aliveText;

    public static GameplayController instance;
    public int alive = 10;

    private void Awake()
    {
        if (instance == null) instance = this;
        aliveText.text = $"Alive: {alive}";
    }
   
    public void CheckWinCondition()
    {
        alive --;
        aliveText.text = $"Alive: {alive}";
        
        if (alive == 0)
        {
            playerController.HandleWin();
            spawner.StopSpawning();
        }
    }       
}
