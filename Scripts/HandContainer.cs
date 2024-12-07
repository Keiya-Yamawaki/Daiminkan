using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandContainer : MonoBehaviour, IEnumerable<HandContainerChild>
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //牌を引くたびに手牌のスクリプトを自動でまとめて,
    //手牌それぞれの牌のスクリプトとその数を index で管理できるようにしている.

    public List<HandContainerChild> handGOList = new List<HandContainerChild>();
    public HandContainerChild this[int index] => handGOList[index];
    public int Count => handGOList.Count;

    private void OnTransformChildrenChanged()
    {
        handGOList.Clear();
        foreach (Transform child in transform)
        {
            handGOList.Add(child.GetComponent<HandContainerChild>());
        }
    }

    public IEnumerator<HandContainerChild> GetEnumerator()
    {
        return handGOList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
