using UnityEngine;
public class Cubie {
    public GameObject Piece;
    private readonly GameObject masterCube; //Highest Parent of each cube is the cube object itself.

    private int Index;

    private Vector3 Position;
    private Quaternion Rotation;

    private readonly float Size = 0.01866f;

    private string observe = "18";

    public Cubie(GameObject pPiece, int index, GameObject cubeObject) {
        this.Piece = pPiece;
        Position = pPiece.transform.localPosition;
        Rotation = Quaternion.identity;
        SetIndex(index);
        masterCube = cubeObject;
    }
    
    public void Turn(Turn t) {
        Quaternion temp = Rotation;

        if (t.face == "R") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.right) * Rotation;
        if (t.face == "L") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.left) * Rotation;
        if (t.face == "F") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.back) * Rotation;     //[SIC]!
        if (t.face == "B") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.forward) * Rotation;  //[SIC]!
        if (t.face == "U") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.up) * Rotation;
        if (t.face == "D") Rotation = Quaternion.AngleAxis(t.GetDegree(), Vector3.down) * Rotation;

        if (Piece.name == observe) {
            Debug.Log(temp.eulerAngles + " * " + Quaternion.AngleAxis(t.GetDegree(), Vector3.right).eulerAngles + " = " + (temp * Quaternion.AngleAxis(t.GetDegree(), Vector3.right)).eulerAngles);
            Debug.Log(Quaternion.AngleAxis(t.GetDegree(), Vector3.right).eulerAngles + " * " + temp.eulerAngles + " = " + (Quaternion.AngleAxis(t.GetDegree(), Vector3.right) * temp).eulerAngles);
            Debug.Log(Piece.name + " was at " + temp.eulerAngles + " and is now at " + Rotation.eulerAngles);
        }
    }

    public void CleanUp() {
        Piece.transform.SetParent(masterCube.transform);
        Piece.transform.localPosition = Position;
        Piece.transform.localRotation = Rotation;
        Piece.transform.localScale = new Vector3(1, 1, 1);
    }

    public int GetIndex() {
        return this.Index;
    }

    public void SetIndex(int newIndex) {
        int temp = this.Index;

        this.Index = newIndex;
        int currIdx = 0;
        for(int y = 1; y >= -1; y--) {
            for(int z = 1; z >= -1; z--) {
                for(int x = -1; x <= 1; x++) {
                    if (x == 0 && y == 0 && z == 0) continue;   //No middle piece
                    if(this.Index == currIdx) {
                        Position = new Vector3(x * Size, y * Size, z * Size);
                        //Debug.Log(Index + " at " + Position);
                        if(Piece.name == observe) {
                            Debug.Log(Piece.name + " was at " + temp + " and is now at " + newIndex + " which is at " + Position);
                        }
                        return;
                    } else {
                        currIdx++;
                    }
                }
            }
        }
    }
}
