using System.Collections;
using UnityEngine;

namespace MazeGeneration_vivi
{
    public class ScreenLogger : MonoBehaviour
    {
        uint qsize = 1;  // number of messages to keep
        Queue myLogQueue = new Queue();
        void OnEnable() {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable() {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type) {
            myLogQueue.Enqueue(logString);
            if (type == LogType.Exception)
                myLogQueue.Enqueue(stackTrace);
            while (myLogQueue.Count > qsize)
                myLogQueue.Dequeue();
        }

        private void OnGUI() {
            GUI.skin.label.fontSize = 15;
            GUILayout.BeginArea(new Rect(Screen.width/2-200, 0, 400, Screen.height));
            GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
            GUILayout.EndArea();
        }
    }
}