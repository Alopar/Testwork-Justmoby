#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TimelineAnimationStateMachine))]
public class TimelineAnimationStateMachineDebug : MonoBehaviour
{
    public TimelineAnimationStateMachine TimelineAnimations;
    private void OnValidate()
    {
        TimelineAnimations = GetComponent<TimelineAnimationStateMachine>();
    }
}

[CustomEditor(typeof(TimelineAnimationStateMachineDebug))]
class CustomTimelineAnimationStateMachineDebug : Editor {
    public override void OnInspectorGUI() 
    {
        var timelineAnimations = ((TimelineAnimationStateMachineDebug)target).TimelineAnimations;
        
        var translations = timelineAnimations.Transitions.Where(v => v.fromName == timelineAnimations.CurrentState);
        EditorGUILayout.LabelField($"Change state from '{timelineAnimations.CurrentState}' to:");
        foreach (var transition in translations)
        {
            if(GUILayout.Button(transition.toName))
                timelineAnimations.ChangeState(transition.toName);
        }

        EditorGUILayout.Space();
        
        if(GUILayout.Button("Reset"))
            timelineAnimations.SetState(timelineAnimations.DefaultState);
        
        DrawLine();
        
        EditorGUILayout.LabelField($"Apply effect:");
        foreach (var effect in timelineAnimations.Effects)
        {
            if(GUILayout.Button(effect.name))
                timelineAnimations.ApplyEffect(effect.name);
        }
    }
    
    private void DrawLine()
    {
        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }
}
#endif