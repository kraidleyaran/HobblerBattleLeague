#if UNITY_EDITOR
using Assets.Resources.Ancible_Tools.Scripts.System.Adventure;
using UnityEditor;
using UnityEngine;

namespace Assets.Ancible_Tools.Scripts.System.Adventure.Editors
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
