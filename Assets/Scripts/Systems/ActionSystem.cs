using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ActionSystem : Singleton<ActionSystem>
{
    // ---- Subscriptions (pre/post) ----
    private static readonly Dictionary<Type, List<Action<GameAction>>> preSubs  = new();
    private static readonly Dictionary<Type, List<Action<GameAction>>> postSubs = new();

    // Keep a mapping so Unsubscribe works (delegate equality on new wrappers won’t match).
    private static readonly Dictionary<Delegate, Action<GameAction>> preWrapMap  = new();
    private static readonly Dictionary<Delegate, Action<GameAction>> postWrapMap = new();

    // ---- Performers by action type ----
    private static readonly Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    // ---- Runtime state ----
    private List<GameAction> reactions = null;           // points at the active reaction list
    public  bool IsPerforming { get; private set; } = false;

    // ---- Busy / processing event for UI (End Turn gating) ----
    public event System.Action<bool> OnProcessingChanged;
    private int _processingDepth = 0;
    public  bool IsProcessing => _processingDepth > 0;

    private void BeginProcessing()
    {
        if (_processingDepth++ == 0)
            OnProcessingChanged?.Invoke(true);
    }
    private void EndProcessing()
    {
        _processingDepth = Mathf.Max(0, _processingDepth - 1);
        if (_processingDepth == 0)
            OnProcessingChanged?.Invoke(false);
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Entry point to execute an action (with its pre/perform/post phases).
    /// NOTE: During an active flow, you should generally queue more work with AddReaction(...).
    /// </summary>
    public void Perform(GameAction action, System.Action onPerformFinished = null)
    {
        // Signal "busy" for the whole flow (including nested reactions).
        BeginProcessing();

        // If you REALLY want to forbid nested Perform calls, keep IsPerforming guard,
        // but still pair Begin/End so busy flag is correct.
        if (IsPerforming)
        {
            // Already in a flow — just finish the busy window we opened.
            EndProcessing();
            return;
        }

        IsPerforming = true;
        StartCoroutine(Flow(action, () =>
        {
            IsPerforming = false;
            onPerformFinished?.Invoke();
            EndProcessing();
        }));
    }

    /// <summary>Add a reaction to the CURRENT phase’s reaction list.</summary>
    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    // ------------------------------------------------------------------------

    private IEnumerator Flow(GameAction action, Action onFlowFinished = null)
    {
        // ----- PRE -----
        reactions = action.PreReactions ?? new List<GameAction>();
        PerformSubscribers(action, preSubs);
        yield return PerformReactions();

        // ----- PERFORM -----
        reactions = action.PerformReactions ?? new List<GameAction>();
        yield return PerformPerformer(action);
        yield return PerformReactions();

        // ----- POST -----
        reactions = action.PostReactions ?? new List<GameAction>();
        PerformSubscribers(action, postSubs);
        yield return PerformReactions();

        reactions = null;
        onFlowFinished?.Invoke();
    }

    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        var type = action.GetType();
        if (!subs.TryGetValue(type, out var list)) return;

        // Snapshot to avoid issues if subscribers add/remove during iteration.
        var snap = new List<Action<GameAction>>(list);
        foreach (var sub in snap)
            sub?.Invoke(action);
    }

    private IEnumerator PerformPerformer(GameAction action)
    {
        var type = action.GetType();
        if (performers.TryGetValue(type, out var perf) && perf != null)
            yield return perf(action);
    }

    /// <summary>
    /// Execute the current "reactions" list. Uses index-based loop so that calls to
    /// AddReaction(...) during iteration are processed in the same phase safely.
    /// </summary>
    private IEnumerator PerformReactions()
    {
        var list = reactions;
        if (list == null || list.Count == 0) yield break;

        int i = 0;
        while (i < list.Count)
        {
            var r = list[i];
            // Run each reaction as a full flow (pre/perform/post)
            yield return Flow(r);
            i++;
        }
    }

    // ------------------------------------------------------------------------
    // Performer registration
    // ------------------------------------------------------------------------

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        var type = typeof(T);
        IEnumerator wrapped(GameAction a) => performer((T)a);
        performers[type] = wrapped;
    }

    public static void DetachPerformer<T>() where T : GameAction
    {
        var type = typeof(T);
        if (performers.ContainsKey(type)) performers.Remove(type);
    }

    // ------------------------------------------------------------------------
    // Subscriber registration (PRE/POST)
    // ------------------------------------------------------------------------

    public static void SubscribePerformer<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        var subs     = timing == ReactionTiming.PRE ? preSubs     : postSubs;
        var wrapMap  = timing == ReactionTiming.PRE ? preWrapMap  : postWrapMap;

        if (reaction == null) return;

        Action<GameAction> wrapped = (ga) => reaction((T)ga);

        var key = (Delegate)reaction;
        wrapMap[key] = wrapped;

        var t = typeof(T);
        if (!subs.TryGetValue(t, out var list))
        {
            list = new List<Action<GameAction>>();
            subs[t] = list;
        }
        list.Add(wrapped);
    }

    public static void UnsubscribePerformer<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        var subs     = timing == ReactionTiming.PRE ? preSubs     : postSubs;
        var wrapMap  = timing == ReactionTiming.PRE ? preWrapMap  : postWrapMap;

        if (reaction == null) return;

        var t = typeof(T);
        if (!subs.TryGetValue(t, out var list)) return;

        var key = (Delegate)reaction;
        if (wrapMap.TryGetValue(key, out var wrapped))
        {
            list.Remove(wrapped);
            wrapMap.Remove(key);
        }
    }
}
