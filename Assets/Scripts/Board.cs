// Michael Camara, 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour {

    // Jagged array for representing holes on board
    private Hole[][] holes;

    // Transform used for instantiating Hole prefab
    public Transform holePrefab;

    // UI transform used for positioning holes on the board
    private RectTransform rectTrans;

    // UI Slider to control number of rows
    public Slider rowSlider;
    private int numRows;

    public GameManager manager;

    private void Awake() {
        if (rowSlider == null) {
            Debug.LogError("Could not find UnityEngine.UI.Slider on " + transform);
        }

        rectTrans = GetComponent<RectTransform>();
        
        if(rectTrans == null) {
            Debug.LogError("Could not find UnityEngine.UI.RectTransform rectTrans on " + transform);
        }

        if(manager == null) {
            Debug.LogError("Could not find GameManager manager on " + transform);
        }
    }

    public Hole[][] CreateNewBoard() {

        // Destroy existing holes/pegs
        if (holes != null) {
            foreach (Hole[] subarray in holes) {
                foreach (Hole h in subarray) {
                    Destroy(h.gameObject);
                }
            }
        }

        // Create jagged array of holes based on number of rows selected in UI
        numRows = (int)rowSlider.value;
        holes = CreateJaggedArray(numRows);

        // Create edges between nodes
        AssignNeighborsByDirection(holes);

        return holes;
    }

    /// <summary>
    /// Creates a jagged array of Hole objects to represent triangular shape of peg board.
    /// 2D Array takes the general form of:
    /// {0}, 
    /// {1, 2}, 
    /// {3, 4, 5}, 
    /// {6, 7, 8, 9}.
    /// {10, 11, 12, 13, 14} ...
    /// </summary>
    private Hole[][] CreateJaggedArray(int numRows) {

        Hole[][] holes = new Hole[numRows][];

        // Get the dimensions on the current canvas size (scaled by current resolution)
        Rect rect = rectTrans.rect;
        
        // How far the next hole is placed along each axis
        int xStep = (int) rect.width / numRows;
        int yStep = (int) rect.height / numRows;

        for (int row = 0, pegID = 0, xStart = 0, yStart = (int)(-yStep / 1.5f); row < numRows; row++, xStart -= xStep / 2, yStart -= yStep) {

            holes[row] = new Hole[row + 1];

            for (int i = 0; i < row + 1; i++, pegID++) {

                Transform currHoleTransform = Instantiate(holePrefab, Vector3.zero, Quaternion.identity, transform);

                // Move the placed hole relative to its pivot on the UI canvas
                RectTransform currRectTransform = currHoleTransform.GetComponent<RectTransform>();
                currRectTransform.localPosition += new Vector3(xStart + xStep * i, yStart, 0);

                // Add the Hole object to our jagged array
                Hole currHole = currHoleTransform.GetComponent<Hole>(); 
                if(currHole == null) {
                    Debug.LogError("Could not find Hole hole on " + currHoleTransform);
                }
                holes[row][i] = currHole;

                // Assign listener to the hole's button, referencing method from GameManager
                Button button = currHole.GetComponent<Button>();
                button.onClick.AddListener(delegate { manager.OnButtonClicked(currHole); });

                // Assign unique ID to hole object
                currHole.SetIDText(pegID);
            }
        }

        return holes;
    }

    /// <summary>
    /// Populate edges contained in each node that refer to their neighbors by relative direction
    /// </summary>
    private void AssignNeighborsByDirection(Hole[][] holes) {

        for (int row = 0; row < holes.Length; row++) {
            for (int col = 0; col < holes[row].Length; col++) {

                Hole currHole = holes[row][col];

                // Assign neighbors "north" of position
                if (row - 1 >= 0) {
                    if (holes[row - 1].Length > col) {
                        currHole.AddNeighborByDirection(Hole.EdgeDirection.NE, holes[row - 1][col]);
                    }

                    if (col - 1 >= 0) {
                        currHole.AddNeighborByDirection(Hole.EdgeDirection.NW, holes[row - 1][col - 1]);
                    }
                }

                // Assign neighbors "south" of position
                if (row + 1 < holes.Length) {
                    currHole.AddNeighborByDirection(Hole.EdgeDirection.SW, holes[row + 1][col]);

                    if (col + 1 < holes[row + 1].Length) {
                        currHole.AddNeighborByDirection(Hole.EdgeDirection.SE, holes[row + 1][col + 1]);
                    }
                }

                // Assign neighbors "east" and "west" of position
                if (col + 1 < holes[row].Length) {
                    currHole.AddNeighborByDirection(Hole.EdgeDirection.E, holes[row][col + 1]);
                }
                if (col - 1 >= 0) {
                    currHole.AddNeighborByDirection(Hole.EdgeDirection.W, holes[row][col - 1]);
                }
            }
        }
    }

    public Hole[][] GetHoles() {
        return holes; 
    }

    public int GetTotalPegs() {
        return numRows * (numRows + 1) / 2;
    }
}