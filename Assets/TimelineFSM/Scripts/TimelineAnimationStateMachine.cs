using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

[Serializable][ExecuteAlways]
public class TimelineAnimationStateMachine : MonoBehaviour
{
    public string DefaultState;
    
    public string CurrentState;
    
    
    public StateTimeline[] States = new StateTimeline[] {};
    
    public TransitionTimeline[] Transitions = new TransitionTimeline[] {};
    
    public EffectTimeline[] Effects = new EffectTimeline[] {};
    
    
    private Dictionary<string, PlayableDirector> _statesBy = new Dictionary<string, PlayableDirector>();
    
    private Dictionary<string, Dictionary<string, PlayableDirector>> _transitionsBy = new Dictionary<string, Dictionary<string, PlayableDirector>>();
    
    private Dictionary<string, PlayableDirector> _effectsBy = new Dictionary<string, PlayableDirector>();
    
    
    private Queue<PlayableDirector> _timelinesQueue = new Queue<PlayableDirector>();

    
    private void Awake()
    {
        foreach (var state in States)
            _statesBy.Add(state.name, state.timeline);

        foreach (var transition in Transitions)
        {
            if (!_transitionsBy.TryGetValue(transition.fromName, out var dictionary))
            {
                dictionary = new Dictionary<string, PlayableDirector>();
                _transitionsBy.Add(transition.fromName, dictionary);
            }
            
            transition.timeline.extrapolationMode = DirectorWrapMode.None;
            dictionary.Add(transition.toName, transition.timeline);
        }
        
        foreach (var effect in Effects)
            _effectsBy.Add(effect.name, effect.timeline);

        SetState(DefaultState);
    }

    private void OnValidate()
    {
        foreach (var state in States)
            state.OnValidate();

        foreach (var transition in Transitions)
            transition.OnValidate();
        
        foreach (var effect in Effects)
            effect.OnValidate();
    }
    
    private void Update()
    {
        if (_timelinesQueue.Count == 0)
            return;

        var current = _timelinesQueue.Peek();
        if (current.state == PlayState.Playing)
            return;

        StopTimeline(current);
        _timelinesQueue.Dequeue();

        if (_timelinesQueue.Count == 0)
            return;
        
        StartTimeline(_timelinesQueue.Peek());
    }
    
    
    private bool TryGetState(string name, out PlayableDirector timeline)
    {
        return _statesBy.TryGetValue(name, out timeline);
    }
    
    private bool TryGetTransition(string fromName, string toName, out PlayableDirector timeline)
    {
        if (_transitionsBy.TryGetValue(fromName, out var dictionary))
            return dictionary.TryGetValue(toName, out timeline);

        timeline = default;
        return false;
    }
    
    private bool TryGetEffect(string name, out PlayableDirector timeline)
    {
        return _effectsBy.TryGetValue(name, out timeline);
    }
    

    private void StartTimeline(PlayableDirector timeline)
    {
        timeline.Play();
    }

    private void StopTimeline(PlayableDirector timeline, float progress = 1f)
    {
        timeline.time = timeline.duration * progress;
        timeline.Evaluate();
        
        timeline.Stop();
    }
    
    private void RestartTimelines()
    {
        if (_timelinesQueue.Count == 0)
            return;

        StartTimeline(_timelinesQueue.Peek());
    }

    private void StopTimelines()
    {
        while (_timelinesQueue.Count > 0)
        {
            var current = _timelinesQueue.Dequeue();
            if (current.state == PlayState.Playing)
                StopTimeline(current);
        }
    }
    
    
    public Task ChangeState(string name)
    {
        StopTimelines();

        var awaitTransition = Task.CompletedTask;
        if (TryGetTransition(CurrentState, name, out var transitionTimeline))
        {
            _timelinesQueue.Enqueue(transitionTimeline);
            awaitTransition = transitionTimeline.AsTask();
        }

        if (TryGetState(name, out var stateTimeline))
            _timelinesQueue.Enqueue(stateTimeline);

        CurrentState = name;
        
        RestartTimelines();

        return awaitTransition;
    }
    
    public void SetState(string name)
    {
        StopTimelines();

        if (TryGetState(name, out var stateTimeline))
            _timelinesQueue.Enqueue(stateTimeline);

        CurrentState = name;
        
        RestartTimelines();
    }
    
    public void ApplyEffect(string name)
    {
        if (!TryGetEffect(name, out var timeline))
            return;
        
        StopTimeline(timeline);
        StartTimeline(timeline);
    }

    public void EvaluateEffect(string name, float progress)
    {
        if (!TryGetEffect(name, out var timeline))
            return;
        StopTimeline(timeline, progress);
    }

    public bool HasState(string stateName)
    {
        return _statesBy.ContainsKey(stateName);
    }

    public bool HasEffect(string effectName)
    {
        return _effectsBy.ContainsKey(effectName);
    }
}


[Serializable]
public class StateTimeline
{
    public string name;
    public PlayableDirector timeline;
    
    public void OnValidate()
    {
        if (timeline == null)
            Debug.LogError($"TimelineAnimationStateMachine: State '{name}' does not have a timeline");
    }
}

[Serializable]
public class TransitionTimeline
{
    [HideInInspector]
    public string label;
    
    public string fromName;
    public string toName;
    public PlayableDirector timeline;
    
    public void OnValidate()
    {
        label = $"{fromName} -> {toName}";
        
        if (timeline == null)
            Debug.LogError($"TimelineAnimationStateMachine: Transition from '{fromName}' to '{toName}' does not have a timeline");
    }
}

[Serializable]
public class EffectTimeline
{
    public string name;
    public PlayableDirector timeline;
    
    public void OnValidate()
    {
        if (timeline == null)
            Debug.LogError($"TimelineAnimationStateMachine: Effect '{name}' does not have a timeline");
    }
}


public static class PlayableDirectorExtensions
{
    public static Task AsTask(this PlayableDirector playableDirector)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        void OnStopped(PlayableDirector innerPlayableDirector)
        {
            innerPlayableDirector.stopped -= OnStopped;
            taskCompletionSource.TrySetResult(true);
        }

        playableDirector.stopped += OnStopped;
           
        return taskCompletionSource.Task;
    }
}
