#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    [CustomEditor(typeof(AdventureTrainerSpawnController))]
    public class AdventureTrainerSpawnControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var saveIdProp = serializedObject.FindProperty("SaveId");
            if (saveIdProp != null && GUILayout.Button("Generate Id"))
            {
                saveIdProp.stringValue = GUID.Generate().ToString();
            }

            var spawnController = serializedObject.targetObject as AdventureTrainerSpawnController;
            if (spawnController)
            {
                spawnController.RefreshEditorSprite();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
