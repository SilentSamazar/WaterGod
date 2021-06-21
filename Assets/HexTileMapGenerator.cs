using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileMapGenerator : MonoBehaviour
{
    public System.Random rnd = new System.Random();

    public GameObject TilePrefab;
    public uint Radius; // Defined in Inspector, sets the map size
    public float HexSideMultiplier = 8; // Defines distance between tiles

    private const float sq3 = 1.7320508075688772935274463415059F;
    
    public void CreateHexTileMap() // Stolen from the internet
    {
        Vector3 currentPoint = transform.position;

        Vector3[] mv = {
            new Vector3(1.5f,0, -sq3*0.5f),       //DR
            new Vector3(0,0, -sq3),               //DX
            new Vector3(-1.5f,0, -sq3*0.5f),      //DL
            new Vector3(-1.5f,0, sq3*0.5f),       //UL
            new Vector3(0,0, sq3),                //UX
            new Vector3(1.5f,0, sq3*0.5f)         //UR
        };

        int lmv = mv.Length;
        float HexSide = TilePrefab.transform.localScale.x * HexSideMultiplier;

        for (int mult = 0; mult <= Radius; mult++)
        {
            int hn = 0;
            for (int j = 0; j < lmv; j++)
            {
                for (int i = 0; i < mult; i++, hn++)
                {
                    GameObject h = Instantiate(TilePrefab, currentPoint, TilePrefab.transform.rotation, transform);
                    h.name = string.Format("Hex Ring: {0}, n: {1}", Ring(mult, hn), Index(mult, hn));
                    h.GetComponent<TileBehavior>().Ring = Ring(mult, hn);
                    h.GetComponent<TileBehavior>().Index = Index(mult, hn);
                    currentPoint += (mv[j] * HexSide);
                }
                if (j == 4)
                {
                    GameObject h = Instantiate(TilePrefab, currentPoint, TilePrefab.transform.rotation, transform);
                    h.name = string.Format("Hex Ring: {0}, n: {1}", Ring(mult, hn), Index(mult, hn));
                    h.GetComponent<TileBehavior>().Ring = Ring(mult, hn);
                    h.GetComponent<TileBehavior>().Index = Index(mult, hn);
                    currentPoint += (mv[j] * HexSide);
                    hn++;
                    if (mult == Radius)
                        break;      //Finished
                }
            }
        }
    }

    int Ring(int mult, int hn) // Since the tile map creator spirals outward, mult and hn loop before the 6th side of the ring
    {
        if(mult == 0)
        {
            return 0;
        }

        if (hn > mult*5 ) // Tiles on the sixth side belong to the next-numbered ring
        {
            return mult + 1;
        } else
        {
            return mult;
        }
    }

    int Index(int mult, int hn)
    {
        if (hn > mult * 5) //The generation has begun a new ring
        {
            return ((mult + 1) * 6) - mult + (hn - mult * 5) - 1;
        }
        else
        {
            return hn;
        }
    }

    public void GrabNeighborsAll()
    {
        foreach (Transform child in this.transform)
        {
            child.GetComponent<TileBehavior>().GetNeighbors();
        }

    }

    public void RandomOutflows()
    {
        foreach (Transform child in this.transform)
        {
            child.GetComponent<TileBehavior>().RandomOutflow();
        }
    }

    public void UpdateAll()
    {
        foreach (Transform child in this.transform) // Data collection only, no outward changes to tiles
        {
            child.GetComponent<TileBehavior>().PrepareTurn();
        }
        foreach (Transform child in this.transform) // Update tile states
        {
            child.GetComponent<TileBehavior>().PerformTurn();
        }
    }

    public void toggleFlow(int dir) // User-added flow function
    {
        int targetflow = this.transform.Find("Hex Ring: 1, n: " + dir).GetComponent<TileBehavior>().flowFromGod;

        if(targetflow > 0)
        {
            targetflow = 0;
        }
        else
        {
            targetflow = 1;
        }

        this.transform.Find("Hex Ring: 1, n: " + dir).GetComponent<TileBehavior>().flowFromGod = targetflow;
    }
}

