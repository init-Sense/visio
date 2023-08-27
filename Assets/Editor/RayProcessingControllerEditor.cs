using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RayProcessingController))]
public class RayProcessingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RayProcessingController controller = (RayProcessingController)target;

        // Draw the default inspector without applying changes
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            // If changes were made in the inspector, update the object
            serializedObject.ApplyModifiedProperties();
        }

        // Custom drawing for other fields can be added here if needed
    }
}