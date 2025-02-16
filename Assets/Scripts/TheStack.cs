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

    // Start is called before the first frame update
    void Start()
    {
        if(originBlock == null)
        {
            Debug.Log("OriginBlock is null");
            return;
        }

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
}
