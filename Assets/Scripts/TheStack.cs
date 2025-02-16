using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.AI;

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
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);    // ���Ӱ� ������ ��� ����� �����ص�

    Transform lastBlock = null;
    float blockTransition = 0f;
    float secondaryPosition = 0f;

    int stackCount = -1;
    int comboCount = 0;

    public Color prevColor;
    public Color nextColor;

    bool isMovingX = true;

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
        SpawnBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(PlaceBlock())
            {
                SpawnBlock();
            }
            else
            {
                // ���� ����
                Debug.Log("Game Over");
            }
        }

        MoveBlock();

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
        ColorChange(newBlock);

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
        isMovingX = !isMovingX;

        return true;
    }

    Color GetRandomColor()
    {
        float r = Random.Range(100f, 250f) / 255f; // 100���� �� ������ 100���� �Ʒ������� �����ϸ� �ʹ� ��ο�����. 
        float g = Random.Range(100f, 250f) / 255f;
        float b = Random.Range(100f, 250f) / 255f;

        return new Color(r, g, b);
    }

    void ColorChange(GameObject go)
    {
        Color applyColor = Color.Lerp(prevColor, nextColor, (stackCount % 11/*0 ~ 10 ��ȯ�ϴ� �� ����*/) / 10f); // prev~next�÷��� �߰����� ����ī��Ʈ�� ���缭 �����°�

        // �츮 Block�� ���� �ִ� �������� �޽� ������. Renderer�� �װ��� �θ� Ŭ����
        Renderer rn = go.GetComponent<Renderer>(); 
        if(rn == null)
        {
            Debug.Log("Renderer is null");
            return;
        }

        rn.material.color = applyColor; // �÷��� ���� ���� material�� ó���ϰ� ����
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f);

        if(applyColor.Equals(nextColor) == true) // �÷� Lerp �� ��������
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();
        }
    }

    void MoveBlock()
    {
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2; // ������ 0���� size���� �Դٰ��� ��ȯ�ϴ� ��

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
            float deltaX = prevBlockPosition.x - lastPosition.x; // ���� ��ϰ� ���� ����� �߽� ��ǥ ��������, ���� �ø� ���� �ܰ����� ��� ����
            bool isNegativeNum = (deltaX < 0) ? true : false;   // ����� ����ʿ��� ����Ʈ���� �̰ŷ� ���� 

            deltaX = Mathf.Abs(deltaX);
            if(deltaX > ErrorMargin)
            {
                stackBounds.x -= deltaX; // ���� ������ ������ �����
                if(stackBounds.x <= 0) 
                {
                    return false;   // ���ӿ���
                }
                float middle = (prevBlockPosition.x + lastPosition.x) / 2f;
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

                Vector3 tempPos = lastBlock.localPosition;
                tempPos.x = middle;
                lastBlock.localPosition = lastPosition = tempPos;

                // ���� ����
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
            }
            else
            {
                // �� �÷��� ���. 
                lastBlock.localPosition = prevBlockPosition + Vector3.up; // ��ġ ����. ������ ���ϸ� ��� �������� errormargin���� ���� ������ ���ðŴ�?
            }
        }
        else 
        {
            float deltaZ = prevBlockPosition.z - lastPosition.z;
            bool isNegativeNum = (deltaZ < 0) ? true : false;   // ����� ����ʿ��� ����Ʈ���� �̰ŷ� ���� 

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

                // ���� ����
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
            }
            else
            {
                lastBlock.localPosition = prevBlockPosition + Vector3.up;
            }
        }

        // ������ ���⿡���� �̵����⿡ ���ƴ� x �Ǵ� z���� ����. ���� ����� �߽��� ����ϱ� ����.
        secondaryPosition = (isMovingX) ? lastBlock.localPosition.x : lastBlock.localPosition.z;
        
        return true;
    }

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = Instantiate(lastBlock.gameObject);
        go.transform.parent = this.transform;   // TheStack ������ �־��ִ� ��

        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;

        go.AddComponent<Rigidbody>();
        go.name = "Rubble"; // ���ӿ�����Ʈ�� �̸��� �ٲ�.
    }
}
