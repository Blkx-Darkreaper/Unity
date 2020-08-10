using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Color = UnityEngine.Color;

namespace Strikeforce
{
    public class PlayerRaidMode : MonoBehaviour
    {
        public Hud hud;
        [HideInInspector]
        public Raider currentRaider;
        public string raiderPrefabName = "Raider";  //testing
        [HideInInspector]
        public Checkpoint previousCheckpoint = null;

        protected void Awake()
        {
            this.hud = Instantiate(hud, this.gameObject.transform).GetComponent<Hud>();
        }

        public void SpawnRaider(Level spawnLevel, ref PlayerCamera playerCamera)	// Testing
        {
            Vector3 spawnLocation = spawnLevel.GetRaiderSpawnLocation();

            // Spawn the raider at the spawnpoint
            GameObject raiderObject = Instantiate(
                GlobalAssets.GetVehiclePrefab(raiderPrefabName),
                spawnLocation,
                Quaternion.identity) as GameObject;

            // Make the raider a child of the enemy base
            raiderObject.transform.parent = spawnLevel.gameObject.transform;  // Make position relative to enemy base

            this.currentRaider = raiderObject.GetComponent<Raider>();

            //NetworkServer.SpawnWithClientAuthority(raiderObject, connectionToClient);
            NetworkServer.SpawnWithClientAuthority(this.currentRaider.gameObject, gameObject);
            GameEntityManager.singleton.RegisterEntity(this.currentRaider);

            RaiderLoadout loadoutPrefab = GlobalAssets.GetMiscPrefab("Loadout").GetComponent<RaiderLoadout>();
            RaiderLoadout loadout = Instantiate<RaiderLoadout>(loadoutPrefab);
            this.currentRaider.ReadyRaider(loadout);

            Hardpoint hardpointPrefab = GlobalAssets.GetMiscPrefab("Hardpoint").GetComponent<Hardpoint>();

            Hardpoint[] leftOuterWing = new Hardpoint[1];
            leftOuterWing[0] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            leftOuterWing[0].Init(-138, -69, 1, 1, HardpointPosition.LeftOuterWing);

            Hardpoint[] leftWing = new Hardpoint[1];
            leftWing[0] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            leftWing[0].Init(-94, -16, 1, 1, HardpointPosition.LeftWing);

            Hardpoint[] center = new Hardpoint[2];
            center[0] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            center[0].Init(-22, 116, 1, 1, HardpointPosition.Center);
            center[1] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            center[1].Init(-22, 26, 1, 3, HardpointPosition.Center);

            Hardpoint[] rightWing = new Hardpoint[1];
            rightWing[0] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            rightWing[0].Init(50, -16, 1, 1, HardpointPosition.RightWing);

            Hardpoint[] rightOuterWing = new Hardpoint[1];
            rightOuterWing[0] = Instantiate<Hardpoint>(hardpointPrefab, loadout.transform);
            rightOuterWing[0].Init(94, -69, 1, 1, HardpointPosition.RightOuterWing);

            currentRaider.Loadout.SetLayout(new Vector3[] {
                new Vector3(-0.25f, -0.25f, 0),
                new Vector3(-0.125f, -0.125f, 0),
                new Vector3(0, 0, 0),
                new Vector3(0.125f, 0, 0),
                new Vector3(0.25f, 0, 0)},
            //new Hardpoint[] { new Hardpoint(-138, -69, 1, 1, HardpointPosition.LeftOuterWing) },
            //new Hardpoint[] { new Hardpoint(-94, -16, 1, 1, HardpointPosition.LeftWing) },
            //new Hardpoint[] { new Hardpoint(-22, 116, 1, 1, HardpointPosition.Center),
            //    new Hardpoint(-22, 26, 1, 3, HardpointPosition.Center) },
            //new Hardpoint[] { new Hardpoint(50, -16, 1, 1, HardpointPosition.RightWing) },
            //new Hardpoint[] { new Hardpoint(94, -69, 1, 1, HardpointPosition.RightOuterWing) });
            leftOuterWing, leftWing, center, rightWing, rightOuterWing);

            GameObject basicShotPrefab = GlobalAssets.GetWeaponPrefab(Weapon.Types.BASIC_SHOT);
            Weapon basicShot1 = GameObject.Instantiate(basicShotPrefab).GetComponent<Weapon>() as Weapon;
            basicShot1.transform.parent = currentRaider.transform;
            GameEntityManager.singleton.RegisterEntity(basicShot1);

            Weapon basicShot2 = GameObject.Instantiate(basicShotPrefab).GetComponent<Weapon>() as Weapon;
            basicShot2.transform.parent = currentRaider.transform;
            GameEntityManager.singleton.RegisterEntity(basicShot2);

            GameObject boltPrefab = GlobalAssets.GetWeaponPrefab(Weapon.Types.BOLT);
            Weapon bolt = GameObject.Instantiate(boltPrefab).GetComponent<Weapon>() as Weapon;
            bolt.transform.parent = currentRaider.transform;
            GameEntityManager.singleton.RegisterEntity(bolt);

            bool equipped = currentRaider.Loadout.EquipWeapon(basicShot1, HardpointPosition.Center, 0, 1, 0, currentRaider);
            equipped &= currentRaider.Loadout.EquipWeapon(basicShot2, HardpointPosition.Center, 0, 2, 0, currentRaider);
            equipped &= currentRaider.Loadout.EquipWeapon(bolt, HardpointPosition.Center, 0, 0, 0, currentRaider);
            if (equipped == true)
            {
                currentRaider.Loadout.ReadyWeapons();
            }
            else
            {
                Debug.Log(String.Format("Failed to equip weapons to raider {0}", currentRaider.entityId));
            }

            Vector3 raiderPosition = raiderObject.transform.position;
            playerCamera.SetOverheadCameraPosition(raiderPosition);

            // Set raider and camera initial velocity
            float initialVelocity = currentRaider.StartingSpeed;
            currentRaider.SetForwardVelocity(initialVelocity);

            Vector3 raiderVelocity = currentRaider.GetVelocity();
            playerCamera.SetMainCameraVelocity(raiderVelocity.x, raiderVelocity.y, raiderVelocity.z);

            // Make raider airbourne
            currentRaider.TakeOff();
            if (currentRaider.isAirborne == false)
            {
                Debug.Log(String.Format("Raider {0} of player {1} failed to take off", currentRaider.entityId, currentRaider.currentOwner.playerId));
            }
        }

