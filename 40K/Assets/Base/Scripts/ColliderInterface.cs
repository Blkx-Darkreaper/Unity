using UnityEngine;

public interface ColliderInterface
{
    void OnTriggerEnter(Collider other);

    void OnTriggerEnter2D(Collider2D other);

    void HandleCollision(Collider other, Collider2D other2d);
}