using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Strikeforce;

public class Entity: Network Behaviour {
	public int entityId { get; set; }
	public static string nameProperty { get { return EntityProperties.NAME; } }
	protected bool isLoadedFromSave = false;
	protected struct Properties {
		public const string POSITION = "Position";
		public const string ROTATION = "Rotation";
		public const string SCALE = "Scale";
	}
	public Rect playingArea { get; set; }
	protected struct EntityProperties {
		public const string NAME = "Name";
		public const string ID = "Id";
		public const string MESHES = "Meshes";
	}

	protected virtual void Awake() {
		GameManager.activeInstance.RegisterGameEntity(this);

		playingArea = new Rect(0f, 0f, 0f, 0f);
	}

	public void SetColliders(bool enabled) {
		Collider[] allColliders = GetComponentsInChildren < Collider > ();
		foreach(Collider collider in allColliders) {
			collider.enabled = enabled;
		}
	}

	protected override void SaveDetails(JsonWriter writer) {
		SaveManager.SaveString(writer, EntityProperties.NAME, name);

		base.SaveDetails(writer);

		SaveManager.SaveInt(writer, EntityProperties.ID, entityId);
		SaveManager.SaveInt(writer, EntityProperties.HIT_POINTS, currentHitPoints);
		SaveManager.SaveBoolean(writer, EntityProperties.IS_ATTACKING, isAttacking);
		SaveManager.SaveBoolean(writer, EntityProperties.IS_ADVANCING, isAdvancing);
		SaveManager.SaveBoolean(writer, EntityProperties.IS_AIMING, isAiming);
		if (currentCooldownRemaining > 0) {
			SaveManager.SaveFloat(writer, EntityProperties.COOLDOWN, currentCooldownRemaining);
		}
		if (attackTarget != null) {
			SaveManager.SaveInt(writer, EntityProperties.TARGET_ID, attackTarget.entityId);
		}
	}

	protected override bool LoadDetails(JsonReader reader, string propertyName) {
		// Properties must be loaded in the order they were saved for loadCompleted to work properly
		bool loadCompleted = false;

		base.LoadDetails(reader, propertyName);

		switch (propertyName) {
		case EntityProperties.ID:
			entityId = LoadManager.LoadInt(reader);
			break;

		case EntityProperties.HIT_POINTS:
			currentHitPoints = LoadManager.LoadInt(reader);
			break;

		case EntityProperties.IS_ATTACKING:
			isAttacking = LoadManager.LoadBoolean(reader);
			break;

		case EntityProperties.IS_ADVANCING:
			isAdvancing = LoadManager.LoadBoolean(reader);
			break;

		case EntityProperties.IS_AIMING:
			isAiming = LoadManager.LoadBoolean(reader);
			loadCompleted = true; // Last property to load
			break;

		case EntityProperties.COOLDOWN:
			currentCooldownRemaining = LoadManager.LoadFloat(reader);
			loadCompleted = true;
			break;

		case EntityProperties.TARGET_ID:
			attackTargetId = LoadManager.LoadInt(reader);
			loadCompleted = true;
			break;
		}

		return loadCompleted;
	}

	protected override void LoadEnd(bool loadComplete) {
		base.LoadEnd(loadComplete);
		if (loadComplete == false) {
			return;
		}

		selectionBounds = ResourceManager.invalidBounds;
		UpdateBounds();
	}

	public void Save(JsonWriter writer) {
		if (writer == null) {
			return;
		}

		SaveStart(writer);
		SaveDetails(writer);
		SaveEnd(writer);
	}

	protected virtual void SaveStart(JsonWriter writer) {
		writer.WriteStartObject();
	}

	protected virtual void SaveDetails(JsonWriter writer) {
		SaveManager.SaveVector(writer, Properties.POSITION, transform.position);
		SaveManager.SaveQuaternion(writer, Properties.ROTATION, transform.rotation);
		SaveManager.SaveVector(writer, Properties.SCALE, transform.localScale); // Last property to save
	}

	protected virtual void SaveEnd(JsonWriter writer) {
		writer.WriteEndObject();

		Debug.Log(string.Format("Saved entity {0}, ", entityId, name));
	}

	public static void Load(JsonReader reader, GameObject gameObject) {
		if (reader == null) {
			return;
		}
		if (gameObject == null) {
			return;
		}

		PersistentEntity entity = gameObject.GetComponent < PersistentEntity > ();
		if (entity == null) {
			return;
		}

		bool loadingComplete = false;

		while (reader.Read() == true) {
			if (reader.Value == null) {
				if (reader.TokenType != JsonToken.EndObject) {
					continue;
				}

				entity.LoadEnd(loadingComplete);
				return;
			}

			if (reader.TokenType != JsonToken.PropertyName) {
				continue;
			}
			//            if (reader.TokenType != JsonToken.StartObject || reader.TokenType != JsonToken.StartArray)
			//            {
			//                continue;
			//            }

			string propertyName = LoadManager.LoadString(reader);
			reader.Read();

			loadingComplete = entity.LoadDetails(reader, propertyName);
		}

		entity.LoadEnd(loadingComplete);
	}

	public void Load(JsonReader reader) {
		if (reader == null) {
			return;
		}

		bool loadingComplete = false;

		while (reader.Read() == true) {
			if (reader.Value == null) {
				if (reader.TokenType != JsonToken.EndObject) {
					continue;
				}

				LoadEnd(loadingComplete);
				return;
			}

			if (reader.TokenType != JsonToken.PropertyName) {
				continue;
			}
			//            if (reader.TokenType != JsonToken.StartObject || reader.TokenType != JsonToken.StartArray)
			//            {
			//                continue;
			//            }
			string propertyName = LoadManager.LoadString(reader);
			reader.Read();

			loadingComplete = LoadDetails(reader, propertyName);
		}

		LoadEnd(loadingComplete);
	}

	protected virtual bool LoadDetails(JsonReader reader, string propertyName) {
		// Properties must be loaded in the order they were saved for loadCompleted to work properly
		bool loadCompleted = false;

		switch (propertyName) {
		case Properties.POSITION:
			transform.position = LoadManager.LoadVector(reader);
			break;

		case Properties.ROTATION:
			transform.rotation = LoadManager.LoadQuaternion(reader);
			break;

		case Properties.SCALE:
			transform.localScale = LoadManager.LoadVector(reader);
			loadCompleted = true; // Last property to load
			break;
		}

		return loadCompleted;
	}

	protected virtual void LoadEnd(bool loadingComplete) {
		if (loadingComplete == false) {
			Debug.Log(string.Format("Failed to load {0}", name));
			GameManager.activeInstance.DestroyGameEntity(gameObject);
			return;
		}

		isLoadedFromSave = true;
		Debug.Log(string.Format("Loaded {0}", name));
	}
}