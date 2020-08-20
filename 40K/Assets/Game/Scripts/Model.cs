using System;
using System.Collections.Generic;
using System.Linq;
using Cover = Global.Cover;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Model : MonoBehaviour
{
    [ReadOnlyInInspector]
    public bool isSelected = false;
    public GameObject selection;
    public bool isFixed = false;

    public Unit unit;
    public Squad squad;

    public MeleeWeapon melee;
    public RangedWeapon ranged;
    public Grenade grenade;
    public List<Wargear> allWargear;

    [ReadOnlyInInspector]
    public bool isSlowed;
    [ReadOnlyInInspector]
    public bool isEngaged;
    [ReadOnlyInInspector]
    public bool isEvacuating;
    [ReadOnlyInInspector]
    public int assaultWoundsInflicted;
    [ReadOnlyInInspector]
    public Cover coverType;
    [ReadOnlyInInspector]
    public float distanceMoved;

    [SerializeField]
    protected float baseDiameter;
    protected float baseRadius;
    protected const float ENEMY_MIN_DISTANCE = 1f;
    [ReadOnlyInInspector]
    public const float COHESION_DISTANCE = 2f;
    protected const float DISTANCE_THRESHOLD = 0.1f;
    protected const float COHESION_CONSTANT = 1.5f;
    protected const float SCATTER_CONSTANT = 10f;
    protected const float LEADER_VELOCITY_CONSTANT = 1.5f;
    protected const float VELOCITY_CONSTANT = 5f;
    protected const float SPEED_THRESHOLD = 0.5f;

    [ReadOnlyInInspector]
    public NavMeshAgent agent;

    public const string LAYER_NAME = "Models";
    public static int LAYER_MASK { get { return LayerMask.NameToLayer(LAYER_NAME); } }

    [ReadOnlyInInspector]
    public float DEBUG_avgDistance;
    public float DEBUG_speed;
    public Vector3 DEBUG_velocity;

    #region Unity
    public void Awake()
    {
        this.agent = GetComponent<NavMeshAgent>();
        this.baseRadius = baseDiameter / 2f;

        JoinSquad();
    }

    public void Start()
    {
        if (squad == null)
        {
            return;
        }

        squad.OnSelectionUpdated += SetSelection;
        //squad.OnMoveToLocation += MoveToLocation;
        squad.OnMoveToLocation += WaypointUpdated;
    }

    public void Update()
    {
        if(isFixed == true)
        {
            return;
        }

        MaintainCoherency();

        this.DEBUG_velocity = this.agent.velocity;
        this.DEBUG_speed = DEBUG_velocity.magnitude;
    }
    #endregion Unity

    protected void JoinSquad()
    {
        if (squad == null)
        {
            return;
        }

        squad.allSquadMembers.AddLast(this);
    }

    public void SetSelection(bool selected)
    {
        this.isSelected = selected;

        if (selection == null)
        {
            return;
        }

        selection.SetActive(selected);
    }

    public void MoveToLocation(Vector3 location)
    {
        float distance = Vector3.Distance(transform.position, location);
        if (distance <= baseRadius)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        agent.SetDestination(location);
    }

    public void WaypointUpdated()
    {
        if (squad.squadLeader != this)
        {
            return;
        }

        MoveToLocation(squad.currentWaypoint);
    }

    public void MaintainCoherency()
    {
        if (this == squad.squadLeader)
        {
            return;
        }

        SortedList<float, Model> allNearbySquadMembers = GetNearestSquadMembers();

        Vector3 velocity = this.agent.velocity;

        Vector3 currentPosition = this.transform.position;

        // Maintain spacing between 2 closest members if too close
        KeyValuePair<float, Model> squadMemberA = allNearbySquadMembers.ElementAt(0);
        Vector3 squadMemberAPosition = squadMemberA.Value.transform.position;
        float squadMemberADistance = squadMemberA.Key;

        KeyValuePair<float, Model> squadMemberB = allNearbySquadMembers.ElementAt(1);
        Vector3 squadMemberBPosition = squadMemberB.Value.transform.position;
        float squadMemberBDistance = squadMemberB.Key;

        float avgDistance = (squadMemberADistance + squadMemberBDistance) / 2f;
        this.DEBUG_avgDistance = avgDistance;

        Vector3 backoffA = Vector3.zero;
        if (COHESION_DISTANCE - squadMemberADistance > DISTANCE_THRESHOLD)
        {
            backoffA = (currentPosition - squadMemberAPosition) * COHESION_CONSTANT;  // Move away
        }

        Vector3 backoffB = Vector3.zero;
        if (COHESION_DISTANCE - squadMemberBDistance > DISTANCE_THRESHOLD)
        {
            backoffB = (currentPosition - squadMemberBPosition) * COHESION_CONSTANT;  // Move away
        }

        // Move towards nearest squad member if too far away
        Vector3 approachA = Vector3.zero;
        if (squadMemberADistance - COHESION_DISTANCE > DISTANCE_THRESHOLD)
        {
            backoffA = squadMemberAPosition - currentPosition;    // Move toward
        }

        Vector3 deltaPosition = backoffA + backoffB + approachA;

        // Maintain momentum
        Vector3 velocityOffset = velocity / VELOCITY_CONSTANT;
        if (velocity.magnitude < SPEED_THRESHOLD)
        {
            velocityOffset = Vector3.zero;
        }

        deltaPosition += velocityOffset;

        // Match leader's speed and direction
        Vector3 leaderVelocityOffset = this.squad.squadLeader.agent.velocity / LEADER_VELOCITY_CONSTANT;

        float squadLeaderSpeed = this.squad.squadLeader.agent.velocity.magnitude;
        if (squadLeaderSpeed < SPEED_THRESHOLD)
        {
            leaderVelocityOffset = Vector3.zero;
        }

        deltaPosition += leaderVelocityOffset;

        float magnitude = deltaPosition.magnitude;

        float moveSpeed = this.squad.GetMaxMovementSpeed();
        float adjMagnitude = Mathf.Clamp(magnitude, 0, moveSpeed);

        Vector3 adjVelocity = deltaPosition.normalized * adjMagnitude;
        this.agent.velocity = adjVelocity;
    }

    #region MaintainCoherency

    protected SortedList<float, Model> GetNearestSquadMembers()
    {
        return this.squad.GetNearestSquadMembers(this);
    }

    #endregion MaintainCoherency

    public float GetDistanceFromModel(Model model)
    {
        float distance = Vector3.Distance(this.transform.position, model.transform.position);
        distance -= baseRadius;
        distance -= model.baseRadius;
        return distance;
    }

    protected float GetDistanceBetweenTwoModels(Model first, Model second)
    {
        float distance = Vector3.Distance(first.transform.position, second.transform.position);
        distance -= first.baseRadius;
        distance -= second.baseRadius;
        return distance;
    }

    public Model ChooseEnemy(Squad enemySquad)
    {
        throw new NotImplementedException();
    }

    public bool MeleeRangeCheck(Model enemyCombatant, Squad squad)
    {
        throw new NotImplementedException();
    }

    public bool IsInCoverCheck()
    {
        throw new NotImplementedException();
    }

    public void MeleeCombat(Model defender, Squad squad)
    {
        throw new NotImplementedException();
    }

    public void NormalRangedAttack(Model enemy)
    {
        throw new NotImplementedException();
    }

    public void Overwatch(Model enemy)
    {
        throw new NotImplementedException();
    }

    public bool IsAliveCheck()
    {
        throw new NotImplementedException();
    }

    public bool MoraleCheck(int modifier)
    {
        throw new NotImplementedException();
    }
}