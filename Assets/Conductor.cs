using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    public GameObject TileGeneration;
    public GameObject WaterControl;
    
    void Start()
    {
        WaterControl.SetActive(true); // Water control is inactive normally because it obscures working on prefabs
        SetupGame();
        StartCoroutine(UpdateLoop());
    }
    
    IEnumerator UpdateLoop()
    {
        while (true)
        {
            if(TileGeneration.transform.childCount > 0) // Prevents update during restart
            {
                TileGeneration.GetComponent<HexTileMapGenerator>().UpdateAll();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void RestartGame() // Currently this is broken
    {
        foreach(Transform child in TileGeneration.transform)
        {
            Destroy(child.gameObject);
        }
        SetupGame();
    }

    void SetupGame()
    {
        TileGeneration.GetComponent<HexTileMapGenerator>().CreateHexTileMap(); // Place Tiles
        TileGeneration.GetComponent<HexTileMapGenerator>().GrabNeighborsAll(); // Have each tile fill its ledger with its neighbors
        TileGeneration.GetComponent<HexTileMapGenerator>().RandomOutflows(); // Have each tile pick a downhill (or same level) neighbor to send water toward
    }

}
