using UnityEngine;
using System.Collections;

public enum SelectionFlags
{
    NONE        = 0x00000000,
    SELECTED    = 0x00000001
};

public class ScriptableBehaviour : MonoBehaviour 
{
    private SelectionFlags selectionFlags;

	void Awake()
    {
        selectionFlags = SelectionFlags.NONE;
    }

    public bool isSelected()
    {
        return (selectionFlags & SelectionFlags.SELECTED) == SelectionFlags.SELECTED;
    }

    void OnMouseDown()
    {
        if (isSelected())
        {
            selectionFlags &= ~SelectionFlags.SELECTED;
        }
        else
        {
            selectionFlags |= SelectionFlags.SELECTED;
        }
    }
};