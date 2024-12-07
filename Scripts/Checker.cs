using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Checker : MonoBehaviour
{
    private TileController tileController;
    private GameObject[] tiles;
    public bool canBlessings;
    public int canConcealedKongCounter;
    //�ԃh���͓_�����������ߐԃh�����ʏ�̔v�������ꂼ��, 0, 10, 20 �� 5, 15, 25�ŋ�ʂ���.
    //0���ݎq��5(������5)�̐ԃh��. 10�͓��q��5(������15)�̐ԃh��. 20�͍��q��5(������25)�̐ԃh��.
    public int[] redDoraTileNums = new int[] { 0, 10, 20 };
    public int[] blackTileNumsHave5 = new int[] { 5, 15, 25 };  
    public List<int> canConcealedKongTileNum;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckerSetting()
    {
        tileController = GetComponent<TileController>();
        tiles = tileController.tiles;
        canBlessings = true;
    }

    //�V�a�A�n�aChecker.
    public bool CanBlessing(List<int> hand, int[] mainHandArray)
    {
        bool canBlessing = false;
        //�����`�� canBlessings �͒N���Ȃ��Ă��Ȃ���, �S���V�a, �n�a�ł����Ԃ���\���Ă���.
        if (!canBlessings)
        {
            return canBlessings;
        }

        //3���Ƃ������v��4�g+�c��2��(��v�ƈ����Ă����v)�������v�ł���ΓV�a, �n�a�ł���ƕԂ�.
        List<int> copyHand = new List<int>(hand);
        foreach (int mainHand in mainHandArray)
        {
            if (copyHand.Contains(mainHand))
            {
                copyHand.RemoveAll(handTile => handTile == mainHand);
            }
        }
        if(copyHand.Count == 2)
        {
            canBlessing = CheckIfSameTiles(copyHand[0], copyHand[1]);
        }
        return canBlessing;
    }

    //�c���l�ȎqChecker.
    public bool CanDrawFourQuads(List<int> hand)
    {
        //4��Ȃ��Ă���, ��v�ƈ����Ă����v�������v�ł���Ύl�Ȏq�ł���ƕԂ�.
        bool canFourQuads = false;
        if (hand.Count == 2)
        {
            canFourQuads = CheckIfSameTiles(hand[0], hand[1]);
        }
        return canFourQuads;
    }

    //�����l�ȎqChecker.
    public bool CanRonFourQuads(List<int> hand, bool furitenn, int lastDiscardedTileNum)
    {
        //4��Ȃ��Ă���, ��v�Ƒ��̃v���C���[�Ɏ̂Ă�ꂽ�v�������v�ł���Ύl�Ȏq�ł���ƕԂ�.
        //������, �U�蒮(���������łɎ̂Ă��v)�Ń����ł��Ȃ�.
        bool canFourQuads = false;
        if (hand.Count == 1 && !furitenn)
        {
            canFourQuads = CheckIfSameTiles(hand[0], lastDiscardedTileNum);
        }
        return canFourQuads;
    }

    //������Checker.
    public bool CanCountingGrandSlum(List<int> hand, int[] mainHandArray, int kongCount, int meldedKongCount, 
                        int doraCount, int finalTileWin, int kingsTileDraw)
    {
        bool canCountingGrandSlum = false;
        List<int> copyHand = new List<int>(hand);
        foreach (int mainHand in mainHandArray)
        {
            if (copyHand.Contains(mainHand))
            {
                copyHand.RemoveAll(handTile => handTile == mainHand);
            }
        }
        //3or4���Ƃ������v�ȊO�̎c��2��(��v�̓����v���Ȃ��v�ƈ����Ă����v)�������v��13�|�ȏ�Ȃ�.
        //�����𖞂ł���ƕԂ�.
        if (copyHand.Count != 2 || !CheckIfSameTiles(copyHand[0], copyHand[1]))
        {
            return canCountingGrandSlum;
        }
        int valueCount = 0;
        valueCount = (kongCount - 1) + meldedKongCount 
            + doraCount + finalTileWin + kingsTileDraw;
        if(valueCount >= 13)
        {
            canCountingGrandSlum = true;
        }
        return canCountingGrandSlum;
    }

    //�ԃh�����l�����������v���ǂ����̔���.
    //���O�̕t������, 
    //�ݎq�FA ���� (�ԃh���Ȃ�R����). ���q�FB ����(�ԃh���Ȃ�R����).
    //���q�FC ���� (�ԃh���Ȃ�R����). ���v�FH ���� (0���瓌, ��, ��, �k, ��, �, ��).
    //�ŏ��̓񕶎����r���邱�ƂŐԃh�����݂œ�����ނ̓����������ǂ�������ł���.
    private bool CheckIfSameTiles(int tileNum0, int tileNum1)
    {
        string pair_0 = tiles[tileNum0].name.Substring(0, 2);
        string pair_1 = tiles[tileNum1].name.Substring(0, 2);

        return pair_0 == pair_1;
    }

    //�U�蒮(= ������(��v�ŌǗ����Ă���)�v�������Ŋ��ɉ͂Ɏ̂ĂĂ��ă����ł��Ȃ����)Checker.
    public bool Furitenn(List<int> hand, int[] mainHandArray, List<int> insideWall)
    {
        bool furitenn = false;
        List<int> copyHand = new List<int>(hand);
        foreach (int mainHand in mainHandArray)
        {
            if (copyHand.Contains(mainHand))
            {
                copyHand.RemoveAll(handTile => handTile == mainHand);
            }
        }
        if (copyHand.Count == 1)
        {
            //������ނ̔v(�ԃh������)�����łɎ̂ĂĂ��Ȃ����m�F����.
            int tileNum = copyHand[0];
            if (redDoraTileNums.Contains(tileNum))
            {
                if (insideWall.Contains(tileNum + 5))
                {
                    furitenn = true;
                }
            }
            else if (blackTileNumsHave5.Contains(tileNum))
            {
                if (insideWall.Contains(tileNum) || insideWall.Contains(tileNum - 5))
                {
                    furitenn = true;
                }
            }
            else
            {
                if (insideWall.Contains(tileNum))
                {
                    furitenn = true;
                }
            }
        }
        return furitenn;
    }

    //�Þ�Checker.
    public bool CanConcealedKong(int wallTilesCount, List<int> hand, int[] mainHandArray)
    {
        bool canConcealedKong = false;
        if(wallTilesCount == 0)
        {
            return canConcealedKong;
        }
        canConcealedKongCounter = 0;
        canConcealedKongTileNum.Clear();
        foreach (int mainHand in mainHandArray)
        {
            //�����v(�ԃh������)��4�������Ă���ΈÞȂł���.
            if (!redDoraTileNums.Contains(mainHand) && !blackTileNumsHave5.Contains(mainHand))
            {
                int sameTileCount = hand.Count(tileNum => tileNum == mainHand);
                if(sameTileCount == 4)
                {
                    canConcealedKong = true;
                    canConcealedKongCounter++;
                    canConcealedKongTileNum.Add(mainHand);
                }
            }
            else if (blackTileNumsHave5.Contains(mainHand))
            {
                int sameTileCount = hand.Count(tileNum => tileNum == mainHand);
                sameTileCount += hand.Count(tileNum => tileNum == mainHand - 5);
                if (sameTileCount == 4)
                {
                    canConcealedKong = true;
                    canConcealedKongCounter++;
                    canConcealedKongTileNum.Add(mainHand);
                }
            }
        }
        return canConcealedKong;
    }

    //�喾��Checker.
    public bool CanMeldedKong(int wallTilesCount, int[] mainHandArray, int lastDiscardedTileNum)
    {
        //3�������Ă���v�Ɠ����v�����v���C���[�Ɏ̂Ă�ꂽ�Ƃ��喾�Ȃł���.
        bool canMeldedKong = false;
        if (wallTilesCount == 0)
        {
            return canMeldedKong;
        }
        else if (mainHandArray.Contains(lastDiscardedTileNum))
        {
            canMeldedKong = true;
        }
        return canMeldedKong;
    }
}
