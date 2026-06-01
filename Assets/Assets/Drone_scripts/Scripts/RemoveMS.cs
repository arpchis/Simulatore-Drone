#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : Editor
{
    [MenuItem("Tools/Remove Missing Scripts")]
    static void RemoveMissingScriptsFromSelected()
    {
        GameObject[] objects = Selection.gameObjects;
        int count = 0;
        foreach (GameObject go in objects)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            count++;
            // Applica anche a tutti i figli
            foreach (Transform child in go.GetComponentsInChildren<Transform>())
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            }
        }
        Debug.Log($"Rimossi Missing Scripts da {count} oggetti");
    }
}
#endif
