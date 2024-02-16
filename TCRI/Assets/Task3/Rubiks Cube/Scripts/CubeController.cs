using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using System.Diagnostics;

public class CubeController : MonoBehaviour
{
    //Cube pieces in !correct! order:
    public GameObject RubiksCube;
    public GameObject[] Pieces;
    public GameObject UpCenter;
    public GameObject DownCenter;
    public GameObject LeftCenter;
    public GameObject RightCenter;
    public GameObject FrontCenter;
    public GameObject BackCenter;

    //Instructions Menu
    public GameObject InstructionsMenu;
    //public PassthroughManager passthroughManager;

    //Cube model
    private Cube cube;

    //User performed Turn Stack
    Stack<Turn> UserPerformedTurns = new Stack<Turn>();

    //Rotation animation parameters
    public float turnDuration = 0.2f;       //A value of 0.2 means a 90° turn takes 0.2 seconds
    private Cubie animatedCenter = null;    //The center/face that is influenced by the rotation animation to automatically perfom turns
    private Quaternion preStart;            //Used for GrabTurn to save the rotation before manual manipulation
    private Quaternion start;               //Start-rotation of the animation
    private Quaternion end;                 //End-rotation of the animation
    private float timeCount = 0.0f;         //Total time the rotation lasted

    //Grabbing
    public int snappingAngle = 10;         //The angle at which a face snaps to align with the cube during grab-turning
    private GameObject pseudoCenter;        //This pseudoCenter gameObject is rotated with handtracking. The grabbingCenter face follows.
    private Cubie grabbingCenter;           //This grabbingCenter face follows the pseudoCenter gameObject

    //Cube states:
    bool executing = false;                 //If the cube currently performs a turn/animation
    bool grabbing = false;                  //If the user is currently grabbing a face
    bool hovering = false;                  //If the user is currently hovering over a face prior to selecting it

    //Instructions
    private InstructionController instructionController;

    //public TextMeshPro littleDebug;
    private AudioSource fanfare;

    public string StartScramble;

    // Start is called before the first frame update
    void Start() {
        fanfare = GetComponent<AudioSource>();
        instructionController = GetComponent<InstructionController>();
        instructionController.updateInstruction(global::Turn.EmptyTurn);

        cube = new Cube(Pieces, RubiksCube, true);    //here gameObject is the 3x3Cube GameObject; its needed in the Cubies for CleanUp measures.
        Scramble(StartScramble);

        PassthroughManager.onShorttermTransition += ShorttermTransitionOccured;
    }

    void Update()
    {
        //Do rotation animation:
        if(executing) {
            animatedCenter.Piece.transform.localRotation = Quaternion.Lerp(start, end, timeCount / turnDuration * (90 / Quaternion.Angle(start, end)));
            timeCount += Time.deltaTime;
            //End of rotation:
            if(1 - Mathf.Abs(Quaternion.Dot(animatedCenter.Piece.transform.localRotation, end)) < 0.0001) {
                animatedCenter = null;
                executing = false;
                timeCount = 0;

                //Put next turn into debug Window
                //littleDebug.text = "Next Turn: " + cube.currentSolution.PeekNextTurn().ToString();
                //Transfer next turn to InstructionController
                instructionController.updateInstruction(cube.currentSolution.PeekNextTurn());

                //CleanUp after animation:
                cube.CleanUp();
            }
        }

        if(!executing && grabbing) {
            //Execute this while user pseudo-grabs and turns a face

            //Calculate the angle the user has already dragged the pseudo-face:
            Quaternion currentRoc = pseudoCenter.transform.localRotation;

            //calc angle made. The turn axis is determined by the localPosition of the center piece
            Vector3 axis = pseudoCenter.transform.localPosition.normalized;
            Vector3 from = Quaternion.identity * Vector3.up;   //pseudoCenter always has NO rotation at the beginning
            Vector3 to = currentRoc * Vector3.up;        //What does the current rotation do to reference Vector?

            if (from == Vector3.up && to == Vector3.up) {                    //Parity for up and down layers
                from = Quaternion.identity * Vector3.forward;   //What does preStart Quaternion do to reference Vector?
                to = currentRoc * Vector3.forward;        //What does start Quaternion do to reference Vector?
            }

            float angle = Vector3.SignedAngle(from, to, axis);

            //snap to nearest 90 deg:
            float snapAngle = Mathf.RoundToInt(angle / 90) * 90;

            //littleDebug.text = "Angle = " + angle + ";  snapAngle = " + snapAngle + "; Snap = " + Mathf.Abs(angle - snapAngle);

            if (Mathf.Abs(angle-snapAngle) < snappingAngle) {
                //user is widthin snapping range
                //teleport center piece and childs to neares 90 degrees
                //Calc snap rotation:
                Quaternion snapRotation = Quaternion.AngleAxis(snapAngle, axis);
                grabbingCenter.Piece.transform.localRotation = snapRotation;
            } else {
                //user is not within snapping range
                //teleport center piece and childs to pseudoCenter
                grabbingCenter.Piece.transform.localRotation = pseudoCenter.transform.localRotation;
            }
        }
        
        if(!executing && !grabbing && !hovering) {
            //The user does nothing with the cube
            cube.CleanUp();
        }   
    }

