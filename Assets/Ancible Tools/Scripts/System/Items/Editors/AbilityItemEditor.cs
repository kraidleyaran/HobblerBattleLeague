#if UNITY_EDITOR
using UnityEditor;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items.Editors
{
    [CustomEditor(typeof(AbilityItem))]
    public class AbilityItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var abilityItem = serializedObject.targetObject as AbilityItem;
            if (abilityItem && abilityItem.Ability)
            {
                if (abilityItem.Icon != abilityItem.Ability.Icon)
                {
                    abilityItem.SetIcon(abilityItem.Ability.Icon);
                }
                abilityItem.DisplayName = abilityItem.Ability.DisplayName;
            }
            serializedObject.ApplyModifiedProperties();
            DrawDefaultInspector();
        }
    }
}


#endif