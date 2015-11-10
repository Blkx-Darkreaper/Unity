using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class EntityController : MonoBehaviour
{

    public string entityName;
    public Texture2D buildImage;
    public int cost, sellValue, maxHitPoints;
    [HideInInspector]
    public int currentHitPoints;
    protected float healthPercentage;
    private const int HEALTH_BAR_VERTICAL_OFFSET = 7;
    private const int HEALTH_BAR_HEIGHT = 5;
    protected GUIStyle healthStyle = new GUIStyle();
    public PlayerController owner { get; set; }
    //protected string[] actionsValues = { };
    public string[] actions { get; protected set; }
    protected bool isSelected = false;
    //protected Bounds selectionBoundsValue;
    public Bounds selectionBounds { get; protected set; }
    public Rect playingArea { get; set; }
    private List<Material> oldMaterials = new List<Material>();

    protected virtual void Awake()
    {
        selectionBounds = ResourceManager.invalidBounds;
        UpdateBounds();

        currentHitPoints = maxHitPoints;
        UpdateHealthPercentage();

        playingArea = new Rect(0f, 0f, 0f, 0f);
    }

    protected virtual void Start()
    {
        owner = GetComponentInParent<PlayerController>();
        if (owner == null)
        {
            //Debug.Log(string.Format("{0} has no owner", entityName));
            return;
        }

        //Debug.Log(string.Format("{0} belongs to {1}", entityName, player.username));
    }

    protected virtual void Update()
    {
    }

    protected virtual void OnGUI()
    {
        if (isSelected == false)
        {
            return;
        }

        DrawSelection();
    }

    public bool CheckOwnedByPlayer(PlayerController player)
    {
        if (owner == null)
        {
            return false;
        }

        return owner.Equals(player);
    }

    public virtual void SetSelection(bool selected)
    {
        isSelected = selected;
        if (selected == false)
        {
            return;
        }

        this.playingArea = HudController.GetPlayingArea();
    }

    private void DrawSelection()
    {
        GUI.skin = ResourceManager.selectionBoxSkin;
        Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

        GUI.BeginGroup(playingArea);
        DrawSelectionBox(selectionBox);
        GUI.EndGroup();
    }

    protected virtual void DrawSelectionBox(Rect selectionBox)
    {
        GUI.Box(selectionBox, string.Empty);

        UpdateHealthPercentage();
        DrawHealthBar(selectionBox);
    }

    protected void DrawHealthBar(Rect selectionBox)
    {
        DrawHealthBarWithLabel(selectionBox, string.Empty);
    }

    protected void DrawHealthBarWithLabel(Rect selectionBox, string label)
    {
        healthStyle.padding.top = -20;
        healthStyle.fontStyle = FontStyle.Bold;

        float x = selectionBox.x;
        float y = selectionBox.y - HEALTH_BAR_VERTICAL_OFFSET;
        float width = selectionBox.width * healthPercentage;
        float height = HEALTH_BAR_HEIGHT;
        GUI.Label(new Rect(x, y, width, height), label, healthStyle);
    }

    protected virtual void UpdateHealthPercentage()
    {
        UpdateHealthPercentage(0.65f, 0.35f);
    }

    protected virtual void UpdateHealthPercentage(float healthyThreshold, float damagedThreshold)
    {
        if (maxHitPoints == 0)
        {
            healthPercentage = 0f;
            return;
        }

        healthPercentage = (float)currentHitPoints / (float)maxHitPoints;

        if (healthPercentage > healthyThreshold)
        {
            healthStyle.normal.background = ResourceManager.HealthBarTextures.healthy;
            return;
        }
        if (healthPercentage > damagedThreshold)
        {
            healthStyle.normal.background = ResourceManager.HealthBarTextures.damaged;
            return;
        }

        healthStyle.normal.background = ResourceManager.HealthBarTextures.critical;
    }

    public void UpdateBounds()
    {
        Bounds updatedBounds = new Bounds(transform.position, Vector3.zero);
        GameObject meshes = transform.Find("Meshes").gameObject;
        Renderer[] allRenderers = meshes.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            bool enabled = renderer.enabled;
            if (enabled == false)
            {
                continue;
            }

			updatedBounds.Encapsulate(renderer.bounds);
        }

        selectionBounds = updatedBounds;
    }

    public virtual void PerformAction(string actionToPerform)
    {
    }

    public virtual void MouseClick(GameObject hitEntity, Vector3 hitPoint, PlayerController player)
    {
        if (isSelected == false)
        {
            return;
        }
        if (hitEntity == null)
        {
            return;
        }
        bool isGround = hitEntity.CompareTag(Tags.ground);
        if (isGround == true)
        {
            return;
        }

        EntityController entity = hitEntity.GetComponentInParent<EntityController>();
        if (entity == null)
        {
            return;
        }

        ResourceController resource = hitEntity.GetComponentInParent<ResourceController>();
        if (resource != null)
        {
            bool resourceDepleted = resource.isEmpty;
            if (resourceDepleted == true)
            {
                return;
            }
        }

        ChangeSelection(entity, player);
    }

    private void ChangeSelection(EntityController otherEntity, PlayerController player)
    {
        if (otherEntity == this)
        {
            return;
        }

        SetSelection(false);
        if (player.selectedEntity != null)
        {
            player.selectedEntity.SetSelection(false);
        }

        player.selectedEntity = otherEntity;
        playingArea = HudController.GetPlayingArea();
        otherEntity.SetSelection(true);
    }

    public virtual void SetOverState(GameObject entityUnderMouse)
    {
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
        if (isGround == true)
        {
            return;
        }

        owner.hud.SetCursorState(CursorState.select);
    }

    public void SetColliders(bool enabled)
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            collider.enabled = enabled;
        }
    }

    public void SetTransparencyMaterial(Material transparencyMaterial, bool saveCurrentMaterial)
    {
        if (saveCurrentMaterial == true)
        {
            oldMaterials.Clear();
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            if (saveCurrentMaterial == true)
            {
                oldMaterials.Add(renderer.material);
            }

            renderer.material = transparencyMaterial;
        }
    }

    public void RestoreMaterials()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        int totalRenderers = allRenderers.Length;
        if (oldMaterials.Count != totalRenderers)
        {
            return;
        }

        for (int i = 0; i < totalRenderers; i++)
        {
            allRenderers[i].material = oldMaterials[i];
        }
    }
}