    private void ShorttermTransitionOccured(PassthroughManager.Environments from, PassthroughManager.Environments to) {
        //Altering the instructions during the environments
        bool menuVisible = false;
        if (to <= PassthroughManager.Environments.FAR) {
            //Set for manual manipulation
            turnDuration = 0.2f;
            menuVisible = false;
            UpCenter.SetActive(true);
            DownCenter.SetActive(true);
            LeftCenter.SetActive(true);
            RightCenter.SetActive(true);
            FrontCenter.SetActive(true);
            BackCenter.SetActive(true);
        } else if (to == PassthroughManager.Environments.AR) {
            //We are in between AV and AR: Set automated manipulation
            turnDuration = 2.0f;
            menuVisible = true;
            UpCenter.SetActive(false);
            DownCenter.SetActive(false);
            LeftCenter.SetActive(false);
            RightCenter.SetActive(false);
            FrontCenter.SetActive(false);
            BackCenter.SetActive(false);
        } else {
            //R: deactivate everything
            turnDuration = 0.1f;
            menuVisible = false;
            UpCenter.SetActive(false);
            DownCenter.SetActive(false);
            LeftCenter.SetActive(false);
            RightCenter.SetActive(false);
            FrontCenter.SetActive(false);
            BackCenter.SetActive(false);
        }
        if(InstructionsMenu != null) {
            if(menuVisible) {
                //InstructionsMenu.transform.localScale = new Vector3(0.056f, 0.031f, 1);
                InstructionsMenu.SetActive(true);
            } else {
                //InstructionsMenu.transform.localScale = Vector3.zero;
                InstructionsMenu.SetActive(false);
            }
        }
    }

    public void AnimateTurn(Turn t) {
        //Determine center and direction from Turn object
        animatedCenter = cube.GetCenter(t.face);

        //Calc rotations. Normalized LocalPosition of selected Center determines turn axis
        start = animatedCenter.Piece.transform.localRotation;
        end = start * Quaternion.AngleAxis(90 * (t.steps == 3 ? -1 : t.steps), animatedCenter.Piece.transform.localPosition.normalized);

        //Perform turn on actual cube:
        executing = true;

        //Perfom turn on cube model:
        cube.Turn(t);

        //Is the cube solved now?
        if (cube.IsSolved()) {
            fanfare.Play();
            Logger.Log("CubeController", "Rubik's Cube solved after AnimateTurn.");
        }

        //Set pieces as child and clean up the transforms
        foreach (Cubie c in cube.FacePieces(t.face.ToString())) {
            c.Piece.transform.SetParent(animatedCenter.Piece.transform);
        }
    }

    public void GrabTurnStart(string center_name) {
        grabbingCenter = cube.GetCenter(center_name);
        grabbing = true;

        switch (center_name) {
            case "R": pseudoCenter = RightCenter; break;
            case "L": pseudoCenter = LeftCenter; break;
            case "F": pseudoCenter = FrontCenter; break;
            case "B": pseudoCenter = BackCenter; break;
            case "U": pseudoCenter = UpCenter; break;
            case "D": pseudoCenter = DownCenter; break;
        }

        //Safe localRotation to later calculate how many degrees the user has turned the pseudoCenter
        preStart = pseudoCenter.transform.localRotation;

        //Set pieces as child and clean up the transforms
        foreach (Cubie c in cube.FacePieces(center_name)) {
            c.Piece.transform.SetParent(grabbingCenter.Piece.transform);
        }

        Logger.Log("CubeController", "GrabTurnStart on " + center_name);
    }

