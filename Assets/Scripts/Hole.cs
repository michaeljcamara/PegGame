// Michael Camara, 2018

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hole : MonoBehaviour {
    
    // Direction from one hole to an adjacent hole
    public enum EdgeDirection {E, W, NE, NW, SE, SW};
    public Dictionary<EdgeDirection, Hole> neighborsByEdge;

    // UI References
    public Image pointingImage;
    public Image buttonImage;
    public Image highlightImage;
    private Text buttonText;

    // Store which first-degree neighbor would be jumped over (value) based on second-degree neighbor (key)
    // Ex: (this hole) --EAST--> (first neighbor) --EAST--> (second neighbor)
    private Dictionary<Hole, Hole> firstNeighborLookup;

    // True if hole has a peg in it currently
    private bool bHasPeg;

    // True if hole has a peg that can jump over neighboring peg
    private bool bIsValidSelection;

    // True if hole is empty and can be jumped into by second-degree neighbor
    private bool bIsValidDestination;

    private void Awake() {
        bHasPeg = true;
        neighborsByEdge = new Dictionary<EdgeDirection, Hole>();
        firstNeighborLookup = new Dictionary<Hole, Hole>();

        //Ensure icon is initially invisible
        pointingImage.canvasRenderer.SetAlpha(0f);

        buttonText = GetComponentInChildren<Text>(true);

        if (buttonText == null) {
            Debug.LogError("Could not find UnityEngine.UI.text buttonText in children of " + transform);
        }

        if (pointingImage == null) {
            Debug.LogError("Could not find UnityEngine.UI.Image pointingImage on " + transform);
        }

        if(buttonImage == null) {
            Debug.LogError("Could not find UnityEngine.UI.Image buttonImage on " + transform);
        }

        if (highlightImage == null) {
            Debug.LogError("Could not find UnityEngine.UI.Image highlightImage on " + transform);
        }
    }

    /// <summary>
    /// Check if any neighboring pegs can be jumped over into an empty hole along same direction.
    /// If so, mark this hole as valid for selection, and store valid first and second neighbor pairs
    /// for future reference.
    /// </summary>
    public bool CheckValidMoves() {

        // Reset these vars each time
        bIsValidSelection = false;
        firstNeighborLookup.Clear();

        // Not valid for selection if no peg in this hole
        if(!bHasPeg) {
            return false;
        }

        foreach(KeyValuePair<EdgeDirection, Hole> pair in neighborsByEdge) {

            Hole closeNeighbor = pair.Value;
            EdgeDirection direction = pair.Key;

            if(closeNeighbor.bHasPeg) {

                // Check if second neighbor, along same direction as first neighbor, has a peg in it;
                // If not, the first neighbor can be jumped over
                Hole farNeighbor = closeNeighbor.GetNeighborByDirection(direction);

                if(farNeighbor != null && !farNeighbor.bHasPeg) {

                    firstNeighborLookup.Add(farNeighbor, closeNeighbor);
                    bIsValidSelection = true;
                }
            }
        }

        return bIsValidSelection;
    }

    /// <summary>
    /// Given the second-degree neighbor of this hole, return the first-degree neighbor
    /// that would be "jumped over" by the peg in this hole and removed from the board.
    /// </summary>
    public Hole GetRemovedPeg(Hole secondNeighbor) {
        Hole removed;
        firstNeighborLookup.TryGetValue(secondNeighbor, out removed);
        return removed;
    }

    public void AddNeighborByDirection(EdgeDirection dir, Hole neighbor) {
        neighborsByEdge.Add(dir, neighbor);
    }

    public Hole GetNeighborByDirection(EdgeDirection dir) {
        Hole neighbor;
        neighborsByEdge.TryGetValue(dir, out neighbor);
        return neighbor;
    }

    public void SetHighlight(bool bIsOn) {

        if (bIsOn) {
            highlightImage.enabled = true;
        }
        else {
            highlightImage.enabled = false;
        }
        
        // Also update color to reflect whether hole has peg or not
        if (!bHasPeg) {
            buttonImage.CrossFadeColor(Color.black, 0.2f, false, true);
        }
        else {
            buttonImage.CrossFadeColor(Color.white, 0.2f, false, true);
        }
    }

    public void SetPointingIcon(bool bIsOn) {
        float alpha = (bIsOn) ? 200 : 0;
        pointingImage.CrossFadeAlpha(alpha, 0.3f, false);
    }

    public void SetIDText(int id) {
        buttonText.text = id.ToString();
    }

    public void SetPeg(bool bIsSet) {
        bHasPeg = bIsSet;
    }

    public bool IsValidSelection() {
        return bIsValidSelection;
    }

    public bool IsValidDestination() {
        return bIsValidDestination;
    }

    public void SetValidDestination(bool bIsValid) {
        bIsValidDestination = bIsValid;
    }

    public Dictionary<Hole,Hole> GetFirstNeighborLookup() {
        return firstNeighborLookup;
    }
}