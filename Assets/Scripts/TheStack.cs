using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    // ���ذ����� ���� �������
    private const float BoundSize = 3.5f;       // �� ������
    private const float MovingBoundsSize = 3f;  // �̵���
    private const float StackMovingSpeed = 5f;  // �̵� ���ǵ�
    private const float BlockMovingSpeed = 3.5f;
    private const float ErrorMargin = 0.1f;     // �������� ����� ���� ����

    public GameObject originBlock = null;

    private Vector3 prevBlockPosition;
    private Vector3 desiredPosition;
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // ���Ӱ� �����Ǵ� ��� ����� �����ص�

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

        //transform.position = desiredPosition; //�̷��� �ϸ� TheStack�� �����̴� �� �ȴ�����.  
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    bool SpawnBlock()
    {
        if(lastBlock != null)
        {
            prevBlockPosition = lastBlock.localPosition; // ������ ��ġ �޾Ƶ�. TheStack ������Ʈ �������� ��ġ ��� ������ localPosition���� ����.
        }

        GameObject newBlock = null;
        Transform newTrans = null;

        newBlock = Instantiate(originBlock); // Instantiate ���� ���� originBlock Ŭ���ؼ� �������ִ� �Լ�

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed");
            return false;
        }

        // ���� ������ ����� �θ���� �׳� ���� �ִ� ����. TheStack ������ ������ �θ� �ڽ��� trans�� �ٲ���� ��.
        // ���� ������ Transform�� ������ �ֱ� ������!! 
        newTrans = newBlock.transform;
        newTrans.parent = this.transform;
        newTrans.localPosition = prevBlockPosition + Vector3.up; // ��� y scale�� 1�̶� up �� ĭ�� ���൵ �ö�. 
        newTrans.localRotation = Quaternion.identity; // ���ʹϾ��� �ʱⰪ. == ȸ�� ���� ����. 
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        stackCount++;

        desiredPosition = Vector3.down * stackCount; // ����ī��Ʈ�� �����ϴ� ��ŭ TheStack�� �����ٰ���. (���� ���� �ִ� ����� ȭ�� �߾ӿ� �ֵ���.)
        blockTransition = 0f;

        lastBlock = newTrans;
        return true;
    }
}
