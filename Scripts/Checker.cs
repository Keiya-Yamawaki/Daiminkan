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
    //赤ドラは点数が高いため赤ドラか通常の牌かをそれぞれ, 0, 10, 20 と 5, 15, 25で区別する.
    //0は萬子の5(数字は5)の赤ドラ. 10は筒子の5(数字は15)の赤ドラ. 20は索子の5(数字は25)の赤ドラ.
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

    //天和、地和Checker.
    public bool CanBlessing(List<int> hand, int[] mainHandArray)
    {
        bool canBlessing = false;
        //複数形の canBlessings は誰も槓していなくて, 全員天和, 地和できる状態かを表している.
        if (!canBlessings)
        {
            return canBlessings;
        }

        //3枚とも同じ牌が4組+残り2枚(手牌と引いてきた牌)が同じ牌であれば天和, 地和できると返す.
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

    //ツモ四槓子Checker.
    public bool CanDrawFourQuads(List<int> hand)
    {
        //4回槓していて, 手牌と引いてきた牌が同じ牌であれば四槓子できると返す.
        bool canFourQuads = false;
        if (hand.Count == 2)
        {
            canFourQuads = CheckIfSameTiles(hand[0], hand[1]);
        }
        return canFourQuads;
    }

    //ロン四槓子Checker.
    public bool CanRonFourQuads(List<int> hand, bool furitenn, int lastDiscardedTileNum)
    {
        //4回槓していて, 手牌と他のプレイヤーに捨てられた牌が同じ牌であれば四槓子できると返す.
        //ただし, 振り聴(自分がすでに捨てた牌)でロンできない.
        bool canFourQuads = false;
        if (hand.Count == 1 && !furitenn)
        {
            canFourQuads = CheckIfSameTiles(hand[0], lastDiscardedTileNum);
        }
        return canFourQuads;
    }

    //数え役満Checker.
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
        //3or4枚とも同じ牌以外の残り2枚(手牌の同じ牌がない牌と引いてきた牌)が同じ牌で13翻以上なら.
        //数え役満できると返す.
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

    //赤ドラを考慮した同じ牌かどうかの判定.
    //名前の付け方は, 
    //萬子：A 数字 (赤ドラならRがつく). 筒子：B 数字(赤ドラならRがつく).
    //索子：C 数字 (赤ドラならRがつく). 字牌：H 数字 (0から東, 南, 西, 北, 白, 發, 中).
    //最初の二文字を比較することで赤ドラ込みで同じ種類の同じ数字かどうか判定できる.
    private bool CheckIfSameTiles(int tileNum0, int tileNum1)
    {
        string pair_0 = tiles[tileNum0].name.Substring(0, 2);
        string pair_1 = tiles[tileNum1].name.Substring(0, 2);

        return pair_0 == pair_1;
    }

    //振り聴(= あたり(手牌で孤立している)牌を自分で既に河に捨てていてロンできない状態)Checker.
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
            //同じ種類の牌(赤ドラ込み)をすでに捨てていないか確認する.
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

    //暗槓Checker.
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
            //同じ牌(赤ドラ込み)を4枚持っていれば暗槓できる.
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

    //大明槓Checker.
    public bool CanMeldedKong(int wallTilesCount, int[] mainHandArray, int lastDiscardedTileNum)
    {
        //3枚持っている牌と同じ牌が他プレイヤーに捨てられたとき大明槓できる.
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
