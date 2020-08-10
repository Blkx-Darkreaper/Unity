using UnityEngine;
using Strikeforce;

public class _DestroyOnContact : MonoBehaviour
{
    public GameObject Explosion;
    public GameObject PlayerExplosion;
    public int pointsValue;

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
        GameplayManager.singleton.GameOver();
    }
}
