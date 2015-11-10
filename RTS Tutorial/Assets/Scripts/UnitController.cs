using UnityEngine;
using System.Collections;
using RTS;

public class UnitController : EntityController
{

    public float moveSpeed, turnSpeed;
    protected bool isMoving, isTurning;
    private Vector3 currentWaypoint;
    private Quaternion targetHeading;
    private GameObject targetEntity;

    public virtual void SetSpawner(StructureController spawner)
    {
    }

    protected override void Update()
    {
        base.Update();
        TurnTowardsPoint();
        MoveToPoint();
    }

    private void TurnTowardsPoint()
    {
        if (isTurning == false)
        {
            return;
        }

        targetHeading = Quaternion.LookRotation(currentWaypoint - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetHeading, turnSpeed);
        Quaternion inverseTargetRotation = new Quaternion(-targetHeading.w, -targetHeading.x, -targetHeading.y, -targetHeading.z);
        UpdateBounds();

        if (transform.rotation != targetHeading)
        {
            if (transform.rotation != inverseTargetRotation)
            {
                return;
            }
        }

        isTurning = false;
        isMoving = true;

        if (targetEntity == null)
        {
            return;
        }

        currentWaypoint = UpdateTargetWaypoint(targetEntity, currentWaypoint);
    }

    private Vector3 UpdateTargetWaypoint(GameObject target, Vector3 waypoint)
    {
        // Calculate the number of origin's vectors from origin center to edge of bounds
        Vector3 originalExtents = selectionBounds.extents;
        Vector3 normalExtents = originalExtents;
        normalExtents.Normalize();
        float numberOfExtents = originalExtents.x / normalExtents.x;
        int originShift = Mathf.FloorToInt(numberOfExtents);

        // Calculate the number of origin's vectors from target center to edge of bounds
        EntityController entity = target.GetComponent<EntityController>();
        if (entity != null)
        {
            originalExtents = entity.selectionBounds.extents;
        }
        else
        {
            originalExtents = Vector3.zero;
        }
        normalExtents = originalExtents;
        normalExtents.Normalize();
        numberOfExtents = originalExtents.x / normalExtents.x;
        int targetShift = Mathf.FloorToInt(numberOfExtents);

        // Calculate the number of origin's vectors between origin center and target center with bounds just touching
        int shiftAmount = targetShift + originShift;

        // Calculate the direction needed to travel to reach target in straight line and normalize to origin's vector
        Vector3 origin = transform.position;
        Vector3 bearing = new Vector3(waypoint.x - origin.x, 0f, waypoint.z - origin.z);
        bearing.Normalize();

        // Move to just within touching target
        for (int i = 0; i < shiftAmount; i++)
        {
            waypoint -= bearing;
        }

        waypoint.y = targetEntity.transform.position.y;
        return waypoint;
    }

    public void MoveToPoint()
    {
        if (isMoving == false)
        {
            if (isAdvancing == false)
            {
                return;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, Time.deltaTime * moveSpeed);
        UpdateBounds();

        if (transform.position != currentWaypoint)
        {
            return;
        }

        isMoving = false;
        isAdvancing = false;
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
            ResourceController resource = hitEntity.GetComponentInParent<ResourceController>();
            if (resource == null)
            {
                return;
            }

            bool resourceDepleted = resource.isEmpty;
            if (resourceDepleted == false)
            {
                return;
            }
        }

        if (hitPoint == ResourceManager.invalidPoint)
        {
            return;
        }

        float x = hitPoint.x;
        float y = hitPoint.y + player.selectedEntity.transform.position.y;  // Ensures unit stays on top of the surface it is on
        float z = hitPoint.z;
        Vector3 destination = new Vector3(x, y, z);
        SetWaypoint(destination);
    }

    public virtual void SetWaypoint(Vector3 destination)
    {
        currentWaypoint = destination;
        isTurning = true;
        isMoving = false;
        targetEntity = null;
    }

    public virtual void SetWaypoint(Vector3 destination, GameObject target)
    {
        SetWaypoint(destination);
        targetEntity = target;
    }

    public override void SetHoverState(GameObject entityUnderMouse)
    {
        base.SetHoverState(entityUnderMouse);

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
            ResourceController resource = entityUnderMouse.GetComponentInParent<ResourceController>();
            if (resource == null)
            {
                return;
            }

            bool resourceDepleted = resource.isEmpty;
            if (resourceDepleted == false)
            {
                return;
            }
        }

        owner.hud.SetCursorState(CursorState.move);
    }
}