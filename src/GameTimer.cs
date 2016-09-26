/* ----------------------------------------------------------------------------------------------------- *\

Copyright(c) 2016 Karim Takieddine

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files(the "Software"), to deal in the Software without restriction,
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and / or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions :

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN
NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

\* ----------------------------------------------------------------------------------------------------- */

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