        public Raider SpawnRaider(string raiderPrefabName, Level spawnLevel, Vector3 spawnLocation)
        {
            GameObject raiderPrefab = GlobalAssets.GetVehiclePrefab(raiderPrefabName);

            // Spawn the raider at the spawnpoint
            GameObject raiderObject = Instantiate(
                raiderPrefab,
                spawnLocation,
                Quaternion.identity) as GameObject;

            // Make the raider a child of the enemy base
            raiderObject.transform.parent = spawnLevel.gameObject.transform;

            Raider raider = raiderObject.GetComponent<Raider>();

            //NetworkServer.SpawnWithClientAuthority(raiderObject, connectionToClient);
            NetworkServer.SpawnWithClientAuthority(raider.gameObject, gameObject);
            GameEntityManager.singleton.RegisterEntity(raider);

            return raider;
        }

        public void DespawnRaider()
        {
            Raider raider = this.currentRaider;
            this.currentRaider = null;

            GameEntityManager.singleton.RemoveEntity(raider);
        }

        public void LaunchRaider(RaiderLoadout loadout, Level spawnLevel, Vector3 spawnLocation, ref PlayerCamera playerCamera)
        {
            string raiderPrefabName = loadout.RaiderType;

            Raider raider = SpawnRaider(raiderPrefabName, spawnLevel, spawnLocation);
            raider.ReadyRaider(loadout);

            this.currentRaider = raider;

            Vector3 raiderPosition = raider.gameObject.transform.position;
            playerCamera.SetOverheadCameraPosition(raiderPosition);

            // Set raider and camera initial velocity
            float initialVelocity = raider.StartingSpeed;
            currentRaider.SetForwardVelocity(initialVelocity);

            Vector3 raiderVelocity = currentRaider.GetVelocity();
            playerCamera.SetMainCameraVelocity(raiderVelocity.x, raiderVelocity.y, raiderVelocity.z);

            // Make raider airbourne
            raider.TakeOff();
            if (currentRaider.isAirborne == false)
            {
                Debug.Log(String.Format("Raider {0} of player {1} failed to take off", raider.entityId, currentRaider.currentOwner.playerId));
            }
        }

        public void SetIsBoosting(bool isBoosting)
        {
            Raider raider = currentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetIsBoosting(isBoosting);
        }

        public void SetPrimaryFiring(bool isFiring)
        {
            Raider raider = currentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetPrimaryFire(isFiring);
        }

        public void SetSecondaryFiring(bool isFiring)
        {
            Raider raider = currentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetSecondaryFire(isFiring);
        }

        public void SetSpecialFiring(bool isFiring)
        {
            Raider raider = currentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetSpecialFire(isFiring);
        }

        public void SetEquipmentActive(bool isActive)
        {
            Raider raider = currentRaider;
            if (raider == null)
            {
                return;
            }

            raider.SetEquipmentActive(isActive);
        }
    }
}