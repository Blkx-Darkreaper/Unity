using UnityEngine;
using System.Collections;
using RTS;
using Random = UnityEngine.Random;
using System;

public class OreDeposit : ResourceController {

    public GameObject oreCrystal;
    public int startingBlocks;
    public bool autoGenerate;
    private int blocksShowing;

	protected override void Awake ()
	{
		if (autoGenerate == true)
		{
			AddBlocks(startingBlocks);
		}

		base.Awake ();
	}

    protected override void Start()
    {
        base.Start();
        blocksShowing = startingBlocks;
        //startingBlocks = GetComponentsInChildren<OreResource>().Length;
        type = ResourceType.ore;
    }

    protected void AddBlocks(int blocksToAdd)
    {
        if (blocksToAdd < 0)
        {
            throw new UnityException("Blocks to add must not be negative");
        }
        
		GameObject meshes = transform.Find(EntityProperties.MESHES).gameObject;
		if (meshes == null)
		{
			return;
		}
        for (int i = 1; i <= blocksToAdd; i++)
        {
            GameObject cubeToAdd = (GameObject) Instantiate(oreCrystal, meshes.transform.position, Quaternion.identity);

            cubeToAdd.name = i.ToString();
            cubeToAdd.transform.parent = meshes.transform;

            float x = (float)Math.Round(Random.Range(-0.17f, 0.39f), 2);
            float y = (float)Math.Round(Random.Range(-0.03f, 0.25f), 2);
            float z = (float)Math.Round(Random.Range(-0.21f, 0.25f), 2);
            //cubeToAdd.transform.position = new Vector3(x, y, z);
            cubeToAdd.transform.localPosition = new Vector3(x, y, z);

            x = (float)Math.Round(Random.Range(-5.5f, 355), MidpointRounding.AwayFromZero) / 2;
            y = (float)Math.Round(Random.Range(0f, 341f), MidpointRounding.AwayFromZero) / 2;
            z = (float)Math.Round(Random.Range(0f, 342f), MidpointRounding.AwayFromZero) / 2;
            cubeToAdd.transform.eulerAngles = new Vector3(x, y, z);
        }
    }

    protected override void Update()
    {
        base.Update();
        float percentageLeft = (float)currentAmount / startingAmount;
        percentageLeft = Mathf.Clamp(percentageLeft, 0, float.MaxValue);

        int blocksToShow = (int)(percentageLeft * startingBlocks);
        //if (totalBlocksToShow < 0)
        //{
        //    return;
        //}
        if (blocksToShow == blocksShowing)
        {
            return;
        }

		GameObject meshes = transform.Find(EntityProperties.MESHES).gameObject;
		Renderer[] blocks = meshes.GetComponentsInChildren<Renderer>();

        int startingIndex = Mathf.Min(blocksShowing, blocksToShow);
        int endingIndex = Mathf.Max(blocksShowing, blocksToShow);
        for (int i = startingIndex; i < endingIndex; i++)
        {
            if (i < blocksToShow)
            {
                blocks[i].enabled = true;
            }
            else
            {
                blocks[i].enabled = false;
            }
        }

        //int totalBlocks = blocks.Length;
        //for (int i = 0; i < totalBlocks; i++)
        //{
        //    if (i < totalBlocksToShow)
        //    {
        //        blocks[i].enabled = true;
        //    }
        //    else
        //    {
        //        blocks[i].enabled = false;
        //    }
        //}
        blocksShowing = blocksToShow;

        //int startingIndex = blocks.Length - 1;
        //for (int i = startingIndex; i > totalBlocksToShow; i--)
        //{
        //    blocks[i].enabled = false;
        //}

        //GameObject[] blocks = GetComponentsInChildren<GameObject>();
        //if (totalBlocksToShow >= blocks.Length)
        //{
        //    return;
        //}

        //GameObject[] sortedBlocks = new GameObject[blocks.Length];
        //foreach (GameObject oreBlock in blocks)
        //{
        //    sortedBlocks[blocks.Length - int.Parse(oreBlock.name)] = oreBlock;
        //}
        //for (int i = totalBlocksToShow; i < sortedBlocks.Length; i++)
        //{
        //    sortedBlocks[i].GetComponent<Renderer>().enabled = false;
        //}

        UpdateBounds();
    }
}