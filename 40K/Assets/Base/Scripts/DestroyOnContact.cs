using UnityEngine;

public struct Tags
{
    public const string BOUNDARY = "Boundary";
    public const string PLAYER = "Player";
    public const string GAMECONTROLLER = "GameController";
    public const string ENEMY = "Enemy";
}

public class DestroyOnContact : MonoBehaviour
{
    public GameObject explosion;
    public GameObject playerExplosion;

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
            return;
        }

        Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
    }
}