    public void GrabTurnEnd(string center_name) {
        grabbing = false;

        //Where did the user drop the pseudoCenter? grabbingCenter is the Piece that currently follows the pseudoCenter
        start = pseudoCenter.transform.localRotation;

        //calc angle made. The turn axis is determined by the localPosition of the pseudoCenter
        Vector3 axis = pseudoCenter.transform.localPosition.normalized;
        Vector3 from = preStart * Vector3.up;   //What does preStart Quaternion do to reference Vector?
        Vector3 to = start * Vector3.up;        //What does start Quaternion do to reference Vector?

        if (from == Vector3.up && to == Vector3.up) {                    //Parity for up and down layers
            from = preStart * Vector3.forward;   //What does preStart Quaternion do to reference Vector?
            to = start * Vector3.forward;        //What does start Quaternion do to reference Vector?
        }

        float angle = Vector3.SignedAngle(from, to, axis);

        //snap to nearest 90 deg:
        float snapAngle = Mathf.RoundToInt(angle / 90) * 90;

        //littleDebug.text = "Angle = " + angle + ";  snapAngle = " + snapAngle + "; center = " + center_name;

        //Do turn to cube model:
        Turn t;
        switch (snapAngle) {
            case 90: t = new Turn(center_name + ""); break;
            case -90: t = new Turn(center_name + "'"); break;
            case 180: t = new Turn(center_name + "2"); break;
            case -180: t = new Turn(center_name + "2"); break;
            default: t = global::Turn.EmptyTurn; break; //If angle is 0 then dont do any rotation
        }
        cube.Turn(t);

        //Push to stack for Undo option:
        if(t != global::Turn.EmptyTurn) {
            UserPerformedTurns.Push(t);
        }

        //Is the cube solved now?
        if (cube.IsSolved()) {
            fanfare.Play();
            Logger.Log("CubeController", "Rubik's Cube solved after GrabTurnEnd.");
        }

        //Calc end rotation:
        end = preStart * Quaternion.AngleAxis(snapAngle, axis);

        //Start Animation of the grabbingCenter to its final position
        animatedCenter = grabbingCenter;
        executing = true;

        //Reset
        grabbingCenter = null;
        pseudoCenter.transform.localRotation = Quaternion.identity;
        pseudoCenter = null;

        //Debug.Log("GrabTurnEnd. Center Name = " + center_name + "; axis = " + axis + "; from = " + from + "; to = " + to + "; angle = " + angle + "; snapAngle = " + snapAngle + "; turn = " + t.ToString());
        Logger.Log("CubeController", "GrabTurnEnd on " + center_name + "; angle = " + angle + "; Turn = " + t.ToString());
    }

    public void PalmGrabStart() {
    }

    public void PalmGrabEnd() {

    }

    //Method used to increase size of the face to indicate its selection
    public void HoverStart(string center_name) {
        if(!hovering) {
            hovering = true;
            Cubie center = cube.GetCenter(center_name);
            //Set pieces as child and clean up the transforms
            foreach (Cubie c in cube.FacePieces(center_name)) {
                c.Piece.transform.SetParent(center.Piece.transform);
            }
            Vector3 scaleChange = new Vector3(0.1f, 0.1f, 0.1f);
            center.Piece.transform.localScale += scaleChange;
        }
    }

    public void HoverEnd() {
        hovering = false;
        cube.CleanUp();
    }

    IEnumerator DoAlgorithm(Algorithm alg) {
        //After the algorithm is applied, the user should not be able to just undo it
        UserPerformedTurns.Clear();

        Logger.Log("CubeController", "Starting Algorithm on " + gameObject.name);
        cube.SetUpdateSolution(false);  //That way, the solution is not recalculated each time a turn is applied
        foreach (Turn t in alg) {
            AnimateTurn(t);
            yield return new WaitUntil(() => animatedCenter == null);
        }
        cube.SetUpdateSolution(true);   //Reactivate solution updateting
        instructionController.updateInstruction(cube.currentSolution.PeekNextTurn());
        Logger.Log("CubeController", "Finished Algorithm");

        yield break;
    }

    public void DoNextTurn() {
        //Get the next turn and apply it. Calculate next turn.
        if(!cube.currentSolution.IsDone() && !executing) {
            //There is a next turn
            Logger.Log("CubeController", "Doing NextTurn");
            Turn t = cube.currentSolution.PeekNextTurn();
            AnimateTurn(t);
            UserPerformedTurns.Push(t);
        }
    }

    public void UndoPreviousTurn() {
        //Pop previous turn and apply the inverse
        if(UserPerformedTurns.Count != 0 && !executing) {
            //Stack is not empty
            Turn t = UserPerformedTurns.Pop();
            AnimateTurn(Turn.Inverse(t));
            Logger.Log("CubeController", "Undoing PreviousTurn");
        }
    }

    public void Scramble(string scramble) {
        Algorithm scr = new Algorithm(scramble);
        StartCoroutine(DoAlgorithm(scr));
        Logger.Log("CubeController", "Scrambling Cube");
    }

    public void ScrambleToState(string scramble) {
        Algorithm algo = Solver.Solve(cube);
        algo.AppendAlgorithm(new Algorithm(scramble));
        StartCoroutine(DoAlgorithm(algo));
        Logger.Log("CubeController", "Scrambling Cube To State");
    }
}
