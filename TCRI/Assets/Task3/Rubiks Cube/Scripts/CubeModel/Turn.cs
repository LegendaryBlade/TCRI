using System;
using System.Text.RegularExpressions;
public class Turn {

    public static Turn EmptyTurn = new Turn(" ", 0);

    //The face to turn:
    public string face;

    //The amount of 90 deg turns to do. 3*90 = 270 = -90 mod 360;
    public int steps; //1 - cw; 2 - double; 3 - ccw

    /// <summary>
    /// Creates a Turn from a string representation.
    /// </summary>
    /// <param name="turn">The string that describes the turn. E.g. R' or F2</param>
    public Turn(string turn) {
        turn = turn.Trim();
        Regex rx = new Regex(@"[RLUDFB]([2']|(?= )|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if(rx.IsMatch(turn)) {
            if(turn.Length == 1) {
                this.face = turn[0].ToString();
                this.steps = 1;
            } else {
                this.face = turn[0].ToString();
                switch(turn[1]) {
                    case '\'':
                        this.steps = 3;
                        break;
                    case '2':
                        this.steps = 2;
                        break;
                    case ' ':
                        this.steps = 1;
                        break;
                    default:
                        this.steps = 1;
                        break;
                }
            }
        }
    }

    public Turn(string face, int steps) {
        this.face = face;
        this.steps = steps;
    }

    /// <summary>
    /// Returns the inverse Turn of this Turn.
    /// </summary>
    /// <param name="t">The Turn whose inverse should be returned.</param>
    /// <returns>A new Turn acting as an invers to the given Turn.</returns>
    public static Turn Inverse(Turn t) {
        Turn newT = new Turn(t.face, t.steps == 2 ? 2 : (t.steps + 2) % 4);
        return newT;
    }

    public int GetDegree() {
        switch(this.steps) {
            case 1: return 90;
            case 2: return 180;
            case 3: return -90;
            default: return 0;
        }
    }

    public static bool operator== (Turn t1, Turn t2) {
        return t1.face == t2.face && t1.steps == t2.steps;
    }

    public static bool operator !=(Turn t1, Turn t2) {
        return !(t1.face == t2.face && t1.steps == t2.steps);
    }

    public override bool Equals(object obj) {
        if (obj == null) return false;
        return this == obj as Turn;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override string ToString() {
        string result = this.face.ToString();
        if(this.steps == 1) {
            //no space
        } else if(this.steps == 3) {
            result += "'";
        } else if(this.steps == 2) {
            result += "2";
        }
        return result;
    }
}
