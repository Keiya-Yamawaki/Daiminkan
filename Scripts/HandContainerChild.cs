using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class HandContainerChild : MonoBehaviourPunCallbacks
{
    //���̃X�N���v�g���A�^�b�`���ꂽ�I�u�W�F�N�g�̏��L�҂��l�b�g���[�N��ł̏��L�҂Ƃ���.
    //�����NPC�̃I�u�W�F�N�g���}�X�^�[�N���C�A���g����������ꍇ�̂��Ƃ��l���Ă���.
    public Player Owner => photonView.Owner;    
    public int order = -1;
    public int myOrder = -2;
    public int handIndex = -1;
    public int doraIndex = -1;
    public int tileNum = -1;
    public int verticalDirection = 1;   //�Ȃ����Ƃ��ɏc�ɒu��(0�Ȃ牡).
    public string tileName = null;
    GameObject basicTable;  //������.
    GameObject gameController;
    GameManager gameManager;
    UIController uiController;
    SoundManager soundManager;
    Vector3 mousePosition;
    Ray ray;
    RaycastHit hit;
    private Vector3 liftHand = Vector3.zero;
    public Vector3 initialHandPosition = Vector3.zero;
    public bool myTurnEnd;
    public bool isWaited;
    public bool canLift;
    public bool canDiscard;

    // Start is called before the first frame update
    void Start()
    {
        //order�ƈ�v���Ă��邩�ǂ����Ŏ����̔v�����f���鏈�������̌゠��̂�
        //-1�`3�ȊO�̐������g���Ă���(-1��order�̏�������).
        myOrder = -2;  
        basicTable = GameObject.FindWithTag("BasicTable");
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        uiController = gameController.GetComponent<UIController>();
        soundManager = gameController.GetComponent<SoundManager>();
        myOrder = gameManager.myOrder;
        TiltHandTile();
    }

    // Update is called once per frame
    void Update()
    {
        //��v�̔v�ɃJ�[�\�������킹��Ǝ����オ�鏈��.
        if (order == myOrder && isWaited)
        {
            LiftHandTile();
        }

        //�����̔ԂɎ�v���炢��Ȃ��v��I�����Ď̂�, ����ȊO�̔v���̂Ă��Ȃ��悤�ɂ��鏈��.
        if (order == myOrder && !myTurnEnd && canDiscard)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject && Input.GetMouseButtonDown(0))
            {
                MyTurnEnd();
                uiController.NotActiveSetUp();
                DiscardTile(myOrder);
            }
        }
        
        //3�������Ă���v�ɃJ�[�\�������킹���, �̂Ă��Ȃ��ƕ\������.
        if (order == myOrder && !myTurnEnd && !canDiscard && !canLift)
        {
            if (!uiController.cantDiscardText.activeSelf)
            {
                uiController.cantDiscardText.SetActive(true);
            }
        }
    }

    //�e�X�̎�v�͖�����ɗ��ĂĂ�����Ă���.
    //���̂܂܂ł͎����̎�v�������Ȃ��̂ŉ�ʂɑ΂��ĕ��s�ɂȂ�悤�Ɏ�v���X����.
    private void TiltHandTile()
    {
        if (order == myOrder)
        {
            switch (order)
            {
                case 0:
                    this.transform.rotation = TileController.myHandQuaternion0;
                    liftHand = TileController.liftHand0;
                    break;
                case 1:
                    this.transform.rotation = TileController.myHandQuaternion1;
                    liftHand = TileController.liftHand1;
                    break;
                case 2:
                    this.transform.rotation = TileController.myHandQuaternion2;
                    liftHand = TileController.liftHand2;
                    break;
                case 3:
                    this.transform.rotation = TileController.myHandQuaternion3;
                    liftHand = TileController.liftHand3;
                    break;
                default:
                    break;
            }
        }
    }

    //�҂��Ă���Ԃ��y���߂�悤�ɁA��v�̔v�ɃJ�[�\�������킹��Ɣv�������グ����悤�ɂ��Ă���.
    //�܂������グ���ۂɋC�����ǂ�������悤�ɂ��Ă���.
    private void LiftHandTile()
    {
        mousePosition = Input.mousePosition;
        ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == gameObject && canLift)
            {
                initialHandPosition = this.transform.position;
                this.transform.position += liftHand;
                canLift = false;
                soundManager.soundType = SoundManager.SOUND_TYPE.MYTILE;
                soundManager.PlaySoundEffect(0);
                JudgeDiscard();
            }
            //�v��������ɃJ�[�\���������Ă��Ȃ���Δv�����̍����ɂ��ǂ�.
            else if (hit.collider.gameObject != gameObject && hit.collider.gameObject != basicTable && !canLift) 
            {
                this.transform.position = initialHandPosition;
                canLift = true;
                canDiscard = false;
                uiController.cantDiscardText.SetActive(false);
            }
        }
    }


    /*
     * �����v��3�����Ă�����̂Ă��Ȃ����[���Ȃ̂�, 3�����Ă��邩���f���郁�\�b�h.
     * �܂��A�ԃh���𔻒f���邽�߂ɐ����łȂ��ʂŊ��蓖�Ă����O�Ŕ��f���Ă���.
     * (���������蓖�Ă����R�͐ԃh���ƕ��ʂ̔v����ʂ��邽��.)
     * ��F�ݎq��5���ݎq�̐ԃh����5�� A5, A5R �őO2��������v���Ă��邩�瓯���v�Ɣ��f����.
     * ���ꂼ��̖��O��,
     * �ݎq�FA ���� (�ԃh���Ȃ�Ō��R����), ���q�FB ����, ���q�FC ����, ���v�FH ����.
     * �Ƃ��Ă���.
    */
    public bool JudgeDiscard()
    {
        canDiscard = false;
        GameObject container = null;
        container = searchHandContainer(order);
        HandContainer handContainer = container.GetComponent<HandContainer>();
        string tempTileName = name.Substring(0, 2);
        int sameTileCount = handContainer.handGOList.Count(tile => tile.tileName.Substring(0, 2) == tempTileName);
        if(sameTileCount != 3)
        {
            canDiscard = true;
        }
        return canDiscard;
    }

    //��v���̂Ă���ȂǂɎ�v���̂Ă���,
    //�����グ����Ƀ\�[�g���ꂽ�Ƃ��ɏ����ʒu���ύX����Ȃ��悤�ɂ��邽�߂̃N�[���^�C��.
    public IEnumerator LiftHandCoolTime()
    {
        isWaited = false;
        canLift = true;
        canDiscard = false;
        yield return new WaitForSeconds(0.5f);
        isWaited = true;
    }

    //��v���̂Ă���ȂǂɎ�v���̂Ă��Ȃ��悤�ɂ��邽�߂̏���.
    private void MyTurnEnd()
    {
        GameObject container = null;
        container = searchHandContainer(myOrder);

        HandContainer handContainer = container.GetComponent<HandContainer>();
        foreach(var handContainerChild in handContainer.handGOList)
        {
            handContainerChild.myTurnEnd = true;
        }
    }

    private GameObject searchHandContainer(int order)
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
        return container;
    }

    public void DiscardTile(int ownerOrder)
    {
        //�v����v�Ƃ��ĔF����, �����グ���Ȃ��悤�ɏ��L�҂Ȃ��ɂ���.
        order = -1;
        canDiscard = false;
        isWaited = false;
        canLift = true;

        //�͂̐e�I�u�W�F�N�g���擾.
        GameObject container = null;
        switch (ownerOrder)
        {
            case 0:
                container = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                container = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                container = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                container = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }

        //�͂�6��ō\������, ������~���l�߂Ēu��.
        HandContainer handContainer = container.GetComponent<HandContainer>();
        int HandContainerCount_horizon = handContainer.Count % 6;
        int HandContainerCount_vertical = handContainer.Count / 6;
        //�u����ƌ����̐ݒ�.
        Vector3 dicardedPosition = Vector3.zero;
        Quaternion insideWallQuaternion = Quaternion.identity;
        switch (ownerOrder)
        {
            case 0:
                dicardedPosition = TileController.insideWallPosition0
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset0 
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset0;
                insideWallQuaternion = TileController.insideWallQuaternion0;
                break;
            case 1:
                dicardedPosition = TileController.insideWallPosition1
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset1
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset1;
                insideWallQuaternion = TileController.insideWallQuaternion1;
                break;
            case 2:
                dicardedPosition = TileController.insideWallPosition2
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset2
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset2;
                insideWallQuaternion = TileController.insideWallQuaternion2;
                break;
            case 3:
                dicardedPosition = TileController.insideWallPosition3
                    + HandContainerCount_horizon * TileController.horizonInsideWallOffset3
                    + HandContainerCount_vertical * TileController.verticalInsideWallOffset3;
                insideWallQuaternion = TileController.insideWallQuaternion3;
                break;
            default:
                break;
        }

        //�v���̂Ă�̂𓯊�.
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(Discard), RpcTarget.All, ownerOrder, handIndex, dicardedPosition, insideWallQuaternion);

        gameManager.discarded = true;
        soundManager.soundType = SoundManager.SOUND_TYPE.TILE;
        soundManager.PlaySoundEffect(2);
    }

    //[PunRPC]�͊֐��𓯊����邽�߂̂���.
    [PunRPC]
    public void Discard(int ownerOrder, int handIndex, Vector3 dicardedPosition, Quaternion insideWallQuaternion)
    {
        //��v�Ɖ͂̐e�I�u�W�F�N�g���擾��, �̂Ĕv���ړ�����, �e�I�u�W�F�N�g��ύX����.
        GameObject firstContainer = null;
        GameObject secondContainer = null;
        switch (ownerOrder)
        {
            case 0:
                firstContainer = GameObject.FindWithTag("Person0");
                secondContainer = GameObject.FindWithTag("InsideWall0");
                break;
            case 1:
                firstContainer = GameObject.FindWithTag("Person1");
                secondContainer = GameObject.FindWithTag("InsideWall1");
                break;
            case 2:
                firstContainer = GameObject.FindWithTag("Person2");
                secondContainer = GameObject.FindWithTag("InsideWall2");
                break;
            case 3:
                firstContainer = GameObject.FindWithTag("Person3");
                secondContainer = GameObject.FindWithTag("InsideWall3");
                break;
            default:
                break;
        }

        //�Ō�Ɏ̂Ă��v�̏��̍X�V(�喾�Ȃ⃍���ł��邩���莞�Ɏg��).
        HandContainer handContainer = firstContainer.GetComponent<HandContainer>();
        handContainer[handIndex].verticalDirection = 0;     //�Ȃ��ꂽ�Ƃ�, �������ɒu�����߂�0�ɂ���.
        GameObject discardedTile = handContainer[handIndex].gameObject;
        int lastDiscardedTileNum = handContainer[handIndex].tileNum;

        //�̂Ĕv���͂̎q�I�u�W�F�N�g�Ƃ��ĉ͂Ɏ̂Ă�.
        discardedTile.transform.parent = null;
        discardedTile.transform.rotation = insideWallQuaternion;
        discardedTile.transform.position = dicardedPosition;
        if (secondContainer != null)
        {
            discardedTile.transform.SetParent(secondContainer.transform);
        }

        //��v�Ɖ͂̏��̍X�V.
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        gameManager.lastDiscardedTileNum = lastDiscardedTileNum;
        List<int> ownerHand = gameManager.people[ownerOrder].hand;
        ownerHand.Remove(lastDiscardedTileNum);
        List<int> ownerInsideWall = gameManager.people[ownerOrder].insideWall;
        ownerInsideWall.Add(lastDiscardedTileNum);
        gameManager.lastDiscardedTile = discardedTile;  //�Ō�Ɏ̂Ă��v�̏��̍X�V.
        gameManager.people[gameManager.nowTurn].judged = true;  //true�ɂ��ĈÞȂ�c���{�^��������.
    }

}
