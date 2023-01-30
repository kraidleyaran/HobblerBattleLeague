using UnityEditor;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Editor
{
    [CustomEditor(typeof(UnitSpawnController), true)]
    public class UnitSpawnControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var unitSpawnController = serializedObject.targetObject as UnitSpawnController;
            if (unitSpawnController)
            {
                unitSpawnController.RefreshSprite();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}