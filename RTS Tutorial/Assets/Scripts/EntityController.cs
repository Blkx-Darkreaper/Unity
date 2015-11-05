using UnityEngine;
using System.Collections;
using RTS;

public class EntityController : MonoBehaviour
{

    public string entityName;
    public Texture2D buildImage;
    public int cost, sellValue, maxHitPoints;
    [HideInInspector]
    public int currentHitPoints;
    protected PlayerController owner;
    protected string[] actions = { };
    protected bool isSelected = false;
    protected Bounds selectionBounds;
    protected Rect playingArea = new Rect(0f, 0f, 0f, 0f);

    protected virtual void Awake()
    {
        selectionBounds = ResourceManager.invalidBounds;
        CalculateBounds();
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

    public bool OwnedByPlayer(PlayerController player)
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

        this.playingArea = owner.hud.GetPlayingArea();
        //this.playingArea = playingArea;
    }

    private void DrawSelection()
    {
        GUI.skin = ResourceManager.selectBoxSkin;
        Rect selectionBox = GameManager.CalculateSelectionBox(selectionBounds, playingArea);

        GUI.BeginGroup(playingArea);
        DrawSelectionBox(selectionBox);
        GUI.EndGroup();
    }

    protected virtual void DrawSelectionBox(Rect selectionBox)
    {
        GUI.Box(selectionBox, string.Empty);
    }

    public void CalculateBounds()
    {
        selectionBounds = new Bounds(transform.position, Vector3.zero);
        GameObject meshes = transform.Find("Meshes").gameObject;
        Renderer[] allRenderers = meshes.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            selectionBounds.Encapsulate(renderer.bounds);
        }
    }

    public string[] GetActions()
    {
        return actions;
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
        //if (hitEntity.name == Tags.ground)
        {
            return;
        }

        EntityController entity = hitEntity.GetComponentInParent<EntityController>();
        if (entity == null)
        {
            return;
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
        playingArea = player.hud.GetPlayingArea();
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
}
