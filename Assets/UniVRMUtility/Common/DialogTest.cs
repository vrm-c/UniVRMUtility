using System.IO;
using UnityEngine;

namespace UniVRMUtility
{
    public class DialogTest : MonoBehaviour
    {
        void OnGUI()
        {
            if (GUILayout.Button("open"))
            {
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = FileDialogForWindows.FileDialog("open");
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
            }

            if (GUILayout.Button("save"))
            {
                // danger. this cause crash
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = FileDialogForWindows.SaveDialog("save", "save.txt");
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
            }

            if(GUILayout.Button("com open"))
            {
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = ComDialog.Open();
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
                Debug.Log(path);
            }

            if(GUILayout.Button("com save"))
            {
                Debug.Log($"before: {Directory.GetCurrentDirectory()}");
                var path = ComDialog.Save();
                Debug.Log($"after: {Directory.GetCurrentDirectory()}");
                Debug.Log(path);
            }
        }
    }
}
