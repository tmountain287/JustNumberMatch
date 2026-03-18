using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] private Text okConversationText = null;
    [SerializeField] private Text falseConversationText = null;
    [SerializeField] private List<GameObject> conversationList = null;

    [SerializeField] private List<GameObject> charObjList = null;

    public void SetConversation(ValidationResultType validationResultType, string segment)
    {
        for (int i = 0; i < conversationList.Count; i++)
        {
            conversationList[i].SetActive(i == (int)validationResultType);
            charObjList[i].SetActive(i == (int)validationResultType);
        }

        falseConversationText.text = segment;
        okConversationText.text = segment;
    }
    
}
