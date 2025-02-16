using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // 새롭게 생성되는 블록 사이즈값 저장해둠

    Transform lastBlock = null;
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;
    int comboCount = 0;

    public Color prevColor;
    public Color nextColor;

    // Start is called before the first frame update
    void Start()
    {
        if(originBlock == null)
        {
            Debug.Log("OriginBlock is null");
            return;
        }
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        prevBlockPosition = Vector3.down;

        SpawnBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            SpawnBlock();
        }

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
}
