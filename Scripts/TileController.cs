using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


/*
 * ����!!!
 * �������d�v�ȃ^�[�����Q�[���Ȃ̂ŃR���[�`���œ�������������܂őҋ@����𑽁̂X�g�p���Ă���.
 * �Ⴆ��, ��v�̓����̏����ƃ\�[�g�̓����̏������قړ����Ɏn�܂��,
 * ��v�̓����ƃ\�[�g�̓��������񏈗�����, ����������񂪊Ԉ�������̂ɂȂ�.
 * ��v��1�ڂ̔v�𓯊�����ԂɃ\�[�g����, 2�ڂ̔v��1�ڂ̔v�Ɠ����v�Ȃ̂ɓ��������Ȃǂ��N����.
 * ���̂��ߓ���������̂�����ꍇ�͊��S�ɏ������I���̂��܂��Ă���, ���̏������s���K�v������.
*/

public class TileController : MonoBehaviourPunCallbacks
{
    HandContainerChild handContainerChild;
    public SoundManager soundManager;
    public GameObject[] tiles;
    public Sprite[] texturesOfDoraIndicator;    //�h���\��.
    public GameObject[] doraIndicators;         //�h���\���v.
    public bool instantiatedDora = false;
    public bool instantiatedHand = false;
    public bool sortedHand = false;
    public bool easySortedHand = false;
    public bool tiltedAllHands = false;
    public bool endKong = false;
    private bool wetherConcealedKong;
    private int kongCount = 4;
    public int copyKongCount = 0;
    public int viewId = 1000;   //�l�b�g���[�N�I�u�W�F�N�g�̌�ID. ��̈ʂŏ��L�҂𔻒f����.
    private Vector3 InstantiateHandPosition = Vector3.zero;
    private Quaternion handQuaternion = Quaternion.identity;
    private Vector3 kongPosition = Vector3.zero;
    private Vector3 kongOffset = Vector3.zero;
    private Vector3 horizonKongOffset = Vector3.zero;
    private Vector3 verticalKongOffset = Vector3.zero;
    private Quaternion kongQuaternion = Quaternion.identity;
    private Quaternion anotherKongQuaternion = Quaternion.identity;

    [Header("HandTransform")]
    public static Vector3 handPosition0 = new Vector3(-2f, 0.14f, -2.75f);
    public static Vector3 handPosition1 = new Vector3(2.75f, 0.14f, -2f);
    public static Vector3 handPosition2 = new Vector3(2f, 0.14f, 2.75f);
    public static Vector3 handPosition3 = new Vector3(-2.75f, 0.14f, 2f);
    public static Vector3 handOffset0 = new Vector3(0.215f, 0f, 0f);
    public static Vector3 handOffset1 = new Vector3(0f, 0f, 0.215f);
    public static Vector3 handOffset2 = new Vector3(-0.215f, 0f, 0f);
    public static Vector3 handOffset3 = new Vector3(0f, 0f, -0.215f);
    //�J�[�\�������킹���Ƃ��Ɏ����グ�����.
    public static Vector3 liftHand0 = new Vector3(0f, 0.05f, 0.0866f);  
    public static Vector3 liftHand1 = new Vector3(-0.0866f, 0.05f, 0f);
    public static Vector3 liftHand2 = new Vector3(0f, 0.05f, -0.0866f);
    public static Vector3 liftHand3 = new Vector3(0.0866f, 0.05f, 0f);
    public static Quaternion handQuaternion0 = Quaternion.Euler(-90f, 0f, -90f);
    public static Quaternion handQuaternion1 = Quaternion.Euler(-90f, -90f, -90f);
    public static Quaternion handQuaternion2 = Quaternion.Euler(-90f, 180f, -90f);
    public static Quaternion handQuaternion3 = Quaternion.Euler(-90f, 90f, -90f);
    //������ɐ����ɒu���ƂȂ�̔v���킩��Ȃ��̂ŉ�ʂƕ��s�ɂȂ�悤�ɌX����.
    public static Quaternion myHandQuaternion0 = Quaternion.Euler(-30f, 0f, -90f);
    public static Quaternion myHandQuaternion1 = Quaternion.Euler(-150f, 90f, -270f);
    public static Quaternion myHandQuaternion2 = Quaternion.Euler(-150f, 0f, 90f);
    public static Quaternion myHandQuaternion3 = Quaternion.Euler(-150f, -90f, 90f);

