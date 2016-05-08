using UnityEngine;
using System;
using System.Collections.Generic;

public enum StateFlags
{
    NONE            = 0x00000000,
    FUNCTION_CALL   = 0x00000001,
    PARSING_ARGS    = 0x00000002
}

public class Shell : MonoBehaviour
{
    public static Dictionary<string, ExecutableFunctions.TransformVectorFunc> transformVectorFunctionMap;

    private List<string> functionArgList;
    private string currentFunctionArg;
    private string content;
    private string output;
    private string functionCall;
    private int currentArgIndex;

    private Rect consoleTextAreaRect;
    private Rect outputTextAreaRect;
    private Rect runButtonRect;
    private Rect clearButtonRect;

    private StateFlags state;

    void Awake()
    {
        transformVectorFunctionMap = new Dictionary<string, ExecutableFunctions.TransformVectorFunc>();
        transformVectorFunctionMap["MOV"] = new ExecutableFunctions.TransformVectorFunc(ExecutableFunctions.move);
        transformVectorFunctionMap["ROT"] = new ExecutableFunctions.TransformVectorFunc(ExecutableFunctions.rotate);

        functionArgList = new List<string>();
        currentFunctionArg = "";
        content = "";
        output = "";
        functionCall = "";
        currentArgIndex = 0;

        consoleTextAreaRect = new Rect(0.0f, 0.0f, 300.0f, 300.0f);
        outputTextAreaRect = new Rect(0.0f, consoleTextAreaRect.height, consoleTextAreaRect.width, 150.0f);
        runButtonRect = new Rect(consoleTextAreaRect.width, consoleTextAreaRect.y, 50.0f, 50.0f);
        clearButtonRect = new Rect(consoleTextAreaRect.width, consoleTextAreaRect.y + runButtonRect.height, 50.0f, 50.0f);

        state = StateFlags.NONE;
    }

    void OnGUI()
    {
        content = GUI.TextArea(consoleTextAreaRect, content);
        output = GUI.TextArea(outputTextAreaRect, output);

        if (GUI.Button(runButtonRect, "Run"))
        {
            int gameObjectCount = Monitor.allGameObjects.Length;
            for (uint i = 0; i < gameObjectCount; ++i)
            {
                ScriptableBehaviour scriptableBehaviour = Monitor.allGameObjects[i].GetComponent<ScriptableBehaviour>();
                if (scriptableBehaviour && scriptableBehaviour.isSelected())
                {
                    try
                    {
                        for (int j = 0; j < content.Length; j++)
                        {
                            char character = content[j];
                            switch (character)
                            {
                                case ';':
                                {
                                    if (transformVectorFunctionMap.ContainsKey(functionCall))
                                    {
                                        if (functionArgList.Count != 3)
                                        {
                                            throw new ScriptingException("Function argument list to: " + functionCall + " needs 3 arguments");
                                        }
                                        int[] argList = new int[3];
                                        for (int k = 0; k < 3; ++k)
                                        {
                                            try
                                            {
                                                argList[k] = System.Int32.Parse(functionArgList[k]);
                                            }
                                            catch (System.FormatException e)
                                            {
                                                throw new ScriptingException("Function argument list to: " + functionCall + " is invalid due to a parsing error: " + e.Message);
                                            }
                                        }
                                        transformVectorFunctionMap[functionCall](argList[0], argList[1], argList[2], scriptableBehaviour.transform);
                                    }
                                    //Else if other map contains function call
                                    else
                                    {
                                        throw new ScriptingException("Failed to find " + functionCall + "() in list of available functions.");
                                    }
                                    Debug.Log(functionCall);
                                    foreach (string s in functionArgList.ToArray())
                                    {
                                        Debug.Log(s);
                                    }
                                    functionCall = "";
                                    state &= ~(StateFlags.FUNCTION_CALL | StateFlags.PARSING_ARGS);

                                    break;
                                }
                                case '(':
                                {
                                    if ((state & StateFlags.FUNCTION_CALL) == StateFlags.FUNCTION_CALL)
                                    {
                                        //Start parsing args
                                        functionArgList = new List<string>();
                                        currentArgIndex = 0;
                                        currentFunctionArg = "";
                                        state &= ~StateFlags.FUNCTION_CALL;
                                        state |= StateFlags.PARSING_ARGS;
                                    }
                                    break;
                                }
                                case ',':
                                {
                                    if ((state & StateFlags.PARSING_ARGS) == StateFlags.PARSING_ARGS)
                                    {
                                        functionArgList.Add(currentFunctionArg);
                                        currentFunctionArg = "";
                                        ++currentArgIndex;
                                    }
                                    break;
                                }
                                case ')':
                                {
                                    if ((state & StateFlags.PARSING_ARGS) == StateFlags.PARSING_ARGS)
                                    {
                                        functionArgList.Add(currentFunctionArg);
                                        state &= ~StateFlags.PARSING_ARGS;
                                    }
                                    break;
                                }
                                //All whitespace is ignored
                                case ' ':
                                {
                                    break;
                                }
                                case '\n':
                                {
                                    break;
                                }
                                default:
                                {
                                    if ((state & StateFlags.FUNCTION_CALL) == StateFlags.FUNCTION_CALL)
                                    {
                                        functionCall += character;
                                    }
                                    else if ((state & StateFlags.PARSING_ARGS) == StateFlags.PARSING_ARGS)
                                    {
                                        currentFunctionArg += character;
                                    }
                                    else
                                    {
                                        functionCall = "";
                                        functionCall += character;
                                        state |= StateFlags.FUNCTION_CALL;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (ScriptingException e)
                    {
                        output += ("Scripting exception! " + e.m_message + '\n');
                    }
                }
            }
        }
        else if (GUI.Button(clearButtonRect, "Clear"))
        {
            content = "";
        }
    }
};