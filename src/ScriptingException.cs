using UnityEngine;
using System;

public class ScriptingException : Exception
{
    public string m_message { get; private set; }

    public ScriptingException() : base()
    {
        m_message = "";
    }

    public ScriptingException(string message) : base()
    {
        m_message = message;
    }

    public ScriptingException setMessage(string message)
    {
        m_message = message;
        return this;
    }
};