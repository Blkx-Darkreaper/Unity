using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameEvent))]
public class GameEventEditor : Editor
{
    public GameEvent gameEvent;
    protected bool isListExpanded = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        this.gameEvent = target as GameEvent;

        if (GUILayout.Button("Raise"))
        {
            gameEvent.Raise();
        }

        this.isListExpanded = EditorGUILayout.Foldout(isListExpanded, "Listeners:");
        EditorGUI.indentLevel++;

        EditorGUILayout.BeginHorizontal();
        if (gameEvent.allListeners.Count == 0)
        {
            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
        }
        else
        {
            foreach (GameEventListener listener in gameEvent.allListeners)
            {
                string listenerName = listener.gameObject.name;
                string responseName = listener.response.ToString();

                EditorGUILayout.LabelField(listenerName);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(responseName);
                
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
    }
}