using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DoraFripper : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] AudioClip fripTileSE;
    [SerializeField] AudioClip dropTileSE;

    //order����v���邩�ǂ����Ō���邩�ς��.���̂��߂ɏ��߂͈�v���Ȃ��悤��-2�Ƃ��Ă���.
    public int orderOfFripper = -1;
    public int myOrder = -2;    
    public int doraIndex;
    public int tileNum;
    public bool isHuman = false;
    public HandContainerChild handContainerChild;
    public GameObject gameController;
    public GameManager gameManager;
    public UIController uiController;
    public bool onText = true;

    private float tileLength = 0.28f;   //�v�̏c��.
    //private float tileWidth = 0.21f;   //�v�̉���.
    //private float tileHeight = 0.165f;   //�v�̌���.
    private float tileRatio = 1000.0f;   //�v���X�N���[���Ō����Ƃ��̑傫���ɍ��킹�邽�߂̔{��.
    private float screenWidthPercentage = 1f / 3f;   //�X�N���[�����ɑ΂��銄��.
    private float screenHeightPercentage = 4f / 5f;  //�X�N���[�������ɑ΂��銄��.

    public bool isScrolled = true;
    public bool isFinished = false;    //�v���߂���I�������ǂ���.
    public bool inside = false;      //�}�E�X�J�[�\�����X�N���[���̎w��͈͓��ɂ��邩�ǂ���.

    private float flightTime = 2 * 1.0f / 5.0f;
    private float rotationDegrees = 180f;    //��]������p�x.
    private float liftHeight = 9.8f / (2.0f * 5.0f * 5.0f);     //�㏸�����鍂��.

    private float timer = -1.0f;
    private Vector3 lastMousePosition;  //���O�̃}�E�X�̈ʒu.

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindWithTag("GameController");
        gameManager = gameController.GetComponent<GameManager>();
        uiController = gameController.GetComponent<UIController>();
        handContainerChild = GetComponent<HandContainerChild>();
        audioSource = GetComponent<AudioSource>();
        myOrder = gameManager.myOrder;
        doraIndex = handContainerChild.doraIndex;
        tileNum = handContainerChild.tileNum;
    }

    // Update is called once per frame
    void Update()
    {
        //�ق��v���C���[�̃h�������҂e�L�X�g�̕\��.
        if (!isScrolled && !onText && (orderOfFripper != myOrder))
        {
            uiController.waitText.SetActive(true);
            onText = true;
        }

        //CPU���Ȃ����ꍇ�͎����Ńh�����߂���.
        if (!isScrolled & !isHuman)
        {
            isScrolled = true;
            StartCoroutine(CPUDoraFrip());
        }

        //�������߂���Ƃ�.
        if (!isScrolled && isHuman && (orderOfFripper == myOrder))
        {
            if (!onText)
            {
                uiController.fingerImage.SetActive(true);
                uiController.swipeText.SetActive(true);
                onText = true;
            }

            //�͈͓�����o���Ƀh���b�O���ꂽ�v���߂���.
            Detection();
            if (inside && Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            if (!isScrolled && lastMousePosition.y > 0 && Input.GetMouseButton(0))
            {
                //�������ɔv�����ȏ�ړ����Ă���Δv����]������.
                Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
                if (deltaMousePosition.y < - tileLength * tileRatio / 2) 
                {
                    isScrolled = true;
                    uiController.fingerImage.SetActive(false);   //�A�j���[�V�����̕ύX.
                    uiController.swipeText.SetActive(false);
                    //�v�㏸���x��S�v���C���[��ʂŗ^��, �Ђ�����Ԃ�.
                    Vector3 velocity = new Vector3(0, Mathf.Sqrt(2.0f * 9.8f * liftHeight), 0); 
                    PhotonView photonView = GetComponent<PhotonView>();
                    photonView.RPC(nameof(ReverseDora), RpcTarget.All, doraIndex, velocity);
                }
            }
        }

        if (timer >= 0)
        {
            if(timer == 0)
            {
                audioSource.PlayOneShot(fripTileSE);
            }
            this.timer += Time.deltaTime;
            float rate = timer / flightTime;
            if (rate >= 1)
            {
                rate = 1;
                timer = -1.0f;
            }
            //�v�𔼉�]������.
            float angle = Mathf.Lerp(0f, rotationDegrees, rate);
            this.transform.rotation = Quaternion.Euler(angle, 180.0f, 90.0f);
            if (rate == 1)
            {
                audioSource.PlayOneShot(dropTileSE);
                Vector3 velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().velocity = velocity;
                this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                isFinished = true;
            }
        }

        //�v���߂���I������e�L�X�g������, �h����ǉ���, ���Ƃ̃v���C��ʂɖ߂�.
        if (isFinished)
        {
            isFinished = false;
            uiController.waitText.SetActive(false);
            uiController.fingerImage.SetActive(false);
            uiController.swipeText.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(NextReady());
            }
        }
    }

    //�}�E�X�J�[�\�����X�N���[���̎w��͈͓��ɂ��邩�m�F���郁�\�b�h.
    public void Detection()
    {
        //�X�N���[���̐^��1/3�A��2/3�͈̔�.
        Rect detectionArea = new Rect(Screen.width * (1 - screenWidthPercentage) / 2, 0, Screen.width * screenWidthPercentage, Screen.height * screenHeightPercentage);

        if (detectionArea.Contains(Input.mousePosition))
        {
            inside = true;
        }
        else
        {
            inside = false;
            lastMousePosition.y = 0;
            //���N���b�N�����܂܎w��͈͊O�ɏo���, ��ʏ�̕��܂ň��������ĉ��낵�����ɔv���߂����.
            //���̎��v�̏���Ȃ����Ă��銴���łȂ��Ȃ邽��, ������Ȃ����l����������.
        }
    }

    IEnumerator CPUDoraFrip()
    {
        yield return new WaitForSeconds(0.5f);  //���̎��Ԃ��Ɛl���߂����Ă���悤�ȃ^�C�~���O�Ɍ�����.
        Vector3 velocity = new Vector3(0, Mathf.Sqrt(2.0f * 9.8f * liftHeight), 0);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(ReverseDora), RpcTarget.All, doraIndex, velocity);
    }

    //�h����ǉ���, ���Ƃ̃v���C��ʂɖ߂�.
    IEnumerator NextReady()
    {
        yield return new WaitForSeconds(1f);
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(AddDora), RpcTarget.All);
    }

    public void InitialSetUpOfDora(int doraIndex, int orderOfFripper, bool isHuman)
    {
        PhotonView photonView = GetComponent<PhotonView>();
        photonView.RPC(nameof(InitialSetUpToFrip), RpcTarget.All, doraIndex, orderOfFripper, isHuman);
    }

    //[PunRPC]�͊֐��𓯊��ł���l�ɂ��邽�߂̂���.
    //�I�u�W�F�N�g����ɓ������Ă���킯�ł͂Ȃ��̂�,
    //����X�N���v�g���擾���Ă��炻�̃X�N���v�g�̃��\�b�h�����s����.

    //�h���v���߂����悤��Y���W��X���̌Œ���O������, ����l�̏���(order)���w�肷��.
    [PunRPC]
    public void InitialSetUpToFrip(int doraIndex, int orderOfFripper, bool isHuman)
    {
        GameObject container = null;
        HandContainer doraContainer = null;
        DoraFripper doraFripper = null;
        container = GameObject.FindWithTag("DoraContainer");
        doraContainer = container.GetComponent<HandContainer>();
        doraFripper = doraContainer[doraIndex].GetComponent<DoraFripper>();
        Rigidbody rb = doraFripper.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
            | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ;
        doraFripper.orderOfFripper = orderOfFripper;
        doraFripper.onText = false;
        doraFripper.isHuman = isHuman;
        doraFripper.isScrolled = false;
    }

    [PunRPC]
    public void ReverseDora(int doraIndex, Vector3 velocity)
    {
        GameObject container = null;
        HandContainer doraContainer = null;
        DoraFripper doraFripper = null;
        container = GameObject.FindWithTag("DoraContainer");
        doraContainer = container.GetComponent<HandContainer>();
        doraFripper = doraContainer[doraIndex].GetComponent<DoraFripper>();
        doraFripper.isScrolled = true;
        doraFripper.orderOfFripper = -1;
        doraFripper.isHuman = false;
        doraFripper.GetComponent<Rigidbody>().velocity = velocity;
        doraFripper.timer = 0.0f;  //�^�C�}�[�X�^�[�g.
        //doraFripper.GetComponent<Rigidbody>().velocity = velocity;
    }

    //���̎�ނ̔v���h���Ƃ��ĉ���, ���̃v���C��ʂɖ߂�悤�ɃJ�������ړ�����, �p�l���ύX����.
    //�h���͎��̐����̔v�A���v�̍Ō�A���v�A�O���v�͓���̍ŏ��̔v�ɂȂ�.
    [PunRPC]
    public void AddDora()
    {
        GameObject gameController = null;
        gameController = GameObject.FindWithTag("GameController");
        GameManager gameManager = gameController.GetComponent<GameManager>();
        switch (tileNum)
        {
            case 0:
                gameManager.doraList.Add(6);
                break;
            case 4:
                gameManager.doraList.Add(0);
                gameManager.doraList.Add(5);
                break;
            case 9:
                gameManager.doraList.Add(1);
                break;
            case 10:
                gameManager.doraList.Add(16);
                break;
            case 14:
                gameManager.doraList.Add(10);
                gameManager.doraList.Add(15);
                break;
            case 19:
                gameManager.doraList.Add(11);
                break;
            case 20:
                gameManager.doraList.Add(26);
                break;
            case 24:
                gameManager.doraList.Add(20);
                gameManager.doraList.Add(25);
                break;
            case 29:
                gameManager.doraList.Add(21);
                break;
            case 33:
                gameManager.doraList.Add(30);
                break;
            case 36:
                gameManager.doraList.Add(34);
                break;
            default:
                gameManager.doraList.Add(tileNum + 1);
                break;
        }
        UIController uiController = gameController.GetComponent<UIController>();
        uiController.changeDoraIndicator(gameManager.doraCounter, tileNum);
        gameManager.doraCounter++;
        CameraController cameraController = gameController.GetComponent<CameraController>();
        cameraController.PlayingCameraTransform(gameManager.myOrder);
        uiController.Dora_Panel.SetActive(false);
        uiController.Playing_Panel.SetActive(true);
        gameManager.nextReady = true;
    }
}
