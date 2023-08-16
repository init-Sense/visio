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
        
        // Add a title for transparency if there are any ChangeTransparencyActionHandler actions
        if (controller.rayReceivers.Count > 0)
        {
            foreach (RayProcessingController.RayReceiver receiver in controller.rayReceivers)
            {
                foreach (RayProcessingController.ActionableObject action in receiver.actionableObjects)
                {
                    if (action.actionBase is ChangeTransparencyAction)
                    {
                        EditorGUILayout.LabelField("Transparency Settings", EditorStyles.boldLabel);
                        break;
                    }
                }
            }
        }
        // Custom drawing for the transparencyIncrement
        for (int i = 0; i < controller.rayReceivers.Count; i++)
        {
            for (int j = 0; j < controller.rayReceivers[i].actionableObjects.Count; j++)
            {
                var actionObj = controller.rayReceivers[i].actionableObjects[j];
                if (actionObj.actionBase is ChangeTransparencyAction)
                {
                    // If the action is ChangeTransparencyActionHandler, show the transparencyIncrement field
                    actionObj.transparencyIncrement = EditorGUILayout.FloatField($"Element {i} | Action {j}", actionObj.transparencyIncrement);
                }
            }
        }
    }
}