using UnityEngine;

public class FallKill : MonoBehaviour
{
    public float killHeight = -10f;
    public PlayerStats playerStats;

    void Start()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (transform.position.y < killHeight && playerStats != null)
        {
            playerStats.Kill();
        }
    }
}
