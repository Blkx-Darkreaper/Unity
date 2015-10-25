using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

[System.Serializable]
public class Movement
{
    public float speed;
    public float bank;
}

[System.Serializable]
public class BallisticWeapon
{
    public float cooldown;
    public GameObject projectileType;
}

public class PlayerController : MonoBehaviour
{
    private const string HORIZONTAL_AXIS = "Horizontal";
    private const string VERTICAL_AXIS = "Vertical";
    private const string FIRE_1 = "Fire1";
    public Movement move;
    public Boundary bounds;

    public BallisticWeapon weapon;
    private float nextFire = 0f;
    private bool fireLeft = true;
    public Transform leftShotSpawn;
    public Transform rightShotSpawn;

    public void Update()
    {
        bool hasFired = Input.GetButton(FIRE_1);
        if (hasFired == false)
        {
            return;
        }

        float currentTime = Time.time;
        if (currentTime <= nextFire)
        {
            Debug.Log(string.Format("Cooldown active"));
            return;
        }
        nextFire = currentTime + weapon.cooldown;

        GameObject projectileClone;
        if (fireLeft == true)
        {
            Debug.Log(string.Format("Firing left cannon"));
            projectileClone = Instantiate(weapon.projectileType, leftShotSpawn.position, leftShotSpawn.rotation) as GameObject;
            fireLeft = false;
        }
        else
        {
            Debug.Log(string.Format("Firing right cannon"));
            projectileClone = Instantiate(weapon.projectileType, rightShotSpawn.position, rightShotSpawn.rotation) as GameObject;
            fireLeft = true;
        }

        var blasterSound = GetComponent<AudioSource>();
        blasterSound.Play();
    }

    public void FixedUpdate()
    {
        float horizontalMovement = GetMovement(HORIZONTAL_AXIS);
        float verticalMovement = GetMovement(VERTICAL_AXIS);

        Vector3 movement = new Vector3(horizontalMovement, 0f, verticalMovement);

        var player = GetComponent<Rigidbody>();
        player.velocity = movement * move.speed;
        player.position = new Vector3(
            Mathf.Clamp(player.position.x, bounds.xMin, bounds.xMax),
            0f,
            Mathf.Clamp(player.position.z, bounds.zMin, bounds.zMax)
        );
        player.rotation = Quaternion.Euler(0f, 0f, player.velocity.x * -move.bank);
    }

    private float GetMovement(string axis)
    {
        float movement = Input.GetAxis(axis);
        return movement;
    }
}
