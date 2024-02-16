using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Cube {

    //3x3 CDS Scheme by Kociemba: (white top/green front)
    //                |***************|
    //                |*W00**W01**W02*|
    //                |***************|
    //                |*W03**W04**W05*|
    //                |***************|
    //                |*W06**W07**W08*|
    // ***************|***************|***************|***************|
    // *O36**O37**O38*|*G18**G19**G20*|*R09**R10**R11*|*B45**B46**B47*|
    // ***************|***************|***************|***************|
    // *O39**O40**O41*|*G21**G22**G23*|*R12**R13**R14*|*B48**B49**B50*|
    // ***************|***************|***************|***************|
    // *O42**O43**O44*|*G24**G25**G26*|*R15**R16**R17*|*B51**B52**B53*|
    // ***************|***************|***************|***************|
    //                |*Y27**Y28**Y29*|
    //                |***************|
    //                |*Y30**Y31**Y32*|
    //                |***************|
    //                |*Y33**Y34**Y35*|
    //                |***************|

    //Tracks position of stickers (CubeDefinitionString)
    private string CDS = "UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB"; //URFDLB                 //CDS[x] = y: at index x is currectly color y

    //3x3 CPL Scheme:
    //00 # 01 # 02  Top Layer
    //03 # 04 # 05
    //06 # 07 # 08

    //09 # 10 # 11  Middle Layer
    //12        13
    //14 # 15 # 16

    //17 # 18 # 19  Buttom Layer
    //20 # 21 # 22
    //23 # 24 # 25

    //List of all Cubies inside this cube
    private Cubie[] Cubies = new Cubie[26];     //Each Cubie holds an index of the position it is currently in. Gets updated after each turn. Though the order of 'Cubies' won't change

    public readonly Cubie RightCenter;
    public readonly Cubie LeftCenter;
    public readonly Cubie UpCenter;
    public readonly Cubie DownCenter;
    public readonly Cubie FrontCenter;
    public readonly Cubie BackCenter;

    public Algorithm currentSolution;

    private bool updateSolution = true;

    public Cube(GameObject[] Pieces, GameObject cubeObject, bool scrambled = false) {
        //Build the Cubie objects from the GameObjects
        for (int i = 0; i < 26; i++) {
            Cubies[i] = new Cubie(Pieces[i], i, cubeObject);
        }
        //Reference the Centers specifically
        RightCenter = Cubies[13];
        LeftCenter = Cubies[12];
        UpCenter = Cubies[4];
        DownCenter = Cubies[21];
        FrontCenter = Cubies[15];
        BackCenter = Cubies[10];

        currentSolution = new Algorithm("");
    }

    public void CleanUp() {
        foreach(Cubie c in Cubies) {
            c.CleanUp();
        }
    }

    /// <summary>
    /// Applies the Turn t to the cube. This involves manipulating the CDS and the CPL of the cube according to the move.
    /// </summary>
    /// <param name="turn">The Turn to apply.</param>
    public void Turn(Turn turn) {
        //Manipulating CDS
        int[] rotation = new int[54];
        if (turn.face == "R") rotation = new int[54] { 0,1,20,3,4,23,6,7,26,15,12,9,16,13,10,17,14,11,18,19,29,21,22,32,24,25,35,27,28,51,30,31,48,33,34,45,36,37,38,39,40,41,42,43,44,8,46,47,5,49,50,2,52,53 };
        if (turn.face == "L") rotation = new int[54] { 53,1,2,50,4,5,47,7,8,9,10,11,12,13,14,15,16,17,0,19,20,3,22,23,6,25,26,18,28,29,21,31,32,24,34,35,42,39,36,43,40,37,44,41,38,45,46,33,48,49,30,51,52,27 };
        if (turn.face == "F") rotation = new int[54] { 0,1,2,3,4,5,44,41,38,6,10,11,7,13,14,8,16,17,24,21,18,25,22,19,26,23,20,15,12,9,30,31,32,33,34,35,36,37,27,39,40,28,42,43,29,45,46,47,48,49,50,51,52,53 };
        if (turn.face == "B") rotation = new int[54] { 11,14,17,3,4,5,6,7,8,9,10,35,12,13,34,15,16,33,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,36,39,42,2,37,38,1,40,41,0,43,44,51,48,45,52,49,46,53,50,47 };
        if (turn.face == "U") rotation = new int[54] { 6,3,0,7,4,1,8,5,2,45,46,47,12,13,14,15,16,17,9,10,11,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,18,19,20,39,40,41,42,43,44,36,37,38,48,49,50,51,52,53 };
        if (turn.face == "D") rotation = new int[54] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,24,25,26,18,19,20,21,22,23,42,43,44,33,30,27,34,31,28,35,32,29,36,37,38,39,40,41,51,52,53,45,46,47,48,49,50,15,16,17 };

        for (int steps = 0; steps < turn.steps; steps++) {
            StringBuilder strBuilder = new StringBuilder(this.CDS);
            for (int i = 0; i < 54; i++) {
                strBuilder[i] = this.CDS[rotation[i]];
            }
            this.CDS = strBuilder.ToString();
        }

        //Manipulating CPL
        int[] rotationCPL = new int[26];                //rotationCPL[x] = y: x is now at y
        if (turn.face == "R") rotationCPL = new int[26] { 0,1,19,3,4,11,6,7,2,9,10,22,12,13,14,15,5,17,18,25,20,21,16,23,24,8 };
        if (turn.face == "L") rotationCPL = new int[26] { 6,1,2,14,4,5,23,7,8,3,10,11,12,13,20,15,16,0,18,19,9,21,22,17,24,25 };
        if (turn.face == "F") rotationCPL = new int[26] { 0,1,2,3,4,5,8,16,25,9,10,11,12,13,7,15,24,17,18,19,20,21,22,6,14,23 };
        if (turn.face == "B") rotationCPL = new int[26] { 17,9,0,3,4,5,6,7,8,18,10,1,12,13,14,15,16,19,11,2,20,21,22,23,24,25 };
        if (turn.face == "U") rotationCPL = new int[26] { 2,5,8,1,4,7,0,3,6,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25 };
        if (turn.face == "D") rotationCPL = new int[26] { 0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,23,20,17,24,21,18,25,22,19 };

        foreach(Cubie c in Cubies) {
            if(rotationCPL[c.GetIndex()] != c.GetIndex()) {         //Piece is affected
                for (int steps = 0; steps < turn.steps; steps++) {  //apply permutation to piece as many times as turn says
                    c.SetIndex(rotationCPL[c.GetIndex()]);          //Set new Index
                }
                c.Turn(turn);                                       //Apply orientation to piece
            }
        }

        //Manipulating Solution
        if(updateSolution) {
            if (currentSolution.PeekNextTurn() == turn) {
                //The applied turn is the next towards the solved state
                currentSolution.PopNextTurn();
            } else {
                //The applied turn was wrong.
                currentSolution = Solver.Solve(this);
            }
        }
    }

    /// <summary>
    /// Applies multiple Turns on this cube. Turns are given as an Algorithm object.
    /// </summary>
    /// <param name="alg">The Algorithm to execute.</param>
    public void ApplyAlgorithm(Algorithm alg) {
        foreach(Turn t in alg) {
            Turn(t);
        }
    }

    /// <summary>
    /// Checks if the cube is solved. The cube is considered solved when all sides are of one color only in the correct order
    /// </summary>
    /// <returns>True if solved. False otherwise.</returns>
    public bool IsSolved() {
        return this.CDS == "UUUUUUUUURRRRRRRRRFFFFFFFFFDDDDDDDDDLLLLLLLLLBBBBBBBBB";
    }

    /// <summary>
    /// Returns all the pieces that are currently on the side specified by the face parameter.
    /// The method returns an array of indices of the pieces that would move if the specified side is turned.
    /// </summary>
    /// <param name="face">The face whose pieces are in question.</param>
    /// <returns>An array containing the indices of the pieces.</returns>
    public List<Cubie> FacePieces(string face) {
        int[] copyIndices = new int[9];
        if (face == "R") copyIndices = new int[9] { 8, 5, 2, 16, 13, 11, 25, 22, 19 };
        if (face == "L") copyIndices = new int[9] { 0, 3, 6, 9, 12, 14, 17, 20, 23 };
        if (face == "F") copyIndices = new int[9] { 6, 7, 8, 14, 15, 16, 23, 24, 25 };
        if (face == "B") copyIndices = new int[9] { 0, 1, 2, 9, 10, 11, 17, 18, 19 };
        if (face == "U") copyIndices = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        if (face == "D") copyIndices = new int[9] { 17, 18, 19, 20, 21, 22, 23, 24, 25 };

        List<Cubie> res = new List<Cubie>();

        for(int i = 0; i < 9; i++) {
            foreach (Cubie c in Cubies) {
                if (c.GetIndex() == copyIndices[i]) {
                    res.Add(c);
                }
            }
        }
        return res;
    }

    public Cubie GetCenter(string name) {
        switch(name) {
            case "R": return RightCenter;
            case "L": return LeftCenter;
            case "F": return FrontCenter;
            case "B": return BackCenter;
            case "U": return UpCenter;
            case "D": return DownCenter;
            default: return null;    //Might cause flaws
        }
    }

    /// <summary>
    /// Returns the CDS of the cube. If the cube is a 2x2 cube the CDS of the underlying 3x3 is returned.
    /// </summary>
    /// <returns>The CDS of this cube.</returns>
    public string GetCDS() {
        return this.CDS;
    }

    public void SetUpdateSolution(bool state) {
        if (updateSolution == false && state == true) {
            //e.g. reactivate solution updateting:
            currentSolution = Solver.Solve(this);
        }

        if (updateSolution == true && state == false) {
            //e.g. disabling solution updateting:
            currentSolution = new Algorithm("");
        }
        updateSolution = state;
    }
}
