using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

//Photonネットワーク上のIDではNPCを扱えない.
//しかし, 麻雀はNPC用も含めて順にプレイしていくため, NPCも扱えるようにしたい.
//NPCとプレイヤー情報からこのクラスで実際にプレイヤーオブジェクトを生成する.
public class PeopleModel : MonoBehaviourPunCallbacks
{
    public GameObject personGO0 = null;
    public GameObject personGO1 = null;
    public GameObject personGO2 = null;
    public GameObject personGO3 = null;
    public List<GameObject> peopleGO;
    public Person person0;
    public Person person1;
    public Person person2;
    public Person person3;
    public List<Person> people;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakePeople(ExitGames.Client.Photon.Hashtable namesHashtable, 
        ExitGames.Client.Photon.Hashtable idsHashtable, 
        ExitGames.Client.Photon.Hashtable typesHashtable, 
        ExitGames.Client.Photon.Hashtable mainHandsHashtable, 
        ExitGames.Client.Photon.Hashtable handsHashtable)
    {
        person0 = personGO0.AddComponent<Person>();
        person1 = personGO1.AddComponent<Person>();
        person2 = personGO2.AddComponent<Person>();
        person3 = personGO3.AddComponent<Person>();
        peopleGO = new List<GameObject> { personGO0, personGO1, personGO2, personGO3 };
        people = new List<Person> { person0, person1, person2, person3 };

        for (int i = 0; i < people.Count; i++)
        {
            int order = i;
            string Name = (string) namesHashtable[order.ToString()];
            int Id = (int)idsHashtable[order.ToString()];
            int type = (int)typesHashtable[Id.ToString()];
            int[] mainHandArray = (int[])mainHandsHashtable[type.ToString()];
            int[] handArray = (int[])handsHashtable[type.ToString()];
            people[i].Initialize(Name, Id, order, type, mainHandArray, handArray);
        }
        
    }
}
