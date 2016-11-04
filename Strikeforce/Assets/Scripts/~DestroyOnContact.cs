using UnityEngine;
using System.Collections;
using Strikeforce;

public class DestroyOnContact : MonoBehaviour {

    public GameObject Explosion;
    public GameObject PlayerExplosion;
    private GameManager gameManager;
    public int pointsValue;

    public void Start()
    {
        GameObject gameControllerObject = GameObject.FindWithTag(Tags.GAMECONTROLLER);
        if (gameControllerObject == null)
        {
            Debug.Log("Cannot find 'GameController' script");
            return;
        }

        gameManager = gameControllerObject.GetComponent<GameManager>();
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
        if (Explosion != null)
        {
            Instantiate(Explosion, transform.position, transform.rotation);
        }
        
        Destroy(other.gameObject);
        Destroy(gameObject);

        if (other.CompareTag(Tags.PLAYER) == false)
        {
            //gameManager.AddPointsToScore(pointsValue);
            return;
        }

        Instantiate(PlayerExplosion, other.transform.position, other.transform.rotation);
        gameManager.GameOver();
    }
}
