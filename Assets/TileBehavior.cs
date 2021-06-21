using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{
    public class Neighbor // Ledger entry item containing
    {
        public Transform tile;
        public int flowRate = 0; // Where a positive flowRate indicates "income" to this tile
        public int dirTo; // Integer angle where 0 is North (positive Z axis) and 
    }
    
    public GameObject LayIndicator; // For debugging
    public GameObject ArrowPrefab; // "River"
    public GameObject LakePrefab;
    public GameObject ForestPrefab;
    public bool hasFlowArrowOut = false;
    public bool hasLake = false;
    public bool hasForest = false;

    public int Ring; // Where 0 is the center tile
    public int Index; // Where 0 is the Northmost and counting goes clockwise
    public bool isEdge = true; // Edge of map
    public bool isEndEdge = false; // An end tile which sends water off the map instead of to a neighbor
    public bool isLake = false;

    public int flowFromGod = 0; //Initially zero, this value is affected by user input for the Ring 1 tiles
    public int TotalFlowIn; // This value is used by neighbors to decide if they are forests etc
    public int FlowNextTurn; // This value is altered by all tiles during update before making any changes to themselves
    public int NearbyFlow = 0; // Used for rules on growing forests etc

    public float grade = 0.5f; // Defines the slope step of the mountain. Larger is steeper.

    public Neighbor OutflowTarget; // Currently rivers will always combine into one (as in reality)
    public int dirToOutflow; // Integer direction to the outflow target neighbor

    public List<Neighbor> Ledger = new List<Neighbor>(); // Contains information on adjacent tiles

    void Start()
    {
        this.gameObject.transform.Translate(0, Ring * -grade, 0); // Move down to simulate mountain
        this.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Ring + ", " + Index; // List coordinates on top
    }

    int intAngleTo(Vector3 position) // Where 0 is North also fuck this entire function
    {
        Vector3 normalizedVector = (position - this.transform.position).normalized;
        
        float angleRad = Mathf.Atan2(normalizedVector.x, normalizedVector.z); // Returns 0 radians for positive z axis direction

        if(angleRad < 0)
        {
            angleRad += (2 * 3.14159f) + 1; // Add 1 full circle to negative radian measurements, plus 1 to keep the index at 0
        }
        
        int angleInt = (int)(angleRad * (6 / (2 * 3.14159f))); // Convert radians to 0-6 integer

        if(angleInt == 6) // This is necessary because I suck at integer math
        {
            angleInt = 0;
        }
        
        return angleInt;
    }

    public void GetNeighbors()
    {
        foreach (Transform tile in this.transform.parent) // Checking all tiles
        {
            int targetRing = tile.transform.GetComponent<TileBehavior>().Ring; //Note the ring
            int targetIndex = tile.transform.GetComponent<TileBehavior>().Index; //Note the index

            if (Ring == 0) //Error catching for central tile
            {
                break; // Center tile might not even need to exist, since flow is defined directly at the 1-Ring tiles
            }

            if ((targetRing == Ring && targetIndex == Index - 1) || (targetIndex == Ring * 6 - 1 && Index == 0 && targetRing == Ring)) //CCW neighbor on the same Ring
            {
                //same ring AND one lower index OR if this tile is the 0 tile, then grab the end-tile
                Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
            }

            if ((targetRing == Ring && targetIndex == Index + 1) || (Index == Ring * 6 - 1 && targetIndex == 0 && targetRing == Ring)) //CW neighbor on the same Ring
            {
                //same ring AND one lower index OR if this tile is the end-tile, then grab the 0 tile
                Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
            }

            if (Index % Ring == 0) // CORNER TILES (will always be an integer multiple of the Ring number)
            {
                if (targetIndex == (Index / Ring) * targetRing && targetRing == Ring + 1) //The corner tile below
                {
                    // The corner tile above will have the same index/ring ratio
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetIndex - 1 == (Index / Ring) * targetRing && targetRing == Ring + 1) //The lower CW tile
                {
                    // Tile after the lower corner tile
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetIndex + 1 == (Index / Ring) * targetRing && targetRing == Ring + 1 || (Index == 0 && targetIndex + 1 == targetRing * 6 && targetRing == Ring + 1)) //The lower CCW tile
                {
                    // Tile before the lower corner tile (with loop-around catch for the 0-tiles)
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetIndex == (Index / Ring) * targetRing && targetRing == Ring - 1) //The corner tile above
                {
                    // The corner tile above will have the same index/ring ratio
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
            }
            else // NON-CORNER TILES
            {
                if ((targetRing == Ring - 1 && targetIndex == Index - (Index / Ring)) || (Index + 1 == Ring * 6 && targetIndex == 0 && targetRing == Ring - 1)) //Upper CW tile
                {
                    // Index values decrease linearly downhill, at a rate defined by the "wedge" the tile belongs to
                    // Index / Ring defines the sixth of the mountain the tile sits in, and is also the growth rate in that wedge downward
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetRing == Ring - 1 && targetIndex + 1 == Index - (Index / Ring)) //Upper CW tile
                {
                    //Neighbors.Add(tile);
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetRing == Ring + 1 && targetIndex == Index + (Index / Ring)) //Lower CCW tile
                {
                    //Neighbors.Add(tile);
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
                if (targetRing == Ring + 1 && targetIndex - 1 == Index + (Index / Ring)) //Lower CW tile
                {
                    //Neighbors.Add(tile);
                    Ledger.Add(new Neighbor { tile = tile, dirTo = intAngleTo(tile.position) });
                }
            }
        }

        // Notes tiles which are on the edge of the map
        foreach (Neighbor h in Ledger)
        {
            if (h.tile.GetComponent<TileBehavior>().Ring > Ring) //Checks if there is at least one lower-ring tile
            {
                isEdge = false;
            }
        }
    }

    public void RandomOutflow() // Pick a same-level or downhill neighbor to send water to
    {

        System.Random rnd = this.GetComponentInParent<HexTileMapGenerator>().rnd;

        if (Ring == 0) // Error catching for the center tile
        {
            return;
        }

        if (isEdge) // Randomly decide if an edge would flow to a neighbor or send water off the map
        {
            if(rnd.Next(0,5) >= 2) // one in three chance it will not be an end edge
            {
                isEndEdge = true;
                return;
            }
        }
        
        Neighbor target = Ledger[rnd.Next(0, Ledger.Count)];

        while(target.tile.gameObject.GetComponent<TileBehavior>().Ring < Ring) // Keep picking random neighbors until it isn't an uphill tile
        {
            target = Ledger[rnd.Next(0, Ledger.Count)];
        }

        OutflowTarget = target;
        dirToOutflow = target.dirTo;

        PlaceLayIndicator(); // Display outflow direction (for debugging)
    }

    void PlaceLayIndicator() // This should use dirTo and not instantiation in this way
    {
        GameObject h = Instantiate(LayIndicator, ((OutflowTarget.tile.position - this.transform.position) / 2) + this.transform.position, LayIndicator.transform.rotation, this.transform);
        //GameObject h = Instantiate(FlowArrowPrefab, ((OutflowTarget.position - this.transform.position) / 2) + this.transform.position, Quaternion.LookRotation(OutflowTarget.position).normalized, this.transform);
        h.transform.LookAt(OutflowTarget.tile);
        h.transform.localPosition = new Vector3(h.transform.localPosition.x, 0, h.transform.localPosition.z);
        //Debug.Log("Outflow target is set to " + OutflowTarget.name);
    }

    public void PrepareTurn() // Poll water income and neighbor states without making outward-facing changes to the tile
    {
        FlowNextTurn = flowFromGod; //Start from baseline (zero in most cases)

        foreach (Neighbor n in Ledger) // Sum all flow in
        {
            if(n.flowRate > 0)
            {
                FlowNextTurn += n.flowRate;
                //Debug.Log(n.tile.name + " added " + n.flowRate + " water to " + this.name);
            }
        }

        NearbyFlow = 0; // Note the nearby flow
        foreach (Neighbor n in Ledger)
        {
            NearbyFlow += n.tile.GetComponent<TileBehavior>().TotalFlowIn;
        }
    }

    public void PerformTurn() // Nothing here should be reading from the Ledger at this point
    {
        TotalFlowIn = FlowNextTurn; // New outward-facing flow value is updated now

        if (TotalFlowIn > 0 && !this.isEndEdge && OutflowTarget.flowRate <= 0) // If receiving water, isn't an edge, and isn't trying to backflow
        {
            foreach (Neighbor n in OutflowTarget.tile.GetComponent<TileBehavior>().Ledger)
            {
                if (n.tile == this.transform)
                {
                    n.flowRate = TotalFlowIn; //Inform the target that it is receiving water within its ledger
                }
            }
        }

        if (TotalFlowIn > 0) // Tiles which receive water
        {
            if (hasForest)
            {
                Destroy(this.transform.Find("Forest").gameObject); // Forests are destroyed by rivers
                hasForest = false;
            }

            if (!isEndEdge) // End edges will become deltas / send water off-map eventually
            {
                if (OutflowTarget.flowRate > 0) // Backflowing tiles
                {
                    isLake = true; // Become lakes
                    if (!hasLake)
                    {
                        GameObject h = Instantiate(LakePrefab, this.transform);
                        h.name = string.Format("Lake");
                        this.hasLake = true;
                    }
                }

                if (!isLake) // Through-flowing tiles
                {
                    if (!this.hasFlowArrowOut) // To be revised into a separate script (?)
                    {
                        GameObject h = Instantiate(ArrowPrefab, this.transform.Find("Node" + dirToOutflow)); // Arrow will point away, placed on the node toward the target tile
                        h.name = string.Format("Flow" + dirToOutflow);
                        this.hasFlowArrowOut = true;
                    }
                }
            }
            // END EDGE WATERFALL / DELTA CREATION HERE
        } else
        {
            // Tiles that aren't part of the river system (forests, towns, jungle etc)
            if (NearbyFlow >= 2 && !hasForest) // Two river tiles nearby, or one river with 2 or more flow within it
            {
                GameObject h = Instantiate(ForestPrefab, this.transform.position, ForestPrefab.transform.rotation, this.transform);
                h.name = string.Format("Forest");
                this.hasForest = true;
            }
        }                
    }
}
