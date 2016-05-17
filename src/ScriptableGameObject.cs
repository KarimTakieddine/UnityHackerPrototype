using UnityEngine;
using System;
using System.Collections.Generic;

public enum ScriptableStateFlags
{
    NONE        = 0x00000000,
    ENABLED     = 0x00000001
};

public class ScriptableGameObject : MonoBehaviour 
{
    public ShellObject m_ShellObject { get; private set; }
    public GameObject m_ShellGameObject;
    public ScriptableStateFlags m_ScriptableState { get; private set; }

    private Queue<Function> m_CurrentExecutionQueue;
    private int m_CurrentExecutionCount;
    private int m_CurrentExecutionLimit;
    private GameTimer m_ExecutionTimer;
    private GameTimer m_ReleaseTimer;
	
	void Awake () 
    {
        m_ShellObject = m_ShellGameObject.GetComponent<ShellObject>();
        if (!m_ShellGameObject)
        {
            throw new ScriptingException("m_ShellGameObject to connect to is unknown");
        }
        else if (!m_ShellObject)
        {
            throw new ScriptingException("m_ShellObject to component is missing from Shell");
        }
        m_CurrentExecutionQueue = new Queue<Function>();
        m_ExecutionTimer = new GameTimer(0.0f);
        m_ReleaseTimer = new GameTimer(0.0f);
        m_CurrentExecutionCount = 0;
        m_CurrentExecutionLimit = 0;
        m_ScriptableState = ScriptableStateFlags.NONE;

        m_ExecutionTimer.TargetTimeReached += OnExecutionTargetTimeReached;
        m_ReleaseTimer.TargetTimeReached += OnReleaseTargetTimeReached;
        m_ExecutionTimer.Pause();
        m_ReleaseTimer.Pause();
	}

    void OnShellExecuteStateReached(object sender, ExecuteEventArgs executeEventArgs)
    {
        m_CurrentExecutionQueue = new Queue<Function>(executeEventArgs.m_ExecutionQueue);
        m_CurrentExecutionCount = 0;
        m_CurrentExecutionLimit = m_CurrentExecutionQueue.Count;
        //Start execution timer
        m_ExecutionTimer.Reset(true);
        m_ExecutionTimer.Resume();
    }

    void OnReleaseTargetTimeReached(object sender, TimerEventArgs timerEventArgs)
    {
        m_ExecutionTimer.Resume();
        m_ReleaseTimer.Pause();
    }

    void OnExecutionTargetTimeReached(object sender, TimerEventArgs timerEventArgs)
    {
        if (m_CurrentExecutionCount < m_CurrentExecutionLimit)
        {
            Function function = m_CurrentExecutionQueue.Dequeue();
            List<string> argumentList = function.m_ArgumentList;
            string signature = function.m_Signature;
            if (SymbolsTables.m_TransformFunctionTable.ContainsKey(signature))
            {
                int[] parsedArgumentList = new int[3];
                for (int i = 0; i < 3; ++i)
                {
                    parsedArgumentList[i] = Int32.Parse(argumentList[i]);
                }
                SymbolsTables.m_TransformFunctionTable[signature](parsedArgumentList[0], parsedArgumentList[1], parsedArgumentList[2], transform);
            }
            else if (signature == "SLP")
            {
                float parsedSeconds = float.Parse(argumentList[0]);
                m_ExecutionTimer.Pause();
                m_ReleaseTimer.m_TargetTime = parsedSeconds;
                m_ReleaseTimer.Reset(true);
                m_ReleaseTimer.Resume();
            }
            ++m_CurrentExecutionCount;
        }
    }

    public bool isEnabled()
    {
        return (m_ScriptableState & ScriptableStateFlags.ENABLED) == ScriptableStateFlags.ENABLED;
    }

    void OnMouseDown()
    {
        if (isEnabled())
        {
            m_ShellObject.OnExecuteStateReached -= OnShellExecuteStateReached;
            m_ScriptableState &= ~ScriptableStateFlags.ENABLED;
        }
        else
        {
            m_ShellObject.OnExecuteStateReached += OnShellExecuteStateReached;
            m_ScriptableState |= ScriptableStateFlags.ENABLED;
        }
    }

    void Update()
    {
        m_ExecutionTimer.Monitor();
        m_ReleaseTimer.Monitor();
    }
};