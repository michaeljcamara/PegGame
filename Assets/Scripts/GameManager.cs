// Michael Camara, 2018

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Board board;

    // Jagged array of all holes instantiated on game board
    private Hole[][] holes;

    // When the number of pegs on board == 1, player wins
    private int remainingPegs;

    // UI text that tells player what to do next
    public Text instructionText;

    // Sequence of phases the game proceeds through in order
    public enum GamePhase { RemoveFirstPeg, CheckAllValidMoves, WaitForPegSelection, HighlightValidDestinations, WaitForDestinationSelection, MovePiece, GameOver }
    private GamePhase currentPhase;

    // Piece selected by player by clicking on occupied hole on board
    private Hole selectedHole;

    // Destination of piece selected by player clicking on empty hole on board
    private Hole destinationHole;

    void Awake() {
        if (instructionText == null) {
            Debug.LogError("Could not find UnityEngine.UI.Text instructionText on " + transform);
        }

        if (board == null) {
            Debug.LogError("Could not find Board board on " + transform);
        }

        holes = board.GetHoles();
    }

    void Start() {
        ResetGame();
    }

    public void ResetGame() {
        // Create new board
        holes = board.CreateNewBoard();

        // Return these vars to default vals
        selectedHole = null;
        destinationHole = null;
        instructionText.color = Color.black;
        currentPhase = GamePhase.RemoveFirstPeg;
        remainingPegs = board.GetTotalPegs();
    }

    /// <summary>
    /// Branch execution each frame based on current GamePhase
    /// </summary>
    void Update() {
        switch (currentPhase) {

            case GamePhase.RemoveFirstPeg:
                RemoveFirstPeg();
                break;

            case GamePhase.CheckAllValidMoves:
                CheckAllValidMoves();
                break;

            case GamePhase.WaitForPegSelection:
                WaitForPegSelection();
                break;

            case GamePhase.HighlightValidDestinations:
                HighlightValidMoves();
                break;

            case GamePhase.WaitForDestinationSelection:
                WaitForDestinationSelection();
                break;

            case GamePhase.MovePiece:
                MovePiece();
                break;

            case GamePhase.GameOver:
                GameOver();
                break;
            default:
                Debug.LogError("Invalid game phase detected");
                break;
        }
    }

    void RemoveFirstPeg() {
        instructionText.text = "Click first peg to remove";

        if (selectedHole != null) {
            remainingPegs--;
            selectedHole.SetPeg(false);
            selectedHole = null;
            currentPhase = GamePhase.CheckAllValidMoves;
        }
    }

    void CheckAllValidMoves() {
        bool bIsMovePossible = false;

        // Check every node to see if it has a valid move available
        foreach (Hole[] subArray in holes) {
            foreach (Hole hole in subArray) {

                bool hasValidMove = hole.CheckValidMoves();

                // Highlight the node if move detected, otherwise remove existing highlight
                if (hasValidMove) {
                    bIsMovePossible = true;
                    hole.SetHighlight(true);
                }
                else {
                    hole.SetHighlight(false);
                }
            }
        }

        // End game if no moves are possible; otherwise continue to next phase
        if (bIsMovePossible == false) {
            currentPhase = GamePhase.GameOver;
        }
        else {
            currentPhase = GamePhase.WaitForPegSelection;
        }
    }

    void WaitForPegSelection() {
        instructionText.text = "Click peg to move";

        // Wait for user to click on valid piece to move
        if (selectedHole != null) {
            currentPhase = GamePhase.HighlightValidDestinations;
        }
    }

    void HighlightValidMoves() {
        // Turn off all current highlighting
        foreach (Hole[] subArray in holes) {
            foreach (Hole hole in subArray) {
                hole.SetHighlight(false);
            }
        }

        // Show icon above the selected piece
        selectedHole.SetPointingIcon(true);

        // Highlight valid spots peg can be moved
        foreach (Hole destination in selectedHole.GetFirstNeighborLookup().Keys) {
            destination.SetHighlight(true);
            destination.SetValidDestination(true); // TODO other way
        }

        currentPhase = GamePhase.WaitForDestinationSelection;
    }

    void WaitForDestinationSelection() {
        instructionText.text = "Click spot to move peg";

        if (destinationHole != null) {
            currentPhase = GamePhase.MovePiece;
        }
    }

    void MovePiece() {
        // Removing existing highlighting from previous phases
        foreach (Hole destination in selectedHole.GetFirstNeighborLookup().Keys) {
            destination.SetHighlight(false);
            destination.SetValidDestination(false); // TODO other way
        }
        selectedHole.SetPointingIcon(false);

        // Remove selected peg and middle peg from hole; put peg into destination
        selectedHole.SetPeg(false);
        destinationHole.SetPeg(true);
        Hole removedPeg = selectedHole.GetRemovedPeg(destinationHole);
        removedPeg.SetPeg(false);

        // Keep track of remaining pegs (player wins when pegs = 1)
        remainingPegs--;

        // Reset references for clicked holes
        selectedHole = null;
        destinationHole = null;

        currentPhase = GamePhase.CheckAllValidMoves;

        //TODO gradual move coroutine?
    }

    void GameOver() {
        // Player wins if only 1 peg remains
        if (remainingPegs == 1) {
            instructionText.text = "Game Over!\nYou Win!";
            instructionText.color = Color.blue;
        }
        else {
            instructionText.text = "Game Over!\nYou Lose!";
            instructionText.color = Color.red;
        }

        // Player needs to press the UI "Reset" button to restart game
    }

    /// <summary>
    /// Record when a hole on the board has been clicked, if it's the correct gamePhase and marked valid 
    /// by previous steps
    /// </summary>
    public void OnButtonClicked(Hole clicked) {

        if (currentPhase == GamePhase.WaitForPegSelection && clicked.IsValidSelection() || currentPhase == GamePhase.RemoveFirstPeg) {
            selectedHole = clicked;
        }
        else if (currentPhase == GamePhase.WaitForDestinationSelection && clicked.IsValidDestination()) {
            destinationHole = clicked;
        }
    }
}