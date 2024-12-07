using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleScript : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] AudioClip fripTileSE;
    [SerializeField] AudioClip dropTileSE;

    public GameObject fingerImage = null;
    public GameObject swipeText = null;
    public GameObject startText = null;

    private float tileLength = 0.28f;   //牌の縦幅.
    //private float tileWidth = 0.21f;   //牌の横幅.
    //private float tileHeight = 0.165f;   //牌の厚み.
    private float tileRatio = 1000.0f;   //牌をスクリーンで見たときの大きさに合わせるための倍率.
    private float screenWidthPercentage = 1f / 3f;   
    private float screenHeightPercentage = 2f / 3f;  

    public bool isScrolled = false;
    public bool isFinished = false;    //牌をめくり終えたかどうか.
    public bool inside = false;      //マウスカーソルがスクリーンの指定範囲内にあるかどうか.

    private float flightTime = 2 * 1.0f / 5.0f;
    private float rotationDegrees = 180f;    //回転させる角度.
    private float liftHeight = 9.8f / (2.0f * 5.0f * 5.0f);

    private float timer = -1.0f;
    private Vector3 lastMousePosition;  //直前のマウスの位置.


    // Start is called before the first frame update
    void Start()
    {
        //牌をめくられるように一部 transform を固定.
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX
            | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //範囲内でドラッグされたときに牌をめくりシーン変更する.
        Detection();
        if(!isScrolled && inside && Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        if (!isScrolled && lastMousePosition.y > 0 && Input.GetMouseButton(0))
        {
            //下方向に牌半個分以上移動していれば牌を捲る.
            //このときマウスカーソルの座標に対応するように大きくする(*1000).
            Vector3 deltaMousePosition = Input.mousePosition - lastMousePosition;
            if (deltaMousePosition.y < - tileLength * tileRatio / 2)
            {
                //Debug.Log("ok");
                isScrolled = true;

                audioSource.PlayOneShot(fripTileSE);

                fingerImage.SetActive(false);   //アニメーションの変更.
                swipeText.SetActive(false);
                startText.SetActive(true);

                Vector3 velocity = new Vector3(0, Mathf.Sqrt(2.0f * 9.8f * liftHeight), 0);
                this.timer = 0.0f;
                this.GetComponent<Rigidbody>().velocity = velocity;
            }
        }
        if (timer >= 0)
        {
            this.timer += Time.deltaTime;
            float rate = timer / flightTime;
            if(rate > 1)
            {
                rate = 1;
                timer = -1.0f;
            }
            //牌を半回転させる.
            float angle = Mathf.Lerp(0f, rotationDegrees, rate);
            this.transform.rotation = Quaternion.Euler(angle, 180.0f, 90.0f);
            if(rate == 1)
            {
                audioSource.PlayOneShot(dropTileSE);
                isFinished = true;
            }
        }

        //牌をめくり終えたらロビーに移動する.
        if (isFinished)
        {
            isFinished = false;
            Invoke("GoLobby", 1.0f);
        }
    }

    //マウスカーソルがスクリーンの指定範囲内にあるか確認するメソッド.
    public void Detection()
    {
        //スクリーンの真ん中1/3 かつ 下2/3の範囲.
        Rect detectionArea = new Rect(Screen.width * (1 - screenWidthPercentage) / 2, 0, 
            Screen.width * screenWidthPercentage, 
            Screen.height * screenHeightPercentage);
        if (detectionArea.Contains(Input.mousePosition))
        {
            inside = true;
        }
        else
        {
            inside = false;
            lastMousePosition.y = 0;
        }
    }

    public void GoLobby()
    {
        //Debug.Log("Went");
        SceneManager.LoadScene("LobbyScene");
    }
}
