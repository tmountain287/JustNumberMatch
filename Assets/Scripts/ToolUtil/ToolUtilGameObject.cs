using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ToolUtilGameObject : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private bool isIgnore = false;
    [SerializeField] private bool isOff = true;
    [SerializeField] private bool olnyOff = false;
    #endregion

    public void SetGameObject(bool _on)
    {
        if (isIgnore)
            return;

        if(olnyOff)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(_on ? isOff : !isOff);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        EditorApplication.QueuePlayerLoopUpdate();
#endif
    }
}

