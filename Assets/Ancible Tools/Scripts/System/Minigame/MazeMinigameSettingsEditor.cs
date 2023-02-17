#if UNITY_EDITOR
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Minigame
{
    [CustomEditor(typeof(MazeMinigameSettings))]
    public class MazeMinigameSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var settings = serializedObject.targetObject as MazeMinigameSettings;
            if (settings)
            {
                
            }
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif
