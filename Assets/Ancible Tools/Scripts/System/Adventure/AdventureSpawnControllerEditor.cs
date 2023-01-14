using UnityEditor;

#if UNITY_EDITOR
namespace Assets.Resources.Ancible_Tools.Scripts.System.Adventure
{
    [CustomEditor(typeof(AdventureUnitSpawnController))]
    public class AdventureUnitSpawnControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var unitSpawnController = serializedObject.targetObject as AdventureUnitSpawnController;
            if (unitSpawnController)
            {
                unitSpawnController.RefreshEditorSprite();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
