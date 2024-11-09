using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject mouselndicator, cellIndicator;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private ObjectsDatabaseSO database;
    private int selectedObjectIndex = -1;

    [SerializeField]
    private GameObject gridVisualization;

    private GridData floorData, furnitureData;

    private Renderer previewRenderer;

    private List<GameObject> placedGameObjects = new();

    private void Start()
    {
        StopPlacement();
        floorData = new();
        furnitureData = new();
        previewRenderer =cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        selectedObjectIndex = database.objectsData.FindIndex(data => data.ID == ID);
        if(selectedObjectIndex < 0)
        {
            Debug.LogError($"No ID Found{ID}");
            return;
        }

        gridVisualization.SetActive(true);
        cellIndicator.SetActive(true);
        inputManager.OnClicked += placeStructure;
        inputManager.OnExit += StopPlacement;
    }

    private void placeStructure()
    {
        if (inputManager.IsPointerOverUI())
        {
            return;
        }

        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        if (placementValidity == false)
            return;

        GameObject newObject = Instantiate(database.objectsData[selectedObjectIndex].Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition);
        placedGameObjects.Add( newObject );
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData :
            furnitureData;
        selectedData.AddObjectAt(gridPosition, database.objectsData[selectedObjectIndex].Size, 
                                               database.objectsData[selectedObjectIndex].ID,
                                               placedGameObjects.Count -1);
    }

    private bool CheckPlacementValidity(Vector3Int gridPosition, int selectedObjectIndex)
    {
        GridData selectedData = database.objectsData[selectedObjectIndex].ID == 0 ?
            floorData : 
            furnitureData;

           return selectedData.CanPlaceObejctAt(gridPosition, database.objectsData[selectedObjectIndex].Size);
    }

    public void StopPlacement()
    {
        selectedObjectIndex = -1;
        gridVisualization.SetActive(false);
        cellIndicator.SetActive(false);
        inputManager.OnClicked -= placeStructure;
        inputManager.OnExit -= StopPlacement;

    }

    private void Update()
    {
        if (selectedObjectIndex < 0)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        bool placementValidity = CheckPlacementValidity(gridPosition, selectedObjectIndex);
        previewRenderer.material.color = placementValidity ? Color.white : Color.red;

        mouselndicator.transform.position = mousePosition;
        cellIndicator.transform.position = grid.CellToWorld(gridPosition);
    }

}
