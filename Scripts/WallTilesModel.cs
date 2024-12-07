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
    public List<int> wallTiles;     //�v�R(���R�D).
    public List<int> deadWall;      //���v(�h���\���v).
    public List<int> initialTilesList0 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public List<int> initialTilesList1 = new List<int> { 0, 1, 2, 3, 4, 5, 6 }; //���v�p.
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

        //�S��13���ڂ̔v��v�R���烉���_���ň���.
        wallTiles = Shuffle(wallTiles);
        for (int j = 0; j < numPlayers; j++)
        {
            hands[j].Add(wallTiles[0]);
            wallTiles.RemoveAt(0);
        }

        //���v�����.
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
     * �����v3���𓯂���ނ̔v4�g(�v3�~4=12��)�ŁA1�����Ȃ���v���\������.
     * ������ނ̂���ȊO�̔v�����ׂĔv�R�ɉ�����.
    */
    public void MakeMainHands(int i, int numPlayers)
    {
        int numSameTile = 4;
        int addKindCount = 4;

        //���v�ȊO�����C���̎�v���\������.
        if(i != 3)
        {
            shuffleList = Shuffle(initialTilesList0);
            for (int j = 0; j < shuffleList.Count; j++)
            {
                int addNumber = 10 * i + shuffleList[j];
                /*
                 * �ݎq0-9, ���q10-19, ���q20-29, ���v30-36.
                 * ���v�̐�����1-9�ŉ��ꌅ�����̂܂ܑΉ������邽�߂ɐԃh����0, 10, 20�Ƃ���.
                 * �ԃh���͂��ꂼ��5, 15, 25�Ɠ������̔v�ňꖇ������.
                 * ������, 5, 15, 25���_�����������ߋ�ʂ���.
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
        //���v�����C���̎�v���\������.
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
