using UnityEngine;
using System.Collections;

public struct Tags
{
    public const string BOUNDARY = "Boundary";
    public const string PLAYER = "Player";
    public const string GAMECONTROLLER = "GameController";
    public const string ENEMY = "Enemy";
}

public class DestroyOnContact : MonoBehaviour {

    public GameObject explosion;
    public GameObject playerExplosion;
    private GameController gameController;
    public int pointsValue;

    public void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag(Tags.GAMECONTROLLER);
        if (gameControllerObject == null)
        {
            Debug.Log("Cannot find 'GameController' script");
            return;
        }

        gameController = gameControllerObject.GetComponent<GameController>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.BOUNDARY) == true)
        {
            return;
        }
        if (other.CompareTag(Tags.ENEMY) == true)
        {
            return;
        }

        Debug.Log(string.Format("{0} destroyed by collision with {1}", this.name, other.name));
        if (explosion != null)
        {
            Instantiate(explosion, transform.position, transform.rotation);
        }
        
        Destroy(other.gameObject);
        Destroy(gameObject);

        if (other.CompareTag(Tags.PLAYER) == false)
        {
            gameController.AddPointsToScore(pointsValue);
            return;
        }

        Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
        gameController.GameOver();
    }
}
