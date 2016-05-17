using UnityEngine;
using System;
using System.Collections.Generic;

public enum ParsingStateFlags
{
    NONE                    = 0x00000000,
    VARIABLE_DECLARATION    = 0x00000001,
    FUNCTION_CALL           = 0x00000002,
    PARSING_ARGUMENT_LIST   = 0x00000004
};

public class Function
{
    public List<string> m_ArgumentList { get; set; }
    public string m_Signature { get; set; }
    
    public Function()
    {
        m_ArgumentList = new List<string>();
        m_Signature = "";
    }

    public Function(string signature, List<string> argumentList)
    {
        m_ArgumentList = new List<string>(argumentList);
        m_Signature = signature;
    }
};

public class ExecuteEventArgs : EventArgs
{
    public Queue<Function> m_ExecutionQueue { get; set; }
   
    public ExecuteEventArgs() : base()
    {
        m_ExecutionQueue = new Queue<Function>();
    }

    public ExecuteEventArgs(Queue<Function> executionQueue) : base()
    {
        m_ExecutionQueue = executionQueue;
    }
};

public class ShellObject : MonoBehaviour 
{
    public static GameObject[] m_ListOfAllGameObjects { get; private set; }

    public Material m_EnableMaterial;
    public Material m_DisableMaterial;

    public string m_Buffer { get; private set; }
    public string m_Output { get; private set; }
    public int m_MessageCount { get; private set; }

    public ParsingStateFlags m_ParsingStateFlags { get; private set; }

    public event EventHandler<ExecuteEventArgs> OnExecuteStateReached;

    public void SignalExecuteStateReached(Queue<Function> executionQueue)
    {
        EventHandler<ExecuteEventArgs> handler = OnExecuteStateReached;
        if (handler != null)
        {
            handler(this, new ExecuteEventArgs(executionQueue));
        }
    }

    public void InitializeGameObjectList()
    {
        m_ListOfAllGameObjects = FindObjectsOfType<GameObject>();
    }

    public void Output(string message)
    {
        m_Output += (message + '\n');
        ++m_MessageCount;
    }

	void Awake () 
    {
        InitializeGameObjectList();
        m_Buffer = "";
        m_Output = "";
        m_MessageCount = 0;
        m_ParsingStateFlags = ParsingStateFlags.NONE;
	}
	
