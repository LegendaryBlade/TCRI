using System.Collections;
using System.Collections.Generic;

public class Algorithm : IEnumerable<Turn> {

    public List<Turn> allTurns;

    public Algorithm() {
        allTurns = new List<Turn>();
    }
    
    public Algorithm(string alg) {
        alg = alg.Trim();
        allTurns = new List<Turn>();
        //TODO
        if (alg != "") {
            alg = alg.Replace("(", "").Replace(")", "");
            string[] turns = alg.Split(" ");

            foreach (string turn in turns) {
                Turn t = new Turn(turn);
                allTurns.Add(t);
            }
        }
    }

    public static Algorithm Inverse(Algorithm alg) {
        Algorithm res = new Algorithm();
        foreach(Turn t in alg) {
            res.allTurns.Insert(0, Turn.Inverse(t));
        }
        return res;
    }

    /// <summary>
    /// Returns the next Turn and remove the next turn from the Algorithm.
    /// </summary>
    /// <returns></returns>
    public Turn PopNextTurn() {
        if (allTurns.Count != 0) {
            Turn t = allTurns[0];
            allTurns.RemoveAt(0);
            return t;
        } else {
            return Turn.EmptyTurn;
        }
    }

    /// <summary>
    /// Returns the next Turn from the Algorithm but NOT removing it.
    /// </summary>
    /// <returns></returns>
    public Turn PeekNextTurn() {
        if(allTurns.Count != 0) {
            return allTurns[0];
        } else {
            return Turn.EmptyTurn;
        }

    }

    public bool IsDone() {
        return allTurns.Count == 0;
    }

    public void AppendTurn(Turn t) {
        allTurns.Add(t);
    }

    public void PrependTurn(Turn t) {
        allTurns.Insert(0, t);
    }

    public void AppendAlgorithm(Algorithm alg) {
        allTurns.AddRange(alg);
    }

    public override string ToString() {
        Turn[] temp = new Turn[allTurns.Count];
        allTurns.CopyTo(temp, 0);

        string res = "";

        foreach(Turn t in temp) {
            res += t.ToString() + " ";
        }

        return res;
    }

    public IEnumerator<Turn> GetEnumerator() {
        return ((IEnumerable<Turn>)allTurns).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable)allTurns).GetEnumerator();
    }
}
