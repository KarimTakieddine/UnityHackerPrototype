using UnityEngine;
using System;

public class TimerEventArgs : EventArgs
{
    public float m_Time { get; set; }

    public TimerEventArgs() : base()
    {
        m_Time = 0.0f;
    }

    public TimerEventArgs(float time) : base()
    {
        m_Time = time;
    }
};

public enum GameTimerFlags
{
    NONE    = 0x00000000,
    PAUSED  = 0x00000001
};

public class GameTimer
{
    public float m_TargetTime { get; set; }
    public float m_CurrentTime { get; private set; }
    public GameTimerFlags m_TimerFlags { get; private set; }

    public event EventHandler<TimerEventArgs> TargetTimeReached;

    public GameTimer()
    {
        m_TargetTime = 0.0f;
        m_CurrentTime = 0.0f;
        m_TimerFlags = GameTimerFlags.NONE;
    }

    public GameTimer(float targetTime)
    {
        m_TargetTime = targetTime;
        m_CurrentTime = 0.0f;
        m_TimerFlags = GameTimerFlags.NONE;
    }

    public void Pause()
    {
        m_TimerFlags |= GameTimerFlags.PAUSED;
    }

    public void Resume()
    {
        m_TimerFlags &= ~GameTimerFlags.PAUSED;
    }

    public void Reset(bool pause = false)
    {
        m_CurrentTime = 0.0f;

        if (pause)
        {
            Pause();
        }
    }

    public bool isPaused()
    {
        return (m_TimerFlags & GameTimerFlags.PAUSED) == GameTimerFlags.PAUSED;
    }

    private void OnTargetTimeReached(TimerEventArgs timerEventArgs)
    {
        EventHandler<TimerEventArgs> handler = TargetTimeReached;
        if (handler != null)
        {
            handler(this, timerEventArgs);
        }
    }

    public void Monitor()
    {
        if (!isPaused())
        {
            if (m_CurrentTime >= m_TargetTime)
            {
                OnTargetTimeReached(new TimerEventArgs(m_CurrentTime));
                m_CurrentTime = 0.0f;
            }
            m_CurrentTime += Time.deltaTime;
        }
    }
};