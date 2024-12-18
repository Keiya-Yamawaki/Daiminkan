using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;

public class WallTilesModel : MonoBehaviourPunCallbacks
{
    public List<int> mainHand0;
    public List<int> mainHand1;
    public List<int> mainHand2;
    public List<int> mainHand3;
    public List<List<int>> mainHands;
    public List<int> hand0;
    public List<int> hand1;
    public List<int> hand2;
    public List<int> hand3;
    public List<List<int>> hands;
    public List<int> wallTiles;     //牌山(≒山札).
    public List<int> deadWall;      //王牌(ドラ表示牌).
    public List<int> initialTilesList0 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public List<int> initialTilesList1 = new List<int> { 0, 1, 2, 3, 4, 5, 6 }; //字牌用.
    public List<int> shuffleList;
    public static int numKongTile = 14;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeWallTiles(int numPlayers)
    {
        if (hand0 == null)
        {
            mainHand0 = new List<int>();
            mainHand1 = new List<int>();
            mainHand2 = new List<int>();
            mainHand3 = new List<int>();
            mainHands = new List<List<int>> { mainHand0, mainHand1, mainHand2, mainHand3 };
            hand0 = new List<int>();
            hand1 = new List<int>();
            hand2 = new List<int>();
            hand3 = new List<int>();
            hands = new List<List<int>> { hand0, hand1, hand2, hand3 };
            wallTiles = new List<int>();
            deadWall = new List<int>();
        }
        else
        {
            mainHand0.Clear();
            mainHand1.Clear();
            mainHand2.Clear();
            mainHand3.Clear();
            mainHands = new List<List<int>> { mainHand0, mainHand1, mainHand2, mainHand3 };
            hand0.Clear();
            hand1.Clear();
            hand2.Clear();
            hand3.Clear();
            hands = new List<List<int>> { hand0, hand1, hand2, hand3 };
            wallTiles.Clear();
            deadWall.Clear();
        }

        for (int i = 0; i < numPlayers; i++)
        {
            MakeMainHands(i, numPlayers);
        }

        //全員13枚目の牌を牌山からランダムで引く.
        wallTiles = Shuffle(wallTiles);
        for (int j = 0; j < numPlayers; j++)
        {
            hands[j].Add(wallTiles[0]);
            wallTiles.RemoveAt(0);
        }

        //王牌を作る.
        for (int j = 0; j < numKongTile; j++)
        {
            deadWall.Add(wallTiles[wallTiles.Count - 1]);
            wallTiles.RemoveAt(wallTiles.Count - 1);
        }

    }

    public List<int> Shuffle(List<int> list)
    {
        List<int> copyList = new List<int>(list);
        for (int i = 0; i < list.Count; i++)
        {
            int k = Random.Range(0, list.Count);
            int temp = copyList[k];
            copyList[k] = copyList[i];
            copyList[i] = temp;
        }
        return copyList;
    }

    /*
     * 同じ牌3枚を同じ種類の牌4組(計3×4=12枚)で、1枚少ない手牌を構成する.
     * 同じ種類のそれ以外の牌をすべて牌山に加える.
    */
    public void MakeMainHands(int i, int numPlayers)
    {
        int numSameTile = 4;
        int addKindCount = 4;

        //字牌以外がメインの手牌を構成する.
        if(i != 3)
        {
            shuffleList = Shuffle(initialTilesList0);
            for (int j = 0; j < shuffleList.Count; j++)
            {
                int addNumber = 10 * i + shuffleList[j];
                /*
                 * 萬子0-9, 筒子10-19, 索子20-29, 字牌30-36.
                 * 数牌の数字が1-9で下一桁をそのまま対応させるために赤ドラは0, 10, 20とした.
                 * 赤ドラはそれぞれ5, 15, 25と同じ数の牌で一枚ずつある.
                 * しかし, 5, 15, 25より点数が高いため区別する.
                */

                if (j < addKindCount)
                {
                    if (shuffleList[j] != 5)
                    {
                        mainHands[i].Add(addNumber);
                        hands[i].AddRange(Enumerable.Repeat(addNumber, numSameTile - 1));
                        wallTiles.Add(addNumber);
                    }
                    else
                    {
                        int addRedDoraCount = 0;
                        int randomNum = Random.Range(0, 4);
                        if(randomNum != 0)
                        {
                            addRedDoraCount = 1;
                        }
                        int addRedDoraNumber = 10 * i;
                        mainHands[i].Add(addRedDoraNumber);
                        mainHands[i].Add(addNumber);
                        hands[i].AddRange(Enumerable.Repeat(addRedDoraNumber, addRedDoraCount));
                        hands[i].AddRange(Enumerable.Repeat(addNumber, numSameTile - 1 - addRedDoraCount));
                        wallTiles.AddRange(Enumerable.Repeat(addRedDoraNumber, 1 - addRedDoraCount));
                        wallTiles.AddRange(Enumerable.Repeat(addNumber, addRedDoraCount));
                    }
                }
                else
                {
                    if (shuffleList[j] != 5)
                    {
                        wallTiles.AddRange(Enumerable.Repeat(addNumber, numSameTile));
                    }
                    else
                    {
                        int addRedDoraNumber = 10 * i;
                        wallTiles.Add(addRedDoraNumber);
                        wallTiles.AddRange(Enumerable.Repeat(addNumber, numSameTile - 1));
                    }
                }
            }

        }
        //字牌がメインの手牌を構成する.
        else
        {
            shuffleList = Shuffle(initialTilesList1);
            for (int j = 0; j < shuffleList.Count; j++)
            {
                int addNumber = 10 * i + shuffleList[j];
                if (j < addKindCount)
                {
                    mainHands[i].Add(addNumber);
                    hands[i].AddRange(Enumerable.Repeat(addNumber, numSameTile - 1));
                    wallTiles.Add(addNumber);
                }
                else
                {
                    wallTiles.AddRange(Enumerable.Repeat(addNumber, numSameTile));
                }
            }
        }
    }

}