	void Update () 
    {
        InitializeGameObjectList();
        for (int i = 0; i < m_ListOfAllGameObjects.Length; ++i)
        {
            GameObject gameObject = m_ListOfAllGameObjects[i];
            ScriptableGameObject scriptableGameObject = gameObject.GetComponent<ScriptableGameObject>();

            if (scriptableGameObject)
            {
                MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    if (scriptableGameObject.isEnabled())
                    {
                        meshRenderer.material = m_EnableMaterial;
                    }
                    else
                    {
                        meshRenderer.material = m_DisableMaterial;
                    }
                }
            }
        }
	}

    void OnGUI()
    {
        m_Buffer = GUI.TextArea(new Rect(0.0f, 0.0f, 300.0f, 250.0f), m_Buffer);
        m_Output = GUI.TextArea(new Rect(0.0f, 250.0f, 300.0f, 100.0f), m_Output);
        if (GUI.Button(new Rect(0.0f, 350.0f, 300.0f, 50.0f), "Run"))
        {
            Queue<Function> functionExecutionQueue = new Queue<Function>();
            List<string> currentFunctionArgumentList = new List<string>();
            string currentVariableName = "";
            string currentFunctionName = "";
            string currentFunctionArgument = "";

            try
            {
                for (int i = 0; i < m_Buffer.Length; ++i)
                {
                    char currentCharacter = m_Buffer[i];
                    switch (currentCharacter)
                    {
                        case '\n':
                        {
                                break;
                        }
                        case ' ':
                        {
                                break;
                        }
                        case '(':
                        {
                            if ((m_ParsingStateFlags & ParsingStateFlags.VARIABLE_DECLARATION) == ParsingStateFlags.VARIABLE_DECLARATION)
                            {
                                currentFunctionArgumentList = new List<string>();
                                currentFunctionName = currentVariableName;
                                currentVariableName = "";
                                currentFunctionArgument = "";
                                m_ParsingStateFlags |= ParsingStateFlags.FUNCTION_CALL;
                                m_ParsingStateFlags |= ParsingStateFlags.PARSING_ARGUMENT_LIST;
                                m_ParsingStateFlags &= ~ParsingStateFlags.VARIABLE_DECLARATION;
                            }
                            break;
                        }
                        case ',':
                        {
                            if ((m_ParsingStateFlags & ParsingStateFlags.PARSING_ARGUMENT_LIST) == ParsingStateFlags.PARSING_ARGUMENT_LIST)
                            {
                                if (currentFunctionArgument != "")
                                {
                                    currentFunctionArgumentList.Add(currentFunctionArgument);
                                    currentFunctionArgument = "";
                                }
                            }
                            break;
                        }
                        case ')':
                        {
                            if ((m_ParsingStateFlags & ParsingStateFlags.PARSING_ARGUMENT_LIST) == ParsingStateFlags.PARSING_ARGUMENT_LIST)
                            {
                                if (currentFunctionArgument != "")
                                {
                                    currentFunctionArgumentList.Add(currentFunctionArgument);
                                    currentFunctionArgument = "";
                                }
                                m_ParsingStateFlags &= ~ParsingStateFlags.PARSING_ARGUMENT_LIST;
                            }
                            break;
                        }
                        case ';':
                        {
                            if ((m_ParsingStateFlags & ParsingStateFlags.FUNCTION_CALL) == ParsingStateFlags.FUNCTION_CALL)
                            {
                                functionExecutionQueue.Enqueue(new Function(currentFunctionName, currentFunctionArgumentList));
                            }
                            currentFunctionArgumentList = new List<string>();
                            currentVariableName = "";
                            currentFunctionName = "";
                            currentFunctionArgument = "";
                            m_ParsingStateFlags = ParsingStateFlags.NONE;
                            break;
                        }
                        default:
                        {
                            if ((m_ParsingStateFlags & ParsingStateFlags.VARIABLE_DECLARATION) == ParsingStateFlags.VARIABLE_DECLARATION)
                            {
                                currentVariableName += currentCharacter;
                            }
                            else if ((m_ParsingStateFlags & ParsingStateFlags.PARSING_ARGUMENT_LIST) == ParsingStateFlags.PARSING_ARGUMENT_LIST)
                            {
                                currentFunctionArgument += currentCharacter;
                            }
                            else
                            {
                                currentVariableName = "";
                                currentVariableName += currentCharacter;
                                m_ParsingStateFlags |= ParsingStateFlags.VARIABLE_DECLARATION;
                            }
                            break;
                        }
                    }
                }
                
                foreach (Function f in functionExecutionQueue.ToArray())
                {
                    //Verify it exists
                    string signature = f.m_Signature;
                    //This method can be made reusable
                    if (SymbolsTables.m_TransformFunctionTable.ContainsKey(signature))
                    {
                        //Parse args
                        //This method can be made reusable
                        List<string> argumentList = f.m_ArgumentList;
                        int argumentCount = argumentList.Count;
                        if (argumentCount != 3)
                        {
                            throw new ScriptingException("ScriptingException: Wrong number of arguments: " + argumentCount + " for function: " + signature);
                        }
                        int[] parsedArgumentList = new int[3];
                        for (int j = 0; j < 3; ++j)
                        {
                            try
                            {
                                parsedArgumentList[j] = Int32.Parse(argumentList[j]);
                            }
                            catch (FormatException formatException)
                            {
                                throw new ScriptingException("ScriptingException: Argument: " + argumentList[j] + " Must be a number -- " + formatException.Message);
                            }
                        }
                        continue;
                    }
                    //else if other static symbols table maps contain signature key (perhaps make map static const)
                    else if (signature == "SLP")
                    {
                        List<string> argumentList = f.m_ArgumentList;
                        int argumentCount = argumentList.Count;
                        if (argumentCount != 1)
                        {
                            throw new ScriptingException("ScriptingException: Wrong number of arguments: " + argumentCount + " for function: " + signature);
                        }
                        int[] parsedArgumentList = new int[1];
                        for (int j = 0; j < 1; ++j)
                        {
                            try
                            {
                                parsedArgumentList[j] = Int32.Parse(argumentList[j]);
                            }
                            catch (FormatException formatException)
                            {
                                throw new ScriptingException("ScriptingException: Argument: " + argumentList[j] + " Must be a number -- " + formatException.Message);
                            }
                        }
                        continue;
                    }
                    throw new ScriptingException("Function: " + signature + " not found in the symbols table");
                }
                SignalExecuteStateReached(functionExecutionQueue);
                m_Output = "";
            }
            catch (ScriptingException scriptingException)
            {
                Output("ScriptingException: " + scriptingException.m_Message);
            }
        }
    }
};