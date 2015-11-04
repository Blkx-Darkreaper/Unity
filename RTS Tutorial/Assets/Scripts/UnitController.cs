using UnityEngine;
using System.Collections;
using RTS;

public class UnitController : EntityController {

    public float moveSpeed, turnSpeed;
    protected bool isMoving, isTurning;
    private Vector3 currentWaypoint;
    private Quaternion targetHeading;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        TurnTowardsPoint();
        MoveToPoint();
    }

    protected override void OnGUI()
    {
        base.OnGUI();
    }

    private void TurnTowardsPoint()
    {
        if (isTurning == false)
        {
            return;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetHeading, turnSpeed);
        Quaternion inverseTargetRotation = new Quaternion(-targetHeading.w, - targetHeading.x, -targetHeading.y, -targetHeading.z);
        CalculateBounds();

        if (transform.rotation != targetHeading)
        {
            if (transform.rotation != inverseTargetRotation)
            {
                return;
            }
        }

        isTurning = false;
        isMoving = true;
    }

    public void MoveToPoint()
    {
        if (isMoving == false)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, Time.deltaTime * moveSpeed);
        CalculateBounds();

        if (transform.position != currentWaypoint)
        {
            return;
        }

        isMoving = false;
    }

    public override void MouseClick(GameObject hitEntity, Vector3 hitPoint, PlayerController player)
    {
        base.MouseClick(hitEntity, hitPoint, player);

        if (player == null)
        {
            return;
        }
        if (player.isNPC == true)
        {
            return;
        }
        if (isSelected == false)
        {
            return;
        }

        bool isGround = hitEntity.CompareTag(Tags.ground);
        if (isGround == false)
        {
            return;
        }

        if (hitPoint == ResourceManager.invalidPoint)
        {
            return;
        }

        float x = hitPoint.x;
        float y = hitPoint.y + player.selectedEntity.transform.position.y;  // Ensures unity statys on top of the surface it is on
        float z = hitPoint.z;
        Vector3 destination = new Vector3(x, y, z);
        SetWaypoint(destination);
    }

    public void SetWaypoint(Vector3 destination)
    {
        currentWaypoint = destination;
        targetHeading = Quaternion.LookRotation(destination - transform.position);
        isTurning = true;
        isMoving = false;
    }

    public override void SetOverState(GameObject entityUnderMouse)
    {
        base.SetOverState(entityUnderMouse);

        if (owner == null)
        {
            return;
        }
        if (owner.isNPC == true)
        {
            return;
        }
        if (isSelected == false)
        {
            return;
        }

        bool isGround = entityUnderMouse.CompareTag(Tags.ground);
        if (isGround == false)
        {
            return;
        }

        owner.hud.SetCursorState(CursorState.move);
    }
}