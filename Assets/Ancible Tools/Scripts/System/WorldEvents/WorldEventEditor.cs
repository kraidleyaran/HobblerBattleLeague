#if UNITY_EDITOR
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldEvents
{
    [CustomEditor(typeof(WorldEvent))]
    public class WorldEventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            var saveProp = serializedObject.FindProperty("Save");
            var saveIdProp = serializedObject.FindProperty("SaveId");
            if (saveProp != null && saveProp.boolValue)
            {
                if (saveIdProp != null)
                {
                    if (string.IsNullOrEmpty(saveIdProp.stringValue))
                    {
                        saveIdProp.stringValue = GUID.Generate().ToString();
                    }
                    EditorGUILayout.PropertyField(saveIdProp);
                }
            }
            else if (saveIdProp != null && !string.IsNullOrEmpty(saveIdProp.stringValue))
            {
                saveIdProp.stringValue = string.Empty;
            }

            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
