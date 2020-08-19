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

    //public void MaintainCoherency()
    //{
    //    if (squad.allSquadMembers.Count == 1)
    //    {
    //        return;
    //    }
    //    if (squad.squadLeader == this)
    //    {
    //        return;
    //    }

    //    List<KeyValuePair<float, Model>> allNearestSquadMembers = GetNearestSquadMembers(2);

    //    Model nearestSquadMember = allNearestSquadMembers[0].Value;
    //    Model nextNearestSquadMember = allNearestSquadMembers[1].Value;

    //    SortedList<float, Vector3> allLocations = new SortedList<float, Vector3>();

    //    float x1 = nearestSquadMember.transform.position.x;
    //    float y1 = nearestSquadMember.transform.position.y;
    //    float z1 = nearestSquadMember.transform.position.z;
    //    float r1 = nearestSquadMember.baseRadius + COHESION_DISTANCE;

    //    float x2 = nextNearestSquadMember.transform.position.x;
    //    float y2 = nextNearestSquadMember.transform.position.y;
    //    float z2 = nextNearestSquadMember.transform.position.z;
    //    float r2 = nextNearestSquadMember.baseRadius + COHESION_DISTANCE;

    //    if (allNearestSquadMembers[0].Key <= r1 && allNearestSquadMembers[1].Key <= r2)
    //    {
    //        return;
    //    }

    //    //float x = transform.position.x;
    //    //float z = transform.position.z;
    //    //float m = (z1 - z) / (x1 - x);
    //    //if (z1 == z)
    //    //{
    //    //    m = 0;
    //    //}

    //    //if (x1 == x)
    //    //{
    //    //    m = 1;
    //    //}

    //    //float deltaX = r1 / (float)Math.Sqrt(Math.Pow(m, 2) + 1);
    //    //float deltaZ = deltaX * m;

    //    //Vector3 nearbyLocation = new Vector3(deltaX + x1, y1, deltaZ + z1);
    //    //float distance = Vector3.Distance(nearbyLocation, transform.position);
    //    //allLocations.Add(distance, nearbyLocation);

    //    //float distanceApart = GetDistanceBetweenTwoModels(nearestSquadMember, nextNearestSquadMember);

    //    //if (distanceApart <= COHESION_DISTANCE)
    //    //{
    //    // Calculate intersection points
    //    double r1Squared = Math.Pow(r1, 2);
    //        double r2Squared = Math.Pow(r2, 2);

    //        double d = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(z1 - z2, 2));
    //        double l = (r1Squared - r2Squared + Math.Pow(d, 2)) / (2 * d);
    //        double h = Math.Sqrt(r1Squared - Math.Pow(l, 2));

    //        double xTerm1 = l / d * (x2 - x1);
    //        double xTerm2 = h / d * (z2 - z1);

    //        double zTerm1 = l / d * (z2 - z1);
    //        double zTerm2 = h / d * (x2 - x1);

    //        float intersectX1 = (float)(xTerm1 + xTerm2 + x1);
    //        float intersectX2 = (float)(xTerm1 - xTerm2 + x1);

    //        float intersectZ1 = (float)(zTerm1 + zTerm2 + z1);
    //        float intersectZ2 = (float)(zTerm1 - zTerm2 + z1);

    //        float avgHeight = (y1 + y2) / 2f;

    //        Vector3 intersect1 = new Vector3(intersectX1, avgHeight, intersectZ1);
    //        float distance1 = Vector3.Distance(intersect1, transform.position);
    //        allLocations.Add(distance1, intersect1);

    //        Vector3 intersect2 = new Vector3(intersectX2, avgHeight, intersectZ2);
    //        float distance2 = Vector3.Distance(intersect2, transform.position);
    //        allLocations.Add(distance2, intersect2);
    //    //}

    //    Vector3 location = allLocations.First().Value;
    //    MoveToLocation(location);
    //}

    //protected Model GetNearestSquadMember(out float smallestDistance)
    //{
    //    Model nearestSquadMember = squad.squadLeader;
    //    smallestDistance = GetDistanceFromModel(nearestSquadMember);

    //    foreach (Model squadMember in squad.allSquadMembers)
    //    {
    //        if (squadMember == this)
    //        {
    //            continue;
    //        }
    //        if (squadMember == squad.squadLeader)
    //        {
    //            continue;
    //        }

    //        float distance = GetDistanceFromModel(squadMember);
    //        if (distance >= smallestDistance)
    //        {
    //            continue;
    //        }

    //        nearestSquadMember = squadMember;
    //        smallestDistance = distance;
    //    }

    //    return nearestSquadMember;
    //}

    //protected List<KeyValuePair<float, Model>> GetNearestSquadMembers(int totalMembers)
    //{
    //    SortedList<float, Model> allSortedSquadMembers = new SortedList<float, Model>();

    //    foreach (Model squadMember in squad.allSquadMembers)
    //    {
    //        if (squadMember == this)
    //        {
    //            continue;
    //        }

    //        float distance = GetDistanceFromModel(squadMember);
    //        allSortedSquadMembers.Add(distance, squadMember);
    //    }

    //    List<KeyValuePair<float, Model>> allNearestSquadMembers = allSortedSquadMembers.ToList();
    //    allNearestSquadMembers.RemoveRange(totalMembers, allNearestSquadMembers.Count - totalMembers);
    //    return allNearestSquadMembers;
    //}

    //public void MaintainCoherency()
    //{
    //    if(this == squad.squadLeader)
    //    {
    //        return;
    //    }

    //    SortedList<float, Model> allNearbySquadMembers = GetNearestSquadMembers();
    //    float distanceSum = 0;
    //    foreach(KeyValuePair<float, Model> nearbySquadMember in allNearbySquadMembers)
    //    {
    //        distanceSum += nearbySquadMember.Key;
    //    }

    //    float avgDistance = distanceSum / allNearbySquadMembers.Count;
    //    this.DEBUG_avgDistance = avgDistance;

    //    if (Mathf.Abs(avgDistance - COHESION_DISTANCE) <= DISTANCE_THRESHOLD)
    //    {
    //        this.agent.velocity = Vector3.zero;
    //        return;
    //    }

    //    Vector3 velocity = this.agent.velocity;

    //    Vector3 cohesionVectorOffset = BoidMoveTowardsCenterOfMass();
    //    Vector3 separationVectorOffset = BoidMaintainSeparation();
    //    Vector3 velocityVectorOffset = BoidMatchVelocity();

    //    float squadLeaderSpeed = this.squad.squadLeader.agent.velocity.magnitude;
    //    if (squadLeaderSpeed < SPEED_THRESHOLD)
    //    {
    //        velocityVectorOffset = Vector3.zero;
    //    }

    //    velocity += cohesionVectorOffset + separationVectorOffset + velocityVectorOffset;
    //    float magnitude = velocity.magnitude;

    //    float moveSpeed = this.squad.GetMaxMovementSpeed();
    //    float adjMagnitude = Mathf.Clamp(magnitude, 0, moveSpeed);

    //    Vector3 adjVelocity = velocity.normalized * adjMagnitude;
    //    this.agent.velocity = adjVelocity;
    //}

    public void MaintainCoherency()
    {
        if (this == squad.squadLeader)
        {
            return;
        }

        SortedList<float, Model> allNearbySquadMembers = GetNearestSquadMembers();

        Vector3 velocity = this.agent.velocity;

        Vector3 currentPosition = this.transform.position;

        // Maintain spacing between 2 closest members
        KeyValuePair<float, Model> squadMemberA = allNearbySquadMembers.ElementAt(0);
        Vector3 squadMemberAPosition = squadMemberA.Value.transform.position;
        float squadMemberADistance = squadMemberA.Key;

        KeyValuePair<float, Model> squadMemberB = allNearbySquadMembers.ElementAt(1);
        Vector3 squadMemberBPosition = squadMemberB.Value.transform.position;
        float squadMemberBDistance = squadMemberB.Key;

        float avgDistance = (squadMemberADistance + squadMemberBDistance) / 2f;
        this.DEBUG_avgDistance = avgDistance;

        Vector3 deltaPositionA = Vector3.zero;
        if(squadMemberADistance - COHESION_DISTANCE > DISTANCE_THRESHOLD)
        {
            deltaPositionA = squadMemberAPosition - currentPosition;    // Move toward
        }
        if (COHESION_DISTANCE - squadMemberADistance > DISTANCE_THRESHOLD)
        {
            deltaPositionA = (currentPosition - squadMemberAPosition) * COHESION_CONSTANT;  // Move away
        }

        Vector3 deltaPositionB = Vector3.zero;
        if (squadMemberBDistance - COHESION_DISTANCE > DISTANCE_THRESHOLD)
        {
            deltaPositionB = squadMemberBPosition - currentPosition;    // Move toward
        }
        if (COHESION_DISTANCE - squadMemberBDistance > DISTANCE_THRESHOLD)
        {
            deltaPositionB = (currentPosition - squadMemberBPosition) * COHESION_CONSTANT;  // Move away
        }

        // Move towards squad leader if possible
        Vector3 deltaPositionLeader = Vector3.zero;

        float leaderDistance = GetDistanceBetweenTwoModels(this, squad.squadLeader);
        if (leaderDistance - COHESION_DISTANCE > DISTANCE_THRESHOLD)
        {
            bool canMoveTowardsLeader = allNearbySquadMembers.ContainsValue(squad.squadLeader);
            if (canMoveTowardsLeader == true)
            {
                deltaPositionLeader = squad.squadLeader.transform.position - currentPosition;    // Move toward
            }
        }

        Vector3 deltaPosition = deltaPositionA + deltaPositionB + deltaPositionLeader;

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

    protected Vector3 BoidMoveTowardsCenterOfMass()
    {
        Vector3 centerOfMass = GetCenterOfMass();
        Vector3 vectorOffset = (centerOfMass - transform.position) / COHESION_CONSTANT;
        return vectorOffset;
    }

    protected Vector3 GetCenterOfMass()
    {
        Vector3 sum = Vector3.zero;

        foreach (Model model in squad.allSquadMembers)
        {
            if (model == this)
            {
                continue;
            }

            Vector3 position = model.transform.position;
            sum += position;
        }

        Vector3 centerOfMass = sum / (squad.allSquadMembers.Count - 1);
        return centerOfMass;
    }

    protected Vector3 BoidMaintainSeparation()
    {
        Vector3 vectorOffset = Vector3.zero;

        foreach (Model model in squad.allSquadMembers)
        {
            if (model == this)
            {
                continue;
            }

            float distance = GetDistanceBetweenTwoModels(model, this);
            if (distance >= COHESION_DISTANCE)
            {
                continue;
            }

            vectorOffset -= (model.transform.position - this.transform.position);
        }

        return vectorOffset;
    }

    protected Vector3 BoidMatchVelocity()
    {
        Vector3 perceivedVelocity = Vector3.zero;

        foreach (Model model in squad.allSquadMembers)
        {
            if (model == this)
            {
                continue;
            }

            perceivedVelocity += model.agent.velocity;
        }

        Vector3 vectorOffset = (perceivedVelocity - this.agent.velocity) / VELOCITY_CONSTANT;
        return vectorOffset;
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