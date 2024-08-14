using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class QuestManager : MonoBehaviourPun
{
    public static QuestManager instance;
    [SerializeField]Text text;
    Dictionary<int, string> questDic = new Dictionary<int, string>();
    Dictionary<int, string> questDic1 = new Dictionary<int, string>();
    Dictionary<int, string> questDic2 = new Dictionary<int, string>();
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
       
        questDic.Add(0, "Npc와 대화하기");
        questDic.Add(1, "인형을 찾고 Npc에게 전달하기");
        questDic.Add(2, "다음 목적지로 이동");

        questDic1.Add(0, "Npc와 대화하기");
        questDic1.Add(1, "인형을 찾고 Npc에게 전달하기");
        questDic1.Add(2, "다음 목적지로 이동");

        questDic2.Add(0, "보스 처치하기");
        
        
        if (ScenesManagerment.Instance.stageCount == 0) //1스테이지 
        {
            StartCoroutine(QuestClearDic(3, 2, 0));
        }
        else if(ScenesManagerment.Instance.stageCount == 1) //2
        {
            StartCoroutine(QuestClearDic(3, 2, 1));
        }
        else if (ScenesManagerment.Instance.stageCount == 2) //3
        {
            StartCoroutine(QuestClearDic(3, 2, 2));
        }
    }

    IEnumerator QuestClearDic(float time, float _time, int stageCount, int isQuest = 0)
    {
        if(stageCount == 0)
        {
            if (!questDic.ContainsKey(isQuest)) yield break;
        }
        else if (stageCount == 1)
        {
            if (!questDic1.ContainsKey(isQuest)) yield break;
        }
        else if(stageCount == 2)
        {
            if (!questDic2.ContainsKey(isQuest)) yield break;
        }
        else
        {
            yield break;
        }

        float alpha = 1;
        while (text.color.a != 0)
        {
            yield return null;
            alpha -= Time.deltaTime / time;
            text.color = Color.Lerp(Color.clear, Color.white, alpha);
        }
        yield return null;

        if (stageCount == 0)
        {
            if (!questDic.ContainsKey(isQuest)) yield break;

            text.text = questDic[isQuest];
            questDic.Remove(isQuest);
        }
        else if(stageCount == 1)
        {
            if (!questDic.ContainsKey(isQuest)) yield break;

            text.text = questDic1[isQuest];
            questDic1.Remove(isQuest);
        }
        else if(stageCount == 2)
        {
            if (!questDic.ContainsKey(isQuest)) yield break;

            text.text = questDic2[isQuest];
            questDic2.Remove(isQuest);
        }
        else
        {
            yield break;
        }
        StartCoroutine(QuestCreateDic(time, stageCount));
    }

    IEnumerator QuestCreateDic(float time, int stageCount)
    {

        float alpha = 0;
        text.color = Color.clear;
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
        StartCoroutine(QuestClearDic(3, 2, ScenesManagerment.Instance.stageCount, isQuest));
    }
    public void QuestClearRPC(int isQuest)
    {
        photonView.RPC("QuestClearRpc", RpcTarget.All, isQuest);
    }

}
