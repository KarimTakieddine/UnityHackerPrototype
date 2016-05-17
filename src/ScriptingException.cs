using UnityEngine;
using System;

public class ScriptingException : Exception
{
    public string m_Message { get; set; }

    public ScriptingException() : base()
    {
        m_Message = "";
    }

    public ScriptingException(string message)
    {
        m_Message = message;
    }
};