    [Header("InsideWallTransform")]
    public static Vector3 insideWallPosition0 = new Vector3(-0.6f, 0.09f, -1f);
    public static Vector3 insideWallPosition1 = new Vector3(1f, 0.09f, -0.6f);
    public static Vector3 insideWallPosition2 = new Vector3(0.6f, 0.09f, 1f);
    public static Vector3 insideWallPosition3 = new Vector3(-1f, 0.09f, 0.6f);
    public static Vector3 horizonInsideWallOffset0 = new Vector3(0.24f, 0f, 0f);
    public static Vector3 horizonInsideWallOffset1 = new Vector3(0f, 0f, 0.24f);
    public static Vector3 horizonInsideWallOffset2 = new Vector3(-0.24f, 0f, 0f);
    public static Vector3 horizonInsideWallOffset3 = new Vector3(0f, 0f, -0.24f);
    public static Vector3 verticalInsideWallOffset0 = new Vector3(0f, 0f, -0.3f);
    public static Vector3 verticalInsideWallOffset1 = new Vector3(0.3f, 0f, 0f);
    public static Vector3 verticalInsideWallOffset2 = new Vector3(0f, 0f, 0.3f);
    public static Vector3 verticalInsideWallOffset3 = new Vector3(-0.3f, 0f, 0f);
    public static Quaternion insideWallQuaternion0 = Quaternion.Euler(0f, 0f, -90f);
    public static Quaternion insideWallQuaternion1 = Quaternion.Euler(0f, -90f, -90f);
    public static Quaternion insideWallQuaternion2 = Quaternion.Euler(0f, 180f, -90f);
    public static Quaternion insideWallQuaternion3 = Quaternion.Euler(0f, 90f, -90f);

    //�ÞȂ����Ƃ�, �����̔v4����\2���Ɨ�2�����c�����ɓ|���Ă���.
    //���l�̔v�ŞȂ����ꍇ�͞Ȃ������l�̐Ȃ̈ʒu�ɍ��킹�đ��l�̔v����������,
    //�����̔v���c�����ɒu��.
    //�E���̐l�Ȃ�4���̂����ł��E, �����̐l�Ȃ�ł���, �ΖʂȂ獶����2�Ԗڂɑ��l�̔v��u��.
    //���̂��߂̃I�t�Z�b�g.
    [Header("KongTransform")]
    public static Vector3 kongPosition0 = new Vector3(2.5f, 0.09f, -2.75f);
    public static Vector3 kongPosition1 = new Vector3(2.75f, 0.09f, 2.5f);
    public static Vector3 kongPosition2 = new Vector3(-2.5f, 0.09f, 2.75f);
    public static Vector3 kongPosition3 = new Vector3(-2.75f, 0.09f, -2.5f);
    public static Vector3 basicKongOffset0 = new Vector3(-0.215f, 0f, 0f);
    public static Vector3 basicKongOffset1 = new Vector3(0f, 0f, -0.215f);
    public static Vector3 basicKongOffset2 = new Vector3(0.215f, 0f, 0f);
    public static Vector3 basicKongOffset3 = new Vector3(0f, 0f, 0.215f);

    public static Vector3 horizonKongOffset0 = new Vector3(-0.035f, 0f, 0f);
    public static Vector3 horizonKongOffset1 = new Vector3(0f, 0f, -0.035f);
    public static Vector3 horizonKongOffset2 = new Vector3(0.035f, 0f, 0f);
    public static Vector3 horizonKongOffset3 = new Vector3(0f, 0f, 0.035f);

    public static Vector3 verticalKongOffset0 = new Vector3(0, 0f, -0.035f);
    public static Vector3 verticalKongOffset1 = new Vector3(0.035f, 0f, 0);
    public static Vector3 verticalKongOffset2 = new Vector3(0f, 0f, 0.035f);
    public static Vector3 verticalKongOffset3 = new Vector3(-0.035f, 0f, 0f);

    public static Quaternion reverseKongOffset = Quaternion.Euler(0f, 180f, 0f);

    [Header("DoraTransform")]
    public static Vector3 doraPosition = new Vector3(-0.645f, 0.09f, 20f);
    public static Vector3 horizonDoraOffset = new Vector3(0.22f, 0f, 0f);
    public static Vector3 verticalDoraOffset = new Vector3(0f, 0f, -0.7f);
    public static Quaternion doraQuaternion = Quaternion.Euler(180f, 0f, -90f);

