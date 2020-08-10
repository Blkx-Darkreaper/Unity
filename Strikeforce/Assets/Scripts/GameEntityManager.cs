using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Strikeforce
{
    public class GameEntityManager : Manager
    {
        public static GameEntityManager singleton = null;
        protected int nextEntityId = 0;
        protected Dictionary<int, Entity> allGameEntities;
        public int maxEntities = 1000;

        private void Awake()
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

            this.allGameEntities = new Dictionary<int, Entity>();
        }

        public void RegisterEntity(Entity spawnedEntity)
        {
            int id;
            bool idInUse = true;
            do
            {
                id = GetNextUniqueId();
                idInUse = allGameEntities.ContainsKey(id);

                int count = allGameEntities.Count;
                if (count > maxEntities)
                {
                    RemoveEntity(spawnedEntity);
                    Debug.Log(string.Format("Entity limit reached"));
                    return;
                }

            } while (idInUse == true);

            spawnedEntity.entityId = id;
            allGameEntities.Add(id, spawnedEntity);
        }

        public Entity GetGameEntityById(int id)
        {
            bool entityExists = allGameEntities.ContainsKey(id);
            if (entityExists == false)
            {
                return null;
            }

            Entity entity = allGameEntities[id];
            return entity;
        }

        public void RemoveEntity(GameObject gameObjectToRemove)
        {
            Entity entityToRemove = gameObjectToRemove.GetComponent<Entity>();
            if (entityToRemove == null)
            {
                return;
            }

            RemoveEntity(entityToRemove);
        }

        public void RemoveEntity(Entity entityToRemove)
        {
            int id = entityToRemove.entityId;

            Entity toCheck = allGameEntities[id];
            if (toCheck != entityToRemove)
            {
                Debug.Log(string.Format("{0} entity does not match registered entity with id: {1}", entityToRemove.name, id));
                return;
            }

            allGameEntities.Remove(id);
            Destroy(entityToRemove.gameObject);
        }

        public int GetNextUniqueId()
        {
            int id = singleton.nextEntityId;
            singleton.nextEntityId++;
            if (singleton.nextEntityId >= int.MaxValue)
            {
                singleton.nextEntityId = 0;
            }

            return id;
        }
    }
}