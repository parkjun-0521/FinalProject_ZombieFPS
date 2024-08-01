using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NPC : MonoBehaviourPun
{
    [SerializeField]TextMesh textMesh;
    Coroutine corTextBubbleQuest;
    Coroutine corTextBubbleQuestClear;
    string[] QuestCleardialogue = { "     ��", "     ��", "     ��", "     ��", "     ����", "     ����", "     ����", "     ����.", "     ����..", "     ����...", "     ����...." };


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
        textMesh.text = "��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "�Τ�";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "����";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "����";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã�� ��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã�� ��";
        yield return new WaitForSeconds(0.2f);
        textMesh.text = "������ ã�� ��";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "������ ã�� ��.";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "������ ã�� ��..";
        yield return new WaitForSeconds(1.5f);
        textMesh.text = "������ ã�� ��...";
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
