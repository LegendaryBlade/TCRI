using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;
using Kociemba;

public class Solver {

    /// <summary>
    /// Solves the cube using Kociemba´s Two Phase algorithm.
    /// </summary>
    /// <param name="cube">The cube to solve</param>
    /// <returns></returns>
    public static Algorithm Solve(Cube cube) {
        string info;
        string solution = Search.solution(cube.GetCDS(), out info);
        return new Algorithm(solution);
    }

    public static Algorithm Scramble() {
        string info;
        Algorithm solve = new Algorithm(Search.solution(Tools.randomCube(), out info));
        return solve; //Basically this is wrong but its cooler. Otherwise the algorithm just reverses the scramble...
        //return Algorithm.Inverse(solve);
    }

}
