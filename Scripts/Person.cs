using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Photon�l�b�g���[�N���ID�ł�NPC�������Ȃ�.
//������, ������NPC�p���܂߂ď��Ƀv���C���Ă�������, NPC��������悤�ɂ�����.
//������, NPC�ƃv���C���[�������̃N���X�Őݒ肷��.
public class Person : MonoBehaviour
{
    public string Name;
    public bool isHuman;  
    public int Id;   
    public int order;    //�v���C����.
    public int type;    //�ŏ��̔v�̎��.
    public int[] mainHandArray;  //��v��4�����̔v��3�����z����. ����4�̐���.
    public List<int> hand;  //��v.
    public List<int> kongTiles = new List<int>(); //�Ȃ���(����)�v.
    public List<int> insideWall = new List<int>();    //��.
    public bool didConcealedKong = false;
    public bool furitenn = false;   //�U�蒮�F�͂Ɏ̂Ă��v�Ń����ł��Ȃ�.
    public bool judged = false;     //�����ς݂��ǂ���.
    public bool ron = false;        //�l���̂Ă��v�Řa���������ǂ���.
    public bool draw = false;       //�������v�Řa���������ǂ���.
    public bool didMeldedKong = false;
    public bool countedDora = false;    //�h���F�_���オ��{�[�i�X�v.
    public int kingsTileDraw = 0;   //���J��(�Ȃ��Ĉ������Ƃ��ɏオ���ƕt����).
    public int finalTileWin = 0;    //�C�ꝝ��(�v�R�Ō�̔v�ŏオ�������ɂ���).
    public int doraCount = 0;   //��v�̃h���̐�.
    public int kongCount = 0;   //�Ȃ�����.
    public int meldedKongCount = 0; //�喾�Ȃ�����.
    public int concealedKongCount = 0;  //�ÞȂ�����.

    //NPC�ƃv���C���[�����ȉ��̊֐��Őݒ肷��.
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
