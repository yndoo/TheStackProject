using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class TheStack : MonoBehaviour
{
    // 기준값으로 사용될 상수값들
    private const float BoundSize = 3.5f;       // 블럭 사이즈
    private const float MovingBoundsSize = 3f;  // 이동량
    private const float StackMovingSpeed = 5f;  // 이동 스피드
    private const float BlockMovingSpeed = 3.5f;
    private const float ErrorMargin = 0.1f;     // 성공으로 취급할 에러 마진

    public GameObject originBlock = null;

    private Vector3 prevBlockPosition;
    private Vector3 desiredPosition;
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // 새롭게 생성될 블록 사이즈값 저장해둠

    Transform lastBlock = null;
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;
    public int Score { get { return stackCount; } }
    int comboCount = 0;
    public int Combo { get { return comboCount; } }

    private int maxCombo = 0;
    public int MaxCombo { get => maxCombo; }

    public Color prevColor;
    public Color nextColor;

    bool isMovingX = true;

    int bestScore = 0;
    public int BestScore { get => bestScore; }

    int bestCombo = 0;
    public int BestCombo { get => bestCombo; }

    private const string BestScoreKey = "BestScore";
    private const string BestComboKey = "BestCombo";

    private bool isGameOver = true;

    void Start()
    {
        if(originBlock == null)
        {
            Debug.Log("OriginBlock is null");
            return;
        }

        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        bestCombo = PlayerPrefs.GetInt(BestComboKey, 0);

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        prevBlockPosition = Vector3.down;

        SpawnBlock();
        SpawnBlock();
    }

    void Update()
    {
        if (isGameOver) return;

        if(Input.GetMouseButtonDown(0))
        {
            if(PlaceBlock())
            {
                SpawnBlock();
            }
            else
            {
                // 게임 오버
                Debug.Log("Game Over");
                UpdateScore();
                isGameOver = true;
                GameOverEffect();
                UIManager.Instance.SetScoreUI();
            }
        }

        MoveBlock();

        //transform.position = desiredPosition; //이렇게 하면 TheStack이 움직이는 게 안느껴짐.  
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    bool SpawnBlock()
    {
        if(lastBlock != null)
        {
            prevBlockPosition = lastBlock.localPosition; // 마지막 위치 받아둠. TheStack 오브젝트 기준으로 위치 잡기 때문에 localPosition으로 받음.
        }

        GameObject newBlock = null;
        Transform newTrans = null;

        newBlock = Instantiate(originBlock); // Instantiate 내부 보면 originBlock 클론해서 생성해주는 함수

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed");
            return false;
        }
        ColorChange(newBlock);

        // 새로 생성한 블록은 부모없이 그냥 씬에 있는 애임. TheStack 하위로 들어가려면 부모를 자신의 trans로 바꿔줘야 함.
        // 계층 구조를 Transform이 가지고 있기 때문에!! 
        newTrans = newBlock.transform;
        newTrans.parent = this.transform;
        newTrans.localPosition = prevBlockPosition + Vector3.up; // 블록 y scale이 1이라 up 한 칸만 해줘도 올라감. 
        newTrans.localRotation = Quaternion.identity; // 쿼터니언의 초기값. == 회전 없는 상태. 
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        stackCount++;

        desiredPosition = Vector3.down * stackCount; // 스택카운트가 증가하는 만큼 TheStack을 내려줄거임. (가장 위에 있는 블록이 화면 중앙에 있도록.)
        blockTransition = 0f;

        lastBlock = newTrans;
        isMovingX = !isMovingX;

        UIManager.Instance.UpdateScore();

        return true;
    }

    Color GetRandomColor()
    {
        float r = Random.Range(100f, 250f) / 255f; // 100부터 한 이유는 100보다 아래값으로 설정하면 너무 어두워서임. 
        float g = Random.Range(100f, 250f) / 255f;
        float b = Random.Range(100f, 250f) / 255f;

        return new Color(r, g, b);
    }

    void ColorChange(GameObject go)
    {
        Color applyColor = Color.Lerp(prevColor, nextColor, (stackCount % 11/*0 ~ 10 순환하는 값 나옴*/) / 10f); // prev~next컬러의 중간값들 스택카운트에 맞춰서 나오는거

        // 우리 Block이 갖고 있는 렌더러는 메쉬 렌더러. Renderer가 그거의 부모 클래스
        Renderer rn = go.GetComponent<Renderer>(); 
        if(rn == null)
        {
            Debug.Log("Renderer is null");
            return;
        }

        rn.material.color = applyColor; // 컬러나 재질 등은 material이 처리하고 있음
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f);

        if(applyColor.Equals(nextColor) == true) // 컬러 Lerp 다 끝났으면
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();
        }
    }

    void MoveBlock()
    {
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2; // 핑퐁은 0부터 size까지 왔다갔다 순환하는 값

        if (isMovingX)
        {
            lastBlock.localPosition = new Vector3(movePosition * MovingBoundsSize, stackCount, secondaryPosition);
        }
        else
        {
            lastBlock.localPosition = new Vector3(secondaryPosition, stackCount, -movePosition * MovingBoundsSize);
        }
    }

    bool PlaceBlock()
    {
        Vector3 lastPosition = lastBlock.transform.localPosition;

        if (isMovingX)
        {
            float deltaX = prevBlockPosition.x - lastPosition.x; // 이전 블록과 현재 블록의 중심 좌표 차이이자, 새로 올린 블럭이 외곽에서 벗어난 차이
            bool isNegativeNum = (deltaX < 0) ? true : false;   // 블록의 어느쪽에서 떨어트릴지 이거로 정함 

            deltaX = Mathf.Abs(deltaX);
            if(deltaX > ErrorMargin)
            {
                stackBounds.x -= deltaX; // 다음 생성할 사이즈 깎아줌
                if(stackBounds.x <= 0) 
                {
                    return false;   // 게임오버
                }
                float middle = (prevBlockPosition.x + lastPosition.x) / 2f;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                Vector3 tempPos = lastBlock.localPosition;
                tempPos.x = middle;
                lastBlock.localPosition = lastPosition = tempPos;

                // 파편 생성
                float rubbleHalfScale = deltaX / 2;
                CreateRubble(
                    new Vector3(
                        isNegativeNum
                        ? lastPosition.x + stackBounds.x / 2 + rubbleHalfScale
                        : lastPosition.x - stackBounds.x / 2 - rubbleHalfScale
                        , lastPosition.y
                        , lastPosition.z
                        ),
                    new Vector3(deltaX, 1, stackBounds.y)
                );
                comboCount = 0;
            }
            else
            {
                // 잘 올려둔 경우. 
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up; // 위치 보정
            }
        }
        else 
        {
            float deltaZ = prevBlockPosition.z - lastPosition.z;
            bool isNegativeNum = (deltaZ < 0) ? true : false;   // 블록의 어느쪽에서 떨어트릴지 이거로 정함 

            deltaZ = Mathf.Abs(deltaZ);
            if(deltaZ > ErrorMargin)
            {
                stackBounds.y -= deltaZ;
                if(stackBounds.y <= 0)
                {
                    return false;
                }
                float middle = (prevBlockPosition.z + lastPosition.z) / 2f;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                Vector3 tempPos = lastBlock.localPosition;
                tempPos.z = middle;
                lastBlock.localPosition = lastPosition = tempPos;

                // 파편 생성
                float rubbleHalfScale = deltaZ / 2;
                CreateRubble(
                    new Vector3(
                        lastPosition.x,
                        lastPosition.y,
                        isNegativeNum
                        ? lastPosition.z + stackBounds.y / 2 + rubbleHalfScale
                        : lastPosition.z - stackBounds.y / 2 - rubbleHalfScale
                        ),
                    new Vector3(stackBounds.x, 1, deltaZ)
                );

                comboCount = 0;
            }
            else
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }

        // 움직인 방향에따라서 이동방향에 사용됐던 x 또는 z값을 저장. 이전 블록의 중심을 사용하기 위함.
        secondaryPosition = (isMovingX) ? lastBlock.localPosition.x : lastBlock.localPosition.z;
        
        return true;
    }

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = Instantiate(lastBlock.gameObject);
        go.transform.parent = this.transform;   // TheStack 안으로 넣어주는 것

        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;

        go.AddComponent<Rigidbody>();
        go.name = "Rubble"; // 게임오브젝트의 이름을 바꿈.
    }

    void ComboCheck()
    {
        comboCount++;

        if(comboCount > maxCombo)
        {
            maxCombo = comboCount;
        }

        if(comboCount % 5 == 0)
        {
            Debug.Log("5 Combo Success!");
            
            stackBounds += new Vector3(0.5f, 0.5f);
            stackBounds.x = (stackBounds.x > BoundSize) ? BoundSize : stackBounds.x;
            stackBounds.y = (stackBounds.y > BoundSize) ? BoundSize : stackBounds.y;
        }
    }

    void UpdateScore()
    {
        if (bestScore < stackCount)
        {
            Debug.Log("최고기록 갱신");
            bestScore = stackCount;
            bestCombo = maxCombo;

            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.SetInt(BestComboKey, bestCombo);
        }
    }

    void GameOverEffect()
    {
        int childCount = this.transform.childCount; // 이 transform의 하위 자식 개수!!

        for(int i = 1; i < 20; i ++) // 상위 20개만 이펙트를 줄 것임.
        {
            if (childCount < i) break;

            GameObject go = transform.GetChild(childCount - i).gameObject; // 제일 끝에서부터 가져옴

            if (go.name.Equals("Rubble")) continue;

            Rigidbody rigid = go.AddComponent<Rigidbody>();
            rigid.AddForce(
                (Vector3.up * Random.Range(0, 10f) + Vector3.right * (Random.Range(0, 10f) - 5f)) * 100f
                );
        }
    }

    public void Restart()   // 대부분의 데이터를 초기화해주기
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        isGameOver = false;

        lastBlock = null;
        desiredPosition = Vector3.zero;
        stackBounds = new Vector3(BoundSize, BoundSize);

        stackCount = -1;
        isMovingX = true;
        blockTransition = 0f;
        secondaryPosition = 0f;

        comboCount = 0;
        maxCombo = 0;

        prevBlockPosition = Vector3.down;

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        SpawnBlock();
        SpawnBlock();
    }

}