    //�a�������Ƃ��ɖ�����ɐ����ɗ��ĂĔv��|�����o�ɓ���.
    //���̎��̃I�t�Z�b�g.
    [Header("Draw&RonTransform")]
    public static Vector3 winOffset0 = new Vector3(0f, -0.035f, 0.245f);
    public static Vector3 winOffset1 = new Vector3(-0.245f, -0.035f, 0f);
    public static Vector3 winOffset2 = new Vector3(0f, -0.035f, -0.245f);
    public static Vector3 winOffset3 = new Vector3(0.245f, -0.035f, 0f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiateDora(List<int> deadWall)
    {
        //���v(����̃h���\���v)���l�b�g���[�N��ʂ��� 2*7 �Ő���.
        //��ɓ������鐶���𑽂��̔v�ł��Ă��܂��Əd���Ȃ�\��������.
        int InstantiateDoraCount = 0;
        foreach (var tileNum in deadWall)
        {
            int InstantiateDoraCount_horizon = InstantiateDoraCount % 7;
            int InstantiateDoraCount_vertical = InstantiateDoraCount / 7;
            Vector3 InstantiateDoraPosition = doraPosition
                + InstantiateDoraCount_horizon * horizonDoraOffset
                + InstantiateDoraCount_vertical * verticalDoraOffset;
            PhotonView photonView = GetComponent<PhotonView>();
            photonView.RPC(nameof(MakeDora), RpcTarget.All, InstantiateDoraCount, 
                tileNum, viewId, InstantiateDoraPosition, doraQuaternion);
            viewId++;
            InstantiateDoraCount++;
        }
        instantiatedDora = true;
    }

    public void InstantiateHand(int order, List<int> hand)
    {
        int InstantiateHandCount = 0;
        foreach (var tileNum in hand)
        {
            InstantiateHandPosition = Vector3.zero;
            handQuaternion = Quaternion.identity;
            switch (order)
            {
                case 0:
                    InstantiateHandPosition = handPosition0 + InstantiateHandCount * handOffset0;
                    handQuaternion = handQuaternion0;
                    break;
                case 1:
                    InstantiateHandPosition = handPosition1 + InstantiateHandCount * handOffset1;
                    handQuaternion = handQuaternion1;
                    break;
                case 2:
                    InstantiateHandPosition = handPosition2 + InstantiateHandCount * handOffset2;
                    handQuaternion = handQuaternion2;
                    break;
                case 3:
                    InstantiateHandPosition = handPosition3 + InstantiateHandCount * handOffset3;
                    handQuaternion = handQuaternion3;
                    break;
                default:
                    break;
            }

            //��ɓ������鐶���𑽂��̔v�ł��Ă��܂��Əd���Ȃ�\��������.
            //�܂�, �����̎�v�����₷���X���邪, �������������Ă��܂�.
            //���̂��߉��̐l�����v�������Ă��܂�.
            //�����, �����Ɣ񓯊����w��ł���悤��Photon�̃��\�b�h���g�p����, �����Ő���.

            PhotonView photonView = GetComponent<PhotonView>();
            photonView.RPC(nameof(MakeHand), RpcTarget.All, order, tileNum, viewId, InstantiateHandPosition, handQuaternion);
            viewId++;

            InstantiateHandCount++;
        }
        instantiatedHand = true;
    }

    public IEnumerator Draw(int order, int tileNum)
    {
        InstantiateHandPosition = Vector3.zero;
        handQuaternion = Quaternion.identity;
        GameObject container = null;
        HandContainer handContainer = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition0 + (handContainer.Count + 0.5f) * handOffset0;
                handQuaternion = handQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition1 + (handContainer.Count + 0.5f) * handOffset1;
                handQuaternion = handQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition2 + (handContainer.Count + 0.5f) * handOffset2;
                handQuaternion = handQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                InstantiateHandPosition = handPosition3 + (handContainer.Count + 0.5f) * handOffset3;
                handQuaternion = handQuaternion3;
                break;
            default:
                break;
        }

        /*
         * �v���������Ƃ��Ƀ\�[�g�����.
         * ���̎��Ɏ����グ���v�̍��������Ƃɖ߂��Ă����Ȃ���,
         * �\�[�g���ꂽ�Ƃ��ɍ��������ꂽ��Ԃ������ʒu�ƂȂ�, 
         * �ēx�����グ����悤�ɂȂ��Ă��܂�.
         * �����Ă��炵�΂炭�����グ���Ȃ��悤�ɃN�[���^�C���𓱓�����.
        */
        bool startedCoolTime = false;
        for(int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            handContainer[i].myTurnEnd = false;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(MakeHand), RpcTarget.All, order, tileNum, viewId, InstantiateHandPosition, handQuaternion);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(0);
        photonView.RPC(nameof(AddHand), RpcTarget.All, order, tileNum);
        viewId++;
        easySortedHand = false;
        photonView.RPC(nameof(EasySortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => easySortedHand);
        handContainer[handContainer.Count - 1].myTurnEnd = false;

        instantiatedHand = true;
    }

    //�����v��4�������Ă���Ƃ��ɍs����Þ�.
    //���h�����ǂ��Ȃ�悤��, �ԃh��������ΉE����2�Ԗڂɗ���悤�ɂ��Ă���.
    //�܂��V�����Ȃ���Ƃ��ɍŌ�ɞȂ����v�̌����ɂ���Ĕv�̒��S�̍��W��
    //��������K�v������̂œK�X�v�Z����(�O��̔v�̒��S���W����ɔv��u������).
    public IEnumerator ConcealedKong(int order, int tileNum)
    {
        kongPosition = Vector3.zero;
        kongQuaternion = Quaternion.identity;
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong0");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if(handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild 
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if(handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position 
                            + basicKongOffset0;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position 
                            + basicKongOffset0 + horizonKongOffset0 - verticalKongOffset0;
                    }
                }
                kongOffset = basicKongOffset0;
                kongQuaternion = insideWallQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong1");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1 + horizonKongOffset1 - verticalKongOffset1;
                    }
                }
                kongOffset = basicKongOffset1;
                kongQuaternion = insideWallQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong2");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2 + horizonKongOffset2 - verticalKongOffset2;
                    }
                }
                kongOffset = basicKongOffset2;
                kongQuaternion = insideWallQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong3");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3;
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3 + horizonKongOffset3 - verticalKongOffset3;
                    }
                }
                kongOffset = basicKongOffset3;
                kongQuaternion = insideWallQuaternion3;
                break;
            default:
                break;
        }

        /*
        * �v���������Ƃ����l�Ȃ����Ƃ��������グ���v�̍��������Ƃɖ߂��Ă���,
        * ���΂炭�����グ���Ȃ��悤�N�[���^�C���𓱓�����.
        */
        bool startedCoolTime = false;
        for (int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        copyKongCount = 0;
        endKong = false;
        wetherConcealedKong = true;
        Checker checker = GetComponent<Checker>();
        //���h���̂��ߐԃh��������ΉE����2�Ԗڂɗ���悤�ɔz�u����.
        //�܂��Ȃ����v�̂����[�̔v�͗������ɂ���.
        for (int i = 0; i < kongCount; i++)
        {
            yield return new WaitUntil(() => (i == copyKongCount));
            if (i == 1 && checker.blackTileNumsHave5.Contains(tileNum))
            {
                int redDoraTileNum = tileNum - 5;
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
            }
            else if(i == 0 || i == 3)
            {
                Quaternion reverseKongQuaternion = kongQuaternion * reverseKongOffset;
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    tileNum, i, kongPosition, reverseKongQuaternion, wetherConcealedKong);
            }
            else
            {
                photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order, 
                    tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
            }
            kongPosition += kongOffset;
            yield return new WaitUntil(() => (i == copyKongCount - 1));
            if (i == kongCount - 1)
            {
                endKong = true;
            }
        }
        yield return new WaitUntil(() => endKong);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(1);
        yield return new WaitForSeconds(0.75f);
        sortedHand = false;
        photonView.RPC(nameof(SortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => sortedHand);
        soundManager.PlaySoundEffect(1);
    }

    //3�������Ă���v���ق��̃v���C���[�Ɏ̂Ă�ꂽ�Ƃ��ɍs����喾��.
    //���h�����ǂ��Ȃ�悤��, �ԃh��������ΉE����2�Ԗڂɗ���悤�ɂ��Ă���.
    //�܂��V�����Ȃ���Ƃ��ɍŌ�ɞȂ����v�̌����ɂ���Ĕv�̒��S�̍��W��
    //��������K�v������̂œK�X�v�Z����.
    public IEnumerator MeldedKong(int order, int nowTurn, int tileNum)
    {
        kongPosition = Vector3.zero;
        kongQuaternion = Quaternion.identity;
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        int differenceInOrder = (nowTurn - order + 4) % 4;
        //�Ȃ����l�Ǝ����̏��Ԃ̍�.
        //�܂�ǂ�����Ȃ�������\��.
        //�E���Ȃ�1, �ΖʂȂ�2, �����Ȃ�3.
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong0");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if(differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset0;
                    }
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset0;
                        if(differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset0 + verticalKongOffset0;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset0 + horizonKongOffset0 - verticalKongOffset0;
                        if(differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset0 + verticalKongOffset0;
                        }
                    }
                }
                kongOffset = basicKongOffset0;
                horizonKongOffset = horizonKongOffset0;
                verticalKongOffset = verticalKongOffset0;
                kongQuaternion = insideWallQuaternion0;
                anotherKongQuaternion = insideWallQuaternion1;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong1");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset1;
                    }
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset1 + verticalKongOffset1;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset1 + horizonKongOffset1 - verticalKongOffset1;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset1 + verticalKongOffset1;
                        }
                    }
                }
                kongOffset = basicKongOffset1;
                horizonKongOffset = horizonKongOffset1;
                verticalKongOffset = verticalKongOffset1;
                kongQuaternion = insideWallQuaternion1;
                anotherKongQuaternion = insideWallQuaternion2;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong2");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset2;
                    }
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset2 + verticalKongOffset2;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset2 + horizonKongOffset2 - verticalKongOffset2;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset2 + verticalKongOffset2;
                        }
                    }
                }
                kongOffset = basicKongOffset2;
                horizonKongOffset = horizonKongOffset2;
                verticalKongOffset = verticalKongOffset2;
                kongQuaternion = insideWallQuaternion2;
                anotherKongQuaternion = insideWallQuaternion3;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                handContainer = container.GetComponent<HandContainer>();
                kongContainer = GameObject.FindWithTag("Kong3");
                handContainer_Kong = kongContainer.GetComponent<HandContainer>();
                if (handContainer_Kong.Count == 0)
                {
                    kongPosition = kongContainer.transform.position;
                    if (differenceInOrder == 1)
                    {
                        kongPosition += verticalKongOffset3;
                    }
                }
                else
                {
                    //�Ō�ɞȂ����v�̌����ƍ�������ɒ������Ă���.
                    HandContainerChild handContainerChild
                        = handContainer_Kong[handContainer_Kong.Count - 1].GetComponent<HandContainerChild>();
                    if (handContainerChild.verticalDirection == 1)
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset3 + verticalKongOffset3;
                        }
                    }
                    else
                    {
                        kongPosition = handContainerChild.transform.position
                            + basicKongOffset3 + horizonKongOffset3 - verticalKongOffset3;
                        if (differenceInOrder == 1)
                        {
                            kongPosition += horizonKongOffset3 + verticalKongOffset3;
                        }
                    }
                }
                kongOffset = basicKongOffset3;
                horizonKongOffset = horizonKongOffset3;
                verticalKongOffset = verticalKongOffset3;
                kongQuaternion = insideWallQuaternion3;
                anotherKongQuaternion = insideWallQuaternion0;
                break;
            default:
                break;
        }

        /*
        * �v���������Ƃ����l�Ȃ����Ƃ��������グ���v�̍��������Ƃɖ߂��Ă���,
        * ���΂炭�����グ���Ȃ��悤�N�[���^�C���𓱓�����.
        */
        bool startedCoolTime = false;
        for (int i = 0; i < handContainer.Count; i++)
        {
            StartCoroutine(handContainer[i].LiftHandCoolTime());
            handContainer[i].transform.position = handContainer[i].initialHandPosition;
            if (i == handContainer.Count - 1)
            {
                startedCoolTime = true;
            }
        }
        yield return new WaitUntil(() => startedCoolTime);
        PhotonView photonView = GetComponent<PhotonView>();
        copyKongCount = 0;
        endKong = false;
        wetherConcealedKong = false;
        Checker checker = GetComponent<Checker>();

        //���h���̂��ߐԃh��������ΉE����2�Ԗڂɗ���悤�ɔz�u����.
        //�喾�Ȃ������l�̐Ȃ̈ʒu�ɍ��킹�đ��l�̔v����������,�����̔v���c�����ɒu��.
        //�E���̐l�Ȃ�4���̂����ł��E, �����̐l�Ȃ�ł���,
        //�ΖʂȂ獶����2�Ԗڂɑ��l�̔v��u��.
        for (int i = 0; i < kongCount; i++)
        {
            yield return new WaitUntil(() => (i == copyKongCount));

            switch (i)
            {
                case 0:
                    //�E���̐l����Ȃ������ǂ���.
                    if(differenceInOrder == 1)
                    {
                        if (handContainer_Kong.Count == 0)
                        {
                            kongPosition -= verticalKongOffset;
                        }
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    else
                    {
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 1:
                    //�E���̐l����Ȃ������ǂ���.
                    if (differenceInOrder == 1)
                    {
                        kongPosition += kongOffset + horizonKongOffset - verticalKongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else if (checker.blackTileNumsHave5.Contains(tileNum))
                        {
                            int redDoraTileNum = tileNum - 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    else
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else if (checker.blackTileNumsHave5.Contains(tileNum))
                        {
                            int redDoraTileNum = tileNum - 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                redDoraTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 2:
                    //�Ζʂ̐l����Ȃ������ǂ���.
                    if (differenceInOrder == 2)
                    {
                        kongPosition += kongOffset + horizonKongOffset + verticalKongOffset;
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    else
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                case 3:
                    //�����̐l����Ȃ������ǂ���.
                    if (differenceInOrder == 3)
                    {
                        kongPosition += kongOffset + horizonKongOffset + verticalKongOffset;
                        photonView.RPC(nameof(MoveDiscardedTile), RpcTarget.All, order, nowTurn,
                            tileNum, i, kongPosition, anotherKongQuaternion);
                    }
                    //�E���̐l����Ȃ������ǂ���.
                    else if (differenceInOrder == 1)
                    {
                        kongPosition += kongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    //�Ζʂ̐l����Ȃ������ǂ���.
                    else if (differenceInOrder == 2)
                    {
                        kongPosition += kongOffset + horizonKongOffset - verticalKongOffset;
                        if (checker.redDoraTileNums.Contains(tileNum))
                        {
                            int blackTileNum = tileNum + 5;
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                blackTileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                        else
                        {
                            photonView.RPC(nameof(MoveToKongTile), RpcTarget.All, order,
                                tileNum, i, kongPosition, kongQuaternion, wetherConcealedKong);
                        }
                    }
                    break;
                default:
                    break;
            }

            yield return new WaitUntil(() => (i == copyKongCount - 1));
            if (i == kongCount - 1)
            {
                endKong = true;
            }
        }
        yield return new WaitUntil(() => endKong);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(1);
        yield return new WaitForSeconds(0.75f);
        sortedHand = false;
        photonView.RPC(nameof(SortHand), RpcTarget.All, order);
        yield return new WaitUntil(() => sortedHand);
        soundManager.PlaySoundEffect(1);
    }

    //�a���������ɔv�𖃐���ɐ����ɗ���, �|�����o.
    public IEnumerator WinMove(int myOrder, int winnerOrder)
    {
        GameObject container = null;
        Vector3 tiltPosition = Vector3.zero;
        Quaternion handQuaternion = Quaternion.identity; ;
        Quaternion tiltQuaternion = Quaternion.identity; ;
        switch (winnerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                tiltPosition = winOffset0;
                handQuaternion = handQuaternion0;
                tiltQuaternion = insideWallQuaternion0;
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                tiltPosition = winOffset1;
                handQuaternion = handQuaternion1;
                tiltQuaternion = insideWallQuaternion1;
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                tiltPosition = winOffset2;
                handQuaternion = handQuaternion2;
                tiltQuaternion = insideWallQuaternion2;
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                tiltPosition = winOffset3;
                handQuaternion = handQuaternion3;
                tiltQuaternion = insideWallQuaternion3;
                break;
            default:
                break;
        }

        HandContainer handContainer = container.GetComponent<HandContainer>();
        if (myOrder == winnerOrder)
        {
            foreach (HandContainerChild handContainerChild in handContainer)
            {
                handContainerChild.order = -1;
                handContainerChild.transform.position = handContainerChild.initialHandPosition;
                handContainerChild.transform.rotation = handQuaternion;
            }
            easySortedHand = false;
            photonView.RPC(nameof(EasySortHand), RpcTarget.All, winnerOrder);
            yield return new WaitUntil(() => easySortedHand);
        }
        yield return new WaitForSeconds(0.5f);
        tiltedAllHands = false;
        photonView.RPC(nameof(TiltAllHands), RpcTarget.All, winnerOrder, 
            tiltPosition, tiltQuaternion);
        yield return new WaitUntil(() => tiltedAllHands);
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(2);

        easySortedHand = false;
        photonView.RPC(nameof(EasySortHand), RpcTarget.All, winnerOrder);
        yield return new WaitUntil(() => easySortedHand);
    }

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    [PunRPC]
    private void MakeDora(int doraIndex, int tileNum, int viewId, Vector3 InstantiateDoraPosition, Quaternion doraQuaternion)
    {
        handContainerChild = tiles[tileNum].GetComponent<HandContainerChild>();
        handContainerChild.order = -1;
        handContainerChild.doraIndex = doraIndex;
        handContainerChild.tileNum = tileNum;

        //�������O���������ł���悤��, 
        //�l�b�g���[�N�I�u�W�F�N�g��Ǝ��ɐ�����, �l�b�g���[�NID��Ǝ��ɐݒ肷��.
        GameObject doraTile = Instantiate(tiles[tileNum],
            InstantiateDoraPosition, doraQuaternion);
        PhotonView photonView = doraTile.GetComponent<PhotonView>();
        photonView.ViewID = viewId;

        //�ꊇ�Ǘ��ł���悤�ɐe�I�u�W�F�N�g���w�肷��.
        GameObject container = GameObject.FindWithTag("DoraContainer");
        if (container != null)
        {
            doraTile.transform.SetParent(container.transform);
        }
    }

    [PunRPC]
    public void MakeHand(int order, int tileNum, int viewId, Vector3 InstantiateHandPosition, Quaternion handQuaternion)
    {
        handContainerChild = tiles[tileNum].GetComponent<HandContainerChild>();
        handContainerChild.order = order;
        handContainerChild.tileNum = tileNum;
        handContainerChild.verticalDirection = 1;
        handContainerChild.tileName = tiles[tileNum].name;
        handContainerChild.myTurnEnd = true;
        handContainerChild.isWaited = false;
        handContainerChild.initialHandPosition = InstantiateHandPosition;
        handContainerChild.canLift = true;
        handContainerChild.canDiscard = false;

        //�������O���������ł���悤��, 
        //�l�b�g���[�N�I�u�W�F�N�g��Ǝ��ɐ�����, �l�b�g���[�NID��Ǝ��ɐݒ肷��.
        GameObject handTile = Instantiate(tiles[tileNum],
            InstantiateHandPosition, handQuaternion);
        PhotonView photonView = handTile.GetComponent<PhotonView>();
        photonView.ViewID = viewId;

        //�ꊇ�Ǘ��ł���悤�ɐe�I�u�W�F�N�g���w�肷��.
        GameObject container = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                break;
            default:
                break;
        }
        if (container != null)
        {
            handTile.transform.SetParent(container.transform);
        }
    }

    [PunRPC]
    public void AddHand(int order, int tileNum)
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        gameManager.people[order].hand.Add(tileNum);
    }

    //�ʏ�̃\�[�g.
    //�v�������Ă����Ƃ���v���̂Ă��Ƃ�, �Ȃ����Ƃ��Ƀ\�[�g���s��.
    //�v�����Ԃɕ��ёւ�����悤�Ɉʒu��ύX����.
    [PunRPC]
    public void SortHand(int order)
    {
        GameObject container = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                break;
            default:
                break;
        }


        //�v�̖��O�Ń\�[�g����.
        //�Ƃ����̂��ݎq��5�̐ԃh�����ݎq��5�͂��ꂼ�ꐔ���ł� 0, 5 �Ƌ�ʂ��Ă��邩��.
        //�ݎq�FA ���� (�ԃh���Ȃ�R���t��), ���q�FB ���� (��), ���q�FC ���� (��), ���v�FH ����.
        //�ԍ��Ń\�[�g�����2�������v�ƔF�����ꂸ����Ď�v�ɒu����Ă��܂�.

        //(�ȉ��⑫�F���Ƃ����Ė��O�Ŕ��f����ƃv���O�����I�ɏ������ɂ���, �ł��邾���ԍ��ň�����������.
        //���ʔԍ��Ɩ��O�������蓖�Ă�).
        HandContainer handContainer = container.GetComponent<HandContainer>();
        List<HandContainerChild> handGOList = handContainer.handGOList;
        handGOList = handGOList.OrderBy(child => child.name).ToList();
        handContainer.handGOList = handGOList;
        Vector3 NewLocalPosition = Vector3.zero;

        for (int i = 0; i < handGOList.Count; i++)
        {
            handContainerChild = handGOList[i].GetComponent<HandContainerChild>();
            handContainerChild.handIndex = i;
            StartCoroutine(handContainerChild.LiftHandCoolTime());
            Transform child = handGOList[i].transform;
            switch (order)
            {
                case 0:
                    NewLocalPosition = i * handOffset0;
                    break;
                case 1:
                    NewLocalPosition = i * handOffset1;
                    break;
                case 2:
                    NewLocalPosition = i * handOffset2;
                    break;
                case 3:
                    NewLocalPosition = i * handOffset3;
                    break;
                default:
                    break;
            }
            child.localPosition = NewLocalPosition;
            handContainerChild.initialHandPosition = child.position;
        }
        sortedHand = true;
    }

    //������̃\�[�g�̓\�[�g�Ƃ������̓\�[�g���ꂽ�v�̓����ԍ���, 
    //�v�������Ă����Ƃ��ɂ���Ȃ��悤�ɂ��邽�߂̂���.
    //��v�������Ă����Ƃ���̂Ă��Ƃ���,
    //�����Őe�I�u�W�F�N�g�ɓ����ԍ������蓖�Ă���悤�ɂ��Ă���.
    //���̍ۂɈ��������ɍX�V����Ă��܂�.
    //������\�[�g����Ă����܂܂̏��ɖ߂����߂̃\�[�g.
    //�����ԍ���ύX���邾���Ŕv���m�̈ʒu�͕ύX���Ȃ�.
    [PunRPC]
    public void EasySortHand(int order)
    {
        GameObject container = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                break;
            default:
                break;
        }

        HandContainer handContainer = container.GetComponent<HandContainer>();
        List<HandContainerChild> handGOList = handContainer.handGOList;
        HandContainerChild lastHandContainerChild = handGOList[handContainer.Count - 1];
        lastHandContainerChild.handIndex = handContainer.Count - 1;
        handGOList.RemoveAt(handContainer.Count - 1);
        handGOList = handGOList.OrderBy(child => child.name).ToList();
        handGOList.Add(lastHandContainerChild);
        handContainer.handGOList = handGOList;
        StartCoroutine(lastHandContainerChild.LiftHandCoolTime());

        easySortedHand = true;
    }

    //��v����Ȃ����l�̞Ȃ����v��u���ꏊ�Ɉړ������郁�\�b�h.
    //���̎��ꊇ�Ǘ��ł���悤�ɐe�I�u�W�F�N�g�̕ύX��,
    //��v�ƔF�����Ď����グ���Ȃ��悤�ɏ��L�҂Ȃ��ɂ���.
    [PunRPC]
    private void MoveToKongTile(int order, int tileNum, int kongCount,
    Vector3 kongPosition, Quaternion kongQuaternion, bool wetherConcealedKong)
    {
        GameObject container = null;
        GameObject kongContainer = null;
        HandContainer handContainer = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                kongContainer = GameObject.FindWithTag("Kong0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                kongContainer = GameObject.FindWithTag("Kong1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                kongContainer = GameObject.FindWithTag("Kong2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                kongContainer = GameObject.FindWithTag("Kong3");
                break;
            default:
                break;
        }
        handContainer = container.GetComponent<HandContainer>();
        handContainer_Kong = kongContainer.GetComponent<HandContainer>();
        foreach (HandContainerChild handContainerChild in handContainer)
        {
            if (handContainerChild.tileNum == tileNum)
            {
                handContainerChild.transform.parent = null;
                handContainerChild.order = -1;  //�v����v�Ƃ��ĔF����, �����グ���Ȃ��悤�ɏ��L�҂Ȃ��ɂ���.
                handContainerChild.transform.rotation = kongQuaternion;
                handContainerChild.transform.position = kongPosition;
                if (kongContainer != null)
                {
                    handContainerChild.transform.SetParent(kongContainer.transform);
                }
                break;
            }
        }
        GameObject gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        List<int> ownerHand = gameManager.people[order].hand;
        ownerHand.Remove(tileNum);
        List<int> ownerKongTiles = gameManager.people[order].kongTiles;
        ownerKongTiles.Add(tileNum);

        //4���Ş�1��ł���̂ōŌ�̔v�ŞȂ����񐔂��C���N�������g.
        if (kongCount == 3)
        {
            gameManager.people[order].kongCount++;
            if (wetherConcealedKong)
            {
                gameManager.people[order].concealedKongCount++;
            }
            else
            {
                gameManager.people[order].meldedKongCount++;
            }
        }
        TileController tileController = gameController.GetComponent<TileController>();
        tileController.copyKongCount++;
    }

    //�͂���Ȃ����l�̞Ȃ����v��u���ꏊ�Ɉړ������郁�\�b�h.
    //���̎��ꊇ�Ǘ��ł���悤�ɐe�I�u�W�F�N�g�̕ύX��,
    //��v�ƔF�����Ď����グ���Ȃ��悤�ɏ��L�҂Ȃ��ɂ���.
    [PunRPC]
    private void MoveDiscardedTile(int order, int nowTurn, int tileNum, int kongCount,
        Vector3 kongPosition, Quaternion kongQuaternion)
    {
        GameObject insideWallContainer = null;
        GameObject kongContainer = null;
        HandContainer handContainer_InsideWall = null;
        HandContainer handContainer_Kong = null;
        switch (order)
        {
            case 0:
                kongContainer = GameObject.FindWithTag("Kong0");
                break;
            case 1:
                kongContainer = GameObject.FindWithTag("Kong1");
                break;
            case 2:
                kongContainer = GameObject.FindWithTag("Kong2");
                break;
            case 3:
                kongContainer = GameObject.FindWithTag("Kong3");
                break;
            default:
                break;
        }
        handContainer_Kong = kongContainer.GetComponent<HandContainer>();
        switch (nowTurn)
        {
            case 0:
                insideWallContainer = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                insideWallContainer = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                insideWallContainer = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                insideWallContainer = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }
        handContainer_InsideWall = insideWallContainer.GetComponent<HandContainer>();
        HandContainerChild handContainerChild 
            = handContainer_InsideWall[handContainer_InsideWall.Count - 1];
        handContainerChild.transform.parent = null;
        handContainerChild.order = -1;  //�v����v�Ƃ��ĔF����, �����グ���Ȃ��悤�ɏ��L�҂Ȃ��ɂ���.
        handContainerChild.transform.rotation = kongQuaternion;
        handContainerChild.transform.position = kongPosition;
        if (kongContainer != null)
        {
            handContainerChild.transform.SetParent(kongContainer.transform);
        }

        GameObject gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        List<int> ownerHand = gameManager.people[order].hand;
        ownerHand.Remove(tileNum);
        List<int> ownerKongTiles = gameManager.people[order].kongTiles;
        ownerKongTiles.Add(tileNum);

        //4���Ş�1��ł���̂ōŌ�̔v�ŞȂ����񐔂��C���N�������g.
        if (kongCount == 3)
        {
            gameManager.people[order].kongCount++;
            gameManager.people[order].meldedKongCount++;
        }
        TileController tileController = gameController.GetComponent<TileController>();
        tileController.copyKongCount++;
    }

    //�a���������ɔv��|�����\�b�h.
    [PunRPC]
    private void TiltAllHands(int winnerOrder, Vector3 tiltPosition, Quaternion tiltQuaternion)
    {
        GameObject container = null;
        switch (winnerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("Person0");
                break;
            case 1:
                container = GameObject.FindWithTag("Person1");
                break;
            case 2:
                container = GameObject.FindWithTag("Person2");
                break;
            case 3:
                container = GameObject.FindWithTag("Person3");
                break;
            default:
                break;
        }
        HandContainer handContainer = container.GetComponent<HandContainer>();
        foreach (HandContainerChild handContainerChild in handContainer)
        {
            handContainerChild.transform.localPosition += tiltPosition;
            handContainerChild.transform.rotation = tiltQuaternion;
        }
        tiltedAllHands = true;
    }
}
