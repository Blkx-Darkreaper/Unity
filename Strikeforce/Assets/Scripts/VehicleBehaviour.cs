using UnityEngine;
using System.Collections;

namespace Strikeforce
{
    public class VehicleBehaviour : MonoBehaviour
    {
        protected Vehicle slave { get; set; }

        protected void ReturnToBase()
        {
            if (slave == null)
            {
                return;
            }

            float distanceTravelled = slave.DistanceTravelled;
            float range = slave.Range;
            if (distanceTravelled < range)
            {
                return;
            }

            float fuel = slave.FuelRemaining;
            if (fuel > 0)
            {
                return;
            }

            slave.AddWaypoint(slave.HomeBase.rallyPoint);
        }
    }
}