#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteelSeries.GameSense;
using SteelSeries.GameSense.Config;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSenseInit))]
public class GameSenseInitEditor : Editor
{
    [SerializeField]
    private int m_value;

    [SerializeField]
    private string m_event;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var config = ((GameSenseInit)target).Config;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor event test", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Active game", GameSense.GameName);
        }

        int index = Array.FindIndex(config.Events, s => s.Name == m_event);
        EditorGUI.BeginChangeCheck();
        index = EditorGUILayout.Popup("Event", index, config.Events.Select(s => s.Name).ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            m_event = index >= 0 ? config.Events[index].Name : string.Empty;
        }

        var evnt = index >= 0 ? config.Events[index] : default;

        using (new EditorGUI.DisabledScope(index < 0))
        {
            EditorGUI.BeginChangeCheck();
            m_value = EditorGUILayout.IntSlider("Event data", m_value, evnt.MinValue, evnt.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                GameSense.SendEvent(evnt.Name, m_value);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Init"))
        {
            config.Initialize();
        }
        if (GUILayout.Button("Release"))
        {
            GameSense.Release();
        }
        if (GUILayout.Button("Reset"))
        {
            using (var client = new GameSenseClient())
            {
                client.RemoveGame(config.GameName);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
