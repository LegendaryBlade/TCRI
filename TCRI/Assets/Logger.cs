using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Logger {

    private static bool newSessionStarted = false;
    public static void Log(string sender, string msg) {

        string path = Path.Combine(Application.persistentDataPath, "LogData.txt");

        //First message that gets logged will be started after some space to quickly detect new sessions.
        if (!newSessionStarted) {
            string newSessionString = "\n\n##########";
            using (StreamWriter sr = new StreamWriter(path, true)) {
                sr.WriteLine(newSessionString);
            }
            newSessionStarted = true;
        }


        DateTime localDate = DateTime.Now;
        string to_write = "[" + sender + "][" + DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss.fff") + "]: " + msg;

        using (StreamWriter sr = new StreamWriter(path, true)) {
            sr.WriteLine(to_write);
        }
    }
}
