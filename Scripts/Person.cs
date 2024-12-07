using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Photonネットワーク上のIDではNPCを扱えない.
//しかし, 麻雀はNPC用も含めて順にプレイしていくため, NPCも扱えるようにしたい.
//そこで, NPCとプレイヤー情報をこのクラスで設定する.
public class Person : MonoBehaviour
{
    public string Name;
    public bool isHuman;  
    public int Id;   
    public int order;    //プレイ順序.
    public int type;    //最初の牌の種類.
    public int[] mainHandArray;  //手牌は4つ数字の牌が3枚ずつ配られる. その4つの数字.
    public List<int> hand;  //手牌.
    public List<int> kongTiles = new List<int>(); //槓した(鳴いた)牌.
    public List<int> insideWall = new List<int>();    //河.
    public bool didConcealedKong = false;
    public bool furitenn = false;   //振り聴：河に捨てた牌でロンできない.
    public bool judged = false;     //処理済みかどうか.
    public bool ron = false;        //人が捨てた牌で和了ったかどうか.
    public bool draw = false;       //引いた牌で和了ったかどうか.
    public bool didMeldedKong = false;
    public bool countedDora = false;    //ドラ：点が上がるボーナス牌.
    public int kingsTileDraw = 0;   //嶺上開花(槓して引いたときに上がれると付く役).
    public int finalTileWin = 0;    //海底撈月(牌山最後の牌で上がった時につく役).
    public int doraCount = 0;   //手牌のドラの数.
    public int kongCount = 0;   //槓した回数.
    public int meldedKongCount = 0; //大明槓した回数.
    public int concealedKongCount = 0;  //暗槓した回数.

    //NPCとプレイヤー情報を以下の関数で設定する.
    public void Initialize(string Name, int Id, int order, 
        int type, int[] mainHandArray , int[] handArray)
    {
        this.Name = Name;
        this.Id = Id;
        this.isHuman = Id > 0;
        this.order = order;
        this.type = type;
        this.mainHandArray = mainHandArray;
        this.hand = new List<int>(handArray);
    }

    public void DoraCountOfHand(List<int> doraList)
    {
        doraCount = 0;
        foreach (int handTile in hand)
        {
            int doraCountPerHnadTile = doraList.Count(tileNum => tileNum == handTile);
            doraCount += doraCountPerHnadTile;
        }
        foreach (int kongTile in kongTiles)
        {
            int doraCountPerKongTile = doraList.Count(tileNum => tileNum == kongTile);
            doraCount += doraCountPerKongTile;
        }
        countedDora = true;
    }


}
