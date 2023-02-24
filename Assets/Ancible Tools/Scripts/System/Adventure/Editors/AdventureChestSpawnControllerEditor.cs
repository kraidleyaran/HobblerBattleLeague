using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using UnityEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Adventure.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AdventureChestSpawnController))]
    public class AdventureChestSpawnControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            
            if (GUILayout.Button("Generate Id"))
            {
                if (serializedObject.targetObjects.Length > 1)
                {
                    foreach (var obj in serializedObject.targetObjects)
                    {
                        var controller = obj as AdventureChestSpawnController;
                        if (controller)
                        {
                            controller.SaveId = GUID.Generate().ToString();
                        }
                    }
                }
                else
                {
                    var saveIdProp = serializedObject.FindProperty("SaveId");
                    saveIdProp.stringValue = GUID.Generate().ToString();
                }
                
            }

            var spawnController = serializedObject.targetObject as AdventureChestSpawnController;
            if (spawnController)
            {
                spawnController.RefreshEditorSprite();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}