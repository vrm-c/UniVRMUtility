using System.IO;
using UnityEngine;

namespace UniVRMUtility
{
    public class DialogTest : MonoBehaviour
    {
        void OnGUI()
        {
            if(GUILayout.Button("com open"))
            {
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = ComDialog.Open(title: "open", "*.txt", "*.md");
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
                Debug.Log(path);
            }

            if(GUILayout.Button("com save"))
            {
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = ComDialog.Save(title: "save", "sample.txt");
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
                Debug.Log(path);
            }
        }
    }
}
