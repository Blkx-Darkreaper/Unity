using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cover = Global.Cover;
using Object = UnityEngine.Object;
using UnityEngine;

[Serializable]
public class Squad : MonoBehaviour
{
    [ReadOnlyInInspector]
    public int ID;
    private static int NEXT_ID = 1;

    public string squadType;
    public Army army;
    public Model squadLeader;
    public LinkedList<Model> allSquadMembers = new LinkedList<Model>();
    public string[] actions;

    public bool isSelected = false;
    public delegate void SelectionUpdateAction(bool selected);
    public event SelectionUpdateAction OnSelectionUpdated;

    public float moveDistance;
    //public delegate void MoveToLocationAction(Vector3 location);
    public delegate void MoveToLocationAction();
    public event MoveToLocationAction OnMoveToLocation;

    [ReadOnlyInInspector]
    public Vector3 currentWaypoint;

    public int startingStrength;
    public int currentStrength;
    [ReadOnlyInInspector]
    public bool hasMoved = false;
    [ReadOnlyInInspector]
    public bool hasFired = false;
    [ReadOnlyInInspector]
    public bool canCharge = true;
    [ReadOnlyInInspector]
    public bool isDelayed = false;
    [ReadOnlyInInspector]
    public bool isFallingBack = false;
    [ReadOnlyInInspector]
    public bool isPinned = false;
    [ReadOnlyInInspector]
    public bool isDestroyed = false;

    #region Unity
    public void Awake()
    {
        this.ID = Squad.NEXT_ID++;
    }

    public void Start()
    {
        float minMoveDistance = float.MaxValue;
        foreach (Model model in allSquadMembers)
        {
            float moveSpeed = model.unit.moveDistance;
            if (moveSpeed >= minMoveDistance)
            {
                continue;
            }

            minMoveDistance = moveSpeed;
        }

        this.moveDistance = minMoveDistance;

        if(squadLeader == null)
        {
            return;
        }

        if (squadLeader.squad == this)
        {
            return;
        }

        squadLeader = null;
    }

    #endregion Unity

    public virtual void SetSelection(bool selected)
    {
        this.isSelected = selected;

        if (OnSelectionUpdated == null)
        {
            return;
        }

        OnSelectionUpdated(selected);
    }

    public virtual void MoveToLocation(Vector3 location)
    {
        if (OnMoveToLocation == null)
        {
            return;
        }

        this.currentWaypoint = location;
        //OnMoveToLocation(location);
        OnMoveToLocation();
    }

    public float GetMaxMovementSpeed()
    {
        // TODO
        return moveDistance;
    }

    protected class SquadDistanceComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            string[] xArr = x.ToString().Split('.');
            string[] yArr = y.ToString().Split('.');

            int xInt = Int32.Parse(xArr[0]);
            int yInt = Int32.Parse(yArr[0]);

            if(xInt != yInt)
            {
                return xInt - yInt;
            }

            string xRem = xArr.Length > 1 ? xArr[1] : string.Empty;
            string yRem = yArr.Length > 1 ? yArr[1] : string.Empty;

            int minLength = Math.Min(xRem.Length, yRem.Length);

            for (int i = 0; i < minLength; i++)
            {
                if(i > xRem.Length)
                {
                    return -1;
                }
                if(i > yRem.Length)
                {
                    return 1;
                }

                int digitX = Int32.Parse(xRem[i].ToString());
                int digitY = Int32.Parse(yRem[i].ToString());

                if(digitX == digitY)
                {
                    continue;
                }

                return digitX - digitY;
            }

            return xRem.Length - yRem.Length;
        }
    }

    public SortedList<float, Model> GetNearestSquadMembers(Model model)
    {
        int squadId = model.squad.ID;

        SortedList<float, Model> allNearbySquadMembers = new SortedList<float, Model>(allSquadMembers.Count, new SquadDistanceComparer());

        foreach (Model squadMember in allSquadMembers)
        {
            if (squadMember == model)
            {
                continue;
            }

            SortedList<float, Model> allSortedHitSquadMembers = new SortedList<float, Model>();

            // Determine if any other models are in between
            const float MAX_DISTANCE = 500f;
            //const float MAX_DISTANCE = Model.COHESION_DISTANCE * allSquadMembers.Count;

            RaycastHit[] allHitModels = Physics.RaycastAll(model.transform.position, squadMember.transform.position, MAX_DISTANCE, Model.LAYER_MASK);
            if (allHitModels.Length > 0)
            {
                foreach (RaycastHit hit in allHitModels)
                {
                    Model hitModel = hit.transform.gameObject.GetComponentInParent<Model>();
                    if (hitModel == null)
                    {
                        continue;
                    }

                    if(hitModel.gameObject.activeInHierarchy != true)
                    {
                        continue;
                    }

                    if (hitModel == model)
                    {
                        continue;
                    }

                    int hitModelSquadId = hitModel.squad.ID;
                    if (hitModelSquadId != squadId)
                    {
                        continue;
                    }

                    float distance = model.GetDistanceFromModel(squadMember);
                    allSortedHitSquadMembers.Add(distance, hitModel);
                }
            }

            if (allSortedHitSquadMembers.Count == 0)
            {
                float distance = model.GetDistanceFromModel(squadMember);

                allNearbySquadMembers.Add(distance, squadMember);
            }
            else
            {
                KeyValuePair<float, Model> nearestHitSquadMember = allSortedHitSquadMembers.First();

                float distance = nearestHitSquadMember.Key;
                Model nearestSquadMember = nearestHitSquadMember.Value;

                allNearbySquadMembers.Add(distance, nearestSquadMember);
            }
        }

        return allNearbySquadMembers;
    }
}