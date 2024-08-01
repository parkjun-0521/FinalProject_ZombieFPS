using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NPC : MonoBehaviourPun
{
    [SerializeField]TextMesh textMesh;
    Coroutine corTextBubbleQuest;
    Coroutine corTextBubbleQuestClear;
    string[] QuestCleardialogue = { "     ㄱ", "     고", "     고ㅁ", "     고마", "     고마ㅇ", "     고마우", "     고마워", "     고마워.", "     고마워..", "     고마워...", "     고마워...." };


    public void QusetTalkRPC()
    {
        if (corTextBubbleQuest != null) return;
        photonView.RPC("TalkQuest", RpcTarget.All);
    }

    [PunRPC]
    void TalkQuest()
    {
        corTextBubbleQuest = StartCoroutine(TextBubbleQuest());
    }

    IEnumerator TextBubbleQuest()
    {
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "ㅇ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "이";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인ㅎ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인혀";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형ㅇ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형으";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 ㅊ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 차";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾ㅇ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾아";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾아 ㅈ";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾아 주";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "인형을 찾아 줘";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "인형을 찾아 줘.";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "인형을 찾아 줘..";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "인형을 찾아 줘...";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "";

        corTextBubbleQuest = null;
    }



    public void QusetClearTalkRPC()
    {
        if (corTextBubbleQuestClear != null) return;
        photonView.RPC("TalkQuestClear", RpcTarget.All);
    }
    [PunRPC]
    void TalkQuestClear()
    {
        corTextBubbleQuestClear = StartCoroutine(TextBubbleQuestClear());
    }
    IEnumerator TextBubbleQuestClear()
    {
        foreach(string temp in QuestCleardialogue)
        {
            yield return new WaitForSeconds(0.2f);
            textMesh.text = temp.ToString();
        }
    }
}
