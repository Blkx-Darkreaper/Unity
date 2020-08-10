using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rules : MonoBehaviour
{
    public static Rules singleton;

    public void Awake()
    {
        if (singleton == null)
        {
            DontDestroyOnLoad(gameObject);
            singleton = this;
        }
        if (singleton != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public int GetStrength(Squad squad)
    {
        int strength = 0;
        foreach (Model model in squad.allSquadMembers)
        {
            Unit.ModelType modelType = model.unit.modelType;
            bool isMonstrous = modelType.Equals(Unit.ModelType.monstrousCreature);
            if (isMonstrous == true)
            {
                strength += 10;
                continue;
            }

            bool isVehicle = model.unit.isVehicle;
            if (isVehicle == true)
            {
                if (model.unit.weaponSkill == 0)
                {
                    continue;
                }

                Vehicle vehicle = (Vehicle)model.unit;
                int frontArmour = vehicle.armour.front;
                if (frontArmour < 12)
                {
                    strength += 5;
                    continue;
                }

                strength += 10;
                continue;
            }

            strength += model.unit.wounds;
        }

        return strength;
    }

    public Vector3 GetSquadLeaderLocation(Squad squad)
    {
        return squad.squadLeader.transform.position;
    }

    //public void MoveToLocation(Squad squad, Vector3 location, float movement)
    //{
    //    if (squad.isFallingBack == true)
    //    {
    //        bool regrouped = RegroupCheck();
    //        if (regrouped == true)
    //        {
    //            Regroup();
    //            return;
    //        }
    //    }

    //    SortedList<float, Model> allDistancesMoved = new SortedList<float, Model>();
    //    foreach (Model model in squad.allSquadMembers)
    //    {
    //        float distanceToLocation = Global.GetDistance(model.transform.position, location);

    //        float distanceMoved = model.MoveToLocation(location);
    //        allDistancesMoved.Add(distanceMoved, model);
    //    }

    //    float mostDistanceMoved = allDistancesMoved.First().Key;

    //    if (mostDistanceMoved == movement)
    //    {
    //        return;
    //    }

    //    if (squad.isFallingBack == false)
    //    {
    //        return;
    //    }

    //    Announcer.AnnounceSquadAction(squad, Announcer.SquadActions.trapped);
    //    squad.isDestroyed = true;
    //}

    //private void ReformUnitCohesion(float distancePool)
    //{
    //    throw new NotImplementedException();
    //}

    //private void RepositionUnit(Vector3 endingDirection, float distancePool)
    //{
    //    throw new NotImplementedException();
    //}

    //public void AssaultSquad(Squad squad, Squad enemySquad)
    //{
    //    if (squad.canCharge == false)
    //    {
    //        return;
    //    }
    //    Announcer.AnnounceSquadAction(squad, Announcer.SquadActions.assault, enemySquad);

    //    bool isMassacre = false;
    //    Vector3 currentLocation = squad.GetSquadLeaderLocation();
    //    Vector3 enemyLocation = enemySquad.GetSquadLeaderLocation();
    //    float distanceToTarget = Global.GetDistance(currentLocation, enemyLocation);
    //    bool chargeSucceeds = ChargeCheck(distanceToTarget);

    //    if (chargeSucceeds == false)
    //    {
    //        enemySquad.Overwatch(this);
    //        throw new NotImplementedException();
    //    }

    //    bool enemyFallingBack = enemySquad.isFallingBack;
    //    if (enemyFallingBack == true)
    //    {
    //        bool enemyRegroups = CanRegroupCheck(enemySquad);
    //        if (enemyRegroups == false)
    //        {
    //            isMassacre = true;
    //            enemySquad.isDestroyed = true;

    //            ConsolidatePosition(isMassacre);
    //            return;
    //        }

    //        enemySquad.Regroup();
    //    }

    //    enemySquad.Overwatch(this);

    //    MoveToLocation(enemyLocation, distanceToTarget);

    //    CloseCombat(enemySquad);

    //    int friendlyWoundsInflicted = GetAssaultWoundsInflicted();
    //    int enemyWoundsInflicted = enemySquad.GetAssaultWoundsInflicted();
    //    RemoveCasualties();
    //    enemySquad.RemoveCasualties();

    //    bool friendliesVanquished = isDestroyed;
    //    if (friendliesVanquished == true)
    //    {
    //        enemySquad.ConsolidatePosition(friendliesVanquished);
    //        return;
    //    }

    //    bool enemyVanquished = enemySquad.isDestroyed;
    //    if (enemyVanquished == true)
    //    {
    //        ConsolidatePosition(enemyVanquished);
    //        return;
    //    }

    //    bool stillInMelee = !enemyVanquished;

    //    Squad assaultVictor = null;
    //    Squad assaultLoser = null;
    //    if (friendlyWoundsInflicted > enemyWoundsInflicted)
    //    {
    //        assaultVictor = this;
    //        assaultLoser = enemySquad;
    //    }
    //    if (enemyWoundsInflicted > friendlyWoundsInflicted)
    //    {
    //        assaultVictor = enemySquad;
    //        assaultLoser = this;
    //    }
    //    if (friendlyWoundsInflicted == enemyWoundsInflicted)
    //    {
    //        return;
    //    }
    //    int modifier = 0;
    //    bool isBelowHalfStrength = assaultLoser.BelowHalfStrengthCheck();
    //    if (isBelowHalfStrength == true)
    //    {
    //        modifier = -1;
    //    }

    //    int outnumberedModifier = assaultLoser.GetOutnumberedModifier(assaultVictor);
    //    modifier += outnumberedModifier;

    //    bool loserStandsGround = assaultLoser.MoraleCheck(modifier);
    //    if (loserStandsGround == true)
    //    {
    //        return;
    //    }

    //    isMassacre = assaultVictor.SweepingAdvanceCheck(assaultLoser);
    //    if (isMassacre == true)
    //    {
    //        stillInMelee = false;
    //        enemySquad.isDestroyed = true;
    //    }
    //    else
    //    {
    //        assaultLoser.FallBack();
    //    }

    //    assaultVictor.ConsolidatePosition(isMassacre);

    //    if (stillInMelee == false)
    //    {
    //        return;
    //    }

    //    PileIn(enemyLocation);
    //    enemySquad.PileIn(currentLocation);
    //}

    //private int GetOutnumberedModifier(Squad enemySquad)
    //{
    //    int friendlyStrength = GetStrength();
    //    int enemyStrength = enemySquad.GetStrength();

    //    if (enemyStrength >= 4 * friendlyStrength)
    //    {
    //        return -4;
    //    }
    //    if (enemyStrength > 3 * friendlyStrength)
    //    {
    //        return -3;
    //    }
    //    if (enemyStrength > 2 * friendlyStrength)
    //    {
    //        return -2;
    //    }
    //    if (enemyStrength > friendlyStrength)
    //    {
    //        return -1;
    //    }

    //    return 0;
    //}

    //private void Disengage()
    //{
    //    if (isDestroyed == true)
    //    {
    //        return;
    //    }

    //    foreach (Model model in allSquadMembers)
    //    {
    //        model.isEngaged = false;
    //    }
    //}

    //private void FallBack()
    //{
    //    Disengage();
    //    isFallingBack = true;

    //    float fallBackMovement = Global.RollDice(2);
    //    Vector3 fallBackPoint = GetFallBackPoint();

    //    MoveToLocation(fallBackPoint, fallBackMovement);
    //}

    //private Vector3 GetFallBackPoint()
    //{
    //    throw new NotImplementedException();
    //}

    //private bool CanRegroupCheck()
    //{
    //    int currentStrength = allSquadMembers.Count;
    //    float strengthPercentage = currentStrength / (float)startingStrength;
    //    if (strengthPercentage < 0.5f)
    //    {
    //        return false;
    //    }

    //    Vector3 currentLocation = GetSquadLeaderLocation();
    //    LinkedList<Squad> nearbyEnemies = army.GetEnemySquadsInRange(currentLocation, Global.Ranges.footMovement);
    //    if (nearbyEnemies.Count > 0)
    //    {
    //        return false;
    //    }

    //    bool hasCoherency = CohesionCheck();
    //    if (hasCoherency == false)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private bool CohesionCheck()
    //{
    //    throw new NotImplementedException();
    //}

    //private bool RegroupCheck()
    //{
    //    bool canRegroup = CanRegroupCheck();
    //    if (canRegroup == false)
    //    {
    //        return false;
    //    }

    //    int modifier = 0;
    //    Vector3 currentLocation = GetSquadLeaderLocation();
    //    LinkedList<Squad> enemiesInSight = army.GetEnemySquadsInSight(currentLocation);
    //    if (enemiesInSight.Count == 0)
    //    {
    //        modifier += 1;
    //    }

    //    bool testPassed = LeadershipCheck(modifier);
    //    if (testPassed == false)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private void Regroup()
    //{
    //    isFallingBack = false;
    //    hasMoved = true;
    //}

    //private void PileIn(Vector3 enemyLocation)
    //{
    //    if (allSquadMembers.Count == 0)
    //    {
    //        return;
    //    }

    //    List<Model> unengagedMembers = new List<Model>();
    //    foreach (Model model in allSquadMembers)
    //    {
    //        bool isEngaged = model.isEngaged;
    //        if (isEngaged == true)
    //        {
    //            continue;
    //        }

    //        Vector2 currentLocation = model.transform.position;
    //        float distanceToEnemy = Global.GetDistance(currentLocation, enemyLocation);
    //        float distanceToMove = 6;
    //        if (distanceToEnemy < distanceToMove)
    //        {
    //            distanceToMove = distanceToEnemy;
    //        }

    //        model.MoveToLocation(enemyLocation, distanceToMove);
    //    }
    //}

    //private bool ChargeCheck(float distanceToTarget)
    //{
    //    int roll = Global.RollDice(2);

    //    if (roll < distanceToTarget)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private void CloseCombat(Squad enemySquad)
    //{
    //    SortedList<int, Tuple<Model, Model>> closeCombatants = new SortedList<int, Tuple<Model, Model>>();

    //    foreach (Model model in allSquadMembers)
    //    {
    //        Model enemyCombatant = model.ChooseEnemy(enemySquad);

    //        bool isEngaged = model.MeleeRangeCheck(enemyCombatant, this);
    //        model.isEngaged = isEngaged;
    //        enemyCombatant.isEngaged = isEngaged;
    //        if (isEngaged == false)
    //        {
    //            continue;
    //        }

    //        int individualInitiative = model.unit.initiative;
    //        bool inCover = model.isInCoverCheck();
    //        if (inCover == true)
    //        {
    //            individualInitiative = 10;
    //        }

    //        Tuple<Model, Model> combatantPair = new Tuple<Model, Model>(model, enemyCombatant);
    //        closeCombatants.Add(individualInitiative, combatantPair);

    //        combatantPair = new Tuple<Model, Model>(enemyCombatant, model);
    //        int enemyInitiative = enemyCombatant.unit.initiative;
    //        inCover = enemyCombatant.isInCoverCheck();
    //        if (inCover == true)
    //        {
    //            enemyInitiative = 10;
    //        }

    //        closeCombatants.Add(enemyInitiative, combatantPair);
    //    }

    //    foreach (Tuple<Model, Model> combatantPair in closeCombatants.Values)
    //    {
    //        Model attacker = combatantPair.Item1;
    //        Model defender = combatantPair.Item2;

    //        attacker.MeleeCombat(defender, this);
    //    }
    //}

    //private int GetAssaultWoundsInflicted()
    //{
    //    int totalWoundsInflicted = 0;

    //    foreach (Model model in allSquadMembers)
    //    {
    //        totalWoundsInflicted += model.assaultWoundsInflicted;
    //        model.assaultWoundsInflicted = 0;
    //    }

    //    return totalWoundsInflicted;
    //}

    //private bool SweepingAdvanceCheck(Squad retreatingEnemy)
    //{
    //    int initiativeCharacteristic = GetInitiative();
    //    int roll = Global.RollDice(1);
    //    initiativeCharacteristic += roll;

    //    int enemyInitiativeCharacteristic = retreatingEnemy.GetInitiative();
    //    roll = Global.RollDice(1);
    //    enemyInitiativeCharacteristic += roll;

    //    if (enemyInitiativeCharacteristic > initiativeCharacteristic)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private int GetInitiative()
    //{
    //    List<int> allInitiatives = new List<int>();
    //    int lowestInitiative = allSquadMembers.First().unit.initiative;
    //    foreach (Model model in allSquadMembers)
    //    {
    //        int initiative = model.unit.initiative;
    //        allInitiatives.Add(initiative);
    //        if (initiative < lowestInitiative)
    //        {
    //            lowestInitiative = initiative;
    //        }
    //    }

    //    var sortedInitiatives = allInitiatives.GroupBy(item => item).OrderByDescending(g => g.Count()).Select(g => g.Key);
    //    int majorityInitiative = sortedInitiatives.ElementAt(0);
    //    int secondMajorityInitiative = sortedInitiatives.ElementAt(1);

    //    if (majorityInitiative == secondMajorityInitiative)
    //    {
    //        return lowestInitiative;
    //    }

    //    return majorityInitiative;
    //}

    //private void ConsolidatePosition(bool enemyVanquished)
    //{
    //    Vector3 currentLocation = GetSquadLeaderLocation();
    //    Squad nearestEnemySquad = army.GetNearestEnemySquad(currentLocation);
    //    Vector3 nearestEnemyLocation = nearestEnemySquad.GetSquadLeaderLocation();

    //    if (enemyVanquished == true)
    //    {
    //        Vector3 nearestEnemyDirection = Global.GetDirection(currentLocation, nearestEnemyLocation);
    //        RepositionUnit(nearestEnemyDirection, 6);
    //        return;
    //    }

    //    ReformUnitCohesion(3);
    //}

    //private void RangedCombat(Squad enemySquad)
    //{
    //    foreach (Model model in allSquadMembers)
    //    {
    //        Model enemy = model.ChooseEnemy(enemySquad);
    //        model.NormalRangedAttack(enemy);
    //    }

    //    enemySquad.RemoveCasualties();
    //    bool checkPassed = enemySquad.ShootingCasualtiesCheck();
    //    if (checkPassed == true)
    //    {
    //        return;
    //    }

    //    int modifier = 0;
    //    bool isBelowHalfStrength = enemySquad.BelowHalfStrengthCheck();
    //    if (isBelowHalfStrength == true)
    //    {
    //        modifier = -1;
    //    }

    //    checkPassed = enemySquad.MoraleCheck(modifier);
    //    if (checkPassed == true)
    //    {
    //        return;
    //    }

    //    enemySquad.FallBack();
    //}

    //private void Overwatch(Squad chargingUnit)
    //{
    //    foreach (Model model in allSquadMembers)
    //    {
    //        Model enemy = model.ChooseEnemy(chargingUnit);
    //        model.Overwatch(enemy);
    //    }
    //}

    //private void RemoveCasualties()
    //{
    //    List<Model> allMembers = new List<Model>(allSquadMembers);
    //    foreach (Model model in allMembers)
    //    {
    //        bool isAlive = model.IsAliveCheck();
    //        if (isAlive == true)
    //        {
    //            continue;
    //        }

    //        allSquadMembers.Remove(model);
    //    }
    //}

    //private bool MassacreCheck()
    //{
    //    if (allSquadMembers.Count == 0)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    //private bool LastManStandingCheck()
    //{
    //    if (allSquadMembers.Count > 1)
    //    {
    //        return false;
    //    }

    //    if (startingStrength == 1)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //public bool ShootingCasualtiesCheck()
    //{
    //    int totalSquadMembers = allSquadMembers.Count;
    //    int casualties = 0;
    //    foreach (Model model in allSquadMembers)
    //    {
    //        bool isAlive = model.IsAliveCheck();
    //        if (isAlive == true)
    //        {
    //            continue;
    //        }
    //        casualties++;
    //    }

    //    float lossesPercentage = casualties / (float)totalSquadMembers;
    //    if (lossesPercentage < 0.25f)
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //public void SetUnitCover()
    //{
    //    throw new NotImplementedException();
    //}

    //private bool IsSquadInCover()
    //{
    //    foreach (Model model in allSquadMembers)
    //    {
    //        Cover coverType = model.coverType;
    //        bool notInCover = coverType.Equals(Global.Cover.clear);
    //        if (notInCover == true)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //private bool BelowHalfStrengthCheck()
    //{
    //    int currentStrength = allSquadMembers.Count;

    //    float strengthPercentage = currentStrength / (float)startingStrength;
    //    if (strengthPercentage < 0.5)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    //private bool MoraleCheck(int modifier)
    //{
    //    int successes = 0;
    //    foreach (Model model in allSquadMembers)
    //    {
    //        bool checkPassed = model.MoraleCheck(modifier);
    //        if (checkPassed == false)
    //        {
    //            continue;
    //        }

    //        successes++;
    //    }

    //    int totalSquadMembers = allSquadMembers.Count;
    //    float successPercentage = successes / (float)totalSquadMembers;
    //    if (successPercentage < 0.5f)   // To be Updated
    //    {
    //        return false;
    //    }

    //    return true;
    //}

    //private bool LeadershipCheck(int modifier)
    //{
    //    throw new NotImplementedException();
    //}

    //public void Deploy(Vector3 deploymentLocation)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool EvacuateCheck()
    //{
    //    foreach (Model model in allSquadMembers)
    //    {
    //        bool evactuating = model.isEvacuating;
    //        if (evactuating == false)
    //        {
    //            continue;
    //        }

    //        return true;
    //    }

    //    return false;
    //}
}