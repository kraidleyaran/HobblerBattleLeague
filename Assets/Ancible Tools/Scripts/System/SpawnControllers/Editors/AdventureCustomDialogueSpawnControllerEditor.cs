#if UNITY_EDITOR
using UnityEditor;


namespace Assets.Resources.Ancible_Tools.Scripts.System.SpawnControllers.Editors
{
    [CustomEditor(typeof(AdventureCustomDialogueSpawnController))]
    public class AdventureCustomDialogueSpawnControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var spawnController = serializedObject.targetObject as AdventureCustomDialogueSpawnController;
            if (spawnController)
            {
                spawnController.RefreshEditorSprite();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif