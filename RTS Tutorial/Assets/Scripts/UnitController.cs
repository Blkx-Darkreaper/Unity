using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class UnitController : EntityController
{

    public float moveSpeed, turnSpeed;
    protected bool isMoving, isTurning;
    private Vector3 currentWaypoint;
    private Quaternion targetHeading;
    private GameObject targetEntityGameObject;
    protected struct UnitProperties
    {
        public const string IS_MOVING = "IsMoving";
        public const string IS_TURNING = "IsTurning";
        public const string WAYPOINT = "Waypoint";
        public const string TARGET_HEADING = "TargetHeading";
        public const string TARGET_ID = "TargetId";
    }

    protected override void Start()
    {
        base.Start();

        if(isLoadedFromSave == false) {
            return;
        }

		LoadTargetEntityGameObject(attackTargetId);
    }

    public virtual void SetSpawner(StructureController spawner)
    {
    }

    protected override void Update()
    {
        base.Update();
        TurnTowardsPoint();
        MoveToPoint();
    }

	protected void LoadTargetEntityGameObject(int entityId) {
		if (entityId < 0) {
			return;
		}

        try
        {
            targetEntityGameObject = GameManager.activeInstance.GetGameEntityById(entityId).gameObject;
        }
        catch
        {
            Debug.Log(string.Format("Failed to load target Entity GameObject"));
        }
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

        if (targetEntityGameObject == null)
        {
            return;
        }

        currentWaypoint = UpdateTargetWaypoint(targetEntityGameObject, currentWaypoint);
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

        waypoint.y = targetEntityGameObject.transform.position.y;
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

        bool isGround = hitEntity.CompareTag(Tags.GROUND);
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
        targetEntityGameObject = null;
    }

    public virtual void SetWaypoint(Vector3 destination, GameObject target)
    {
        SetWaypoint(destination);
        targetEntityGameObject = target;
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

        bool isGround = entityUnderMouse.CompareTag(Tags.GROUND);
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

    protected override void SaveDetails(JsonWriter writer)
    {
        base.SaveDetails(writer);

        SaveManager.SaveBoolean(writer, UnitProperties.IS_MOVING, isMoving);
        SaveManager.SaveBoolean(writer, UnitProperties.IS_TURNING, isTurning);
        SaveManager.SaveVector(writer, UnitProperties.WAYPOINT, currentWaypoint);
        SaveManager.SaveQuaternion(writer, UnitProperties.TARGET_HEADING, targetHeading);
        if (targetEntityGameObject != null)
        {
            EntityController targetEntity = targetEntityGameObject.GetComponent<EntityController>();
            if (targetEntity != null)
            {
                SaveManager.SaveInt(writer, UnitProperties.TARGET_ID, targetEntity.entityId);
            }
        }
    }

    protected override bool LoadDetails(JsonReader reader, string propertyName)
    {
        bool loadCompete = false;

        base.LoadDetails(reader, propertyName);

        switch (propertyName)
        {
            case UnitProperties.IS_MOVING:
                isMoving = LoadManager.LoadBoolean(reader);
                break;

            case UnitProperties.IS_TURNING:
                isTurning = LoadManager.LoadBoolean(reader);
                break;

            case UnitProperties.WAYPOINT:
                currentWaypoint = LoadManager.LoadVector(reader);
                break;

            case UnitProperties.TARGET_HEADING:
                targetHeading = LoadManager.LoadQuaternion(reader);
                break;

            case UnitProperties.TARGET_ID:
                attackTargetId = LoadManager.LoadInt(reader);
                break;
        }

        return loadCompete;
    }
}