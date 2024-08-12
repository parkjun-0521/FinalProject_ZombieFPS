using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class QuestManager : MonoBehaviourPun
{
    public static QuestManager instance;
    [SerializeField]Text text;
    List<string> questList = new List<string>();
    List<string> questList1 = new List<string>();
    List<string> questList2 = new List<string>();

    
    List<bool> isQuestList;
    List<bool> isQuestList1;
    List<bool> isQuestList2 = new List<bool>();

    bool isQuest_0;
    bool isQuest_1;
    bool isQuest_2;
    bool isQuest_3;

    bool isQuest1_0;
    bool isQuest1_1;
    bool isQuest1_2;
    bool isQuest1_3;

    bool isQuest2_0;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        questList.Add("Npc�� ��ȭ�ϱ�");
        questList.Add("������ ã�� Npc���� �����ϱ�");
        questList.Add("���� �������� �̵�");

        questList1.Add("Npc�� ��ȭ�ϱ�");
        questList1.Add("������ óġ�ϰ� ���� �����ϱ�");
        questList1.Add("���� �������� �̵�");

        questList2.Add("���� óġ�ϱ�");

        isQuestList = new List<bool> { isQuest_0, isQuest_1, isQuest_2, isQuest_3 };
        isQuestList1 = new List<bool> { isQuest1_0, isQuest1_1, isQuest1_2, isQuest1_3 };
        if (ScenesManagerment.Instance.stageCount == 0) //1�������� 
        {
            StartCoroutine(QuestClear(3, 2, 0));
        }
        else if(ScenesManagerment.Instance.stageCount == 1) //2
        {
            StartCoroutine(QuestClear(3, 2, 1));
        }
        else if (ScenesManagerment.Instance.stageCount == 2) //3
        {
            StartCoroutine(QuestClear(3, 2, 2));
        }
    }

    //�غ��ϱ� 3, 3, stage�� ����
    IEnumerator QuestClear(float time, float _time, int stageCount, int isQuest = 0)
    {
        if(stageCount == 0)
        {
            if (isQuestList[isQuest] == true) yield break;
            isQuestList[isQuest] = true;
        }
        else if(stageCount == 1)
        {
            if (isQuestList1[isQuest] == true) yield break;
            isQuestList1[isQuest] = true;
        }

        float alpha = 1;
        while(text.color.a != 0)
        {
            yield return null;
            alpha -= Time.deltaTime / time;
            text.color = Color.Lerp(Color.clear, Color.white, alpha);
        }
        yield return null; //���� �ٷ� �Ѿ�� ����غ����� ����
        StartCoroutine(QuestCreate(_time, stageCount));
    }

    IEnumerator QuestCreate(float time, int stageCount)
    {
        float alpha = 0;
        text.color = Color.clear;
        if (stageCount == 0)
        {
            text.text = questList[0];
            questList.RemoveAt(0);
        }
        else if(stageCount == 1)
        {
            text.text = questList1[0];
            questList1.RemoveAt(0);
        }
        else if (stageCount == 2)
        {
            text.text = questList2[0];
            questList2.RemoveAt(0);
        }
        else
        {
            yield break;
        }


        while (text.color.a != 1)
        {
            yield return null;
            alpha += Time.deltaTime / time;
            text.color = Color.Lerp(Color.clear, Color.white, alpha);
        }
    }
    
    [PunRPC]
    void QuestClearRpc(int isQuest)
    {
        StartCoroutine(QuestClear(3, 2, ScenesManagerment.Instance.stageCount, isQuest));
    }
    public void QuestClearRPC(int isQuest)
    {
        photonView.RPC("QuestClearRpc", RpcTarget.All, isQuest);
    }

}
