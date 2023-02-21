using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayManager : MonoBehaviour
{
    [SerializeField] Text pointText;
    [SerializeField] Text targetPointText;

    private int starPoint = 0;
    private int moveCount = 0;
    private int targetPoint = 15;
    private bool gameEnd = false;
    private GameObject selectedBlock;
    private GameObject targetBlock;
    private GameObject resultWindow;
    private BlockManager blockManager;

    private bool test = false;

    void Awake()
    {
        InitGame();
    }
    void InitGame()
    {
        blockManager = GameObject.FindGameObjectWithTag("BlockManager").GetComponent<BlockManager>();
        targetPointText.text = string.Format($"{targetPoint}");
        resultWindow = GameObject.FindGameObjectWithTag("ResultWindow");
        resultWindow.SetActive(false);
    }

    void Update()
    {
        if (gameEnd) return;

        if (Input.touchCount > 0 || test)
        {
            if (test)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 clickPosToVector3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(clickPosToVector3);
                    RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                    if (hit.collider != null)
                    {
                        selectedBlock = hit.collider.gameObject;
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    Vector3 clickPosToVector3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1);
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(clickPosToVector3);
                    RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                    if (hit.collider != null)
                    {
                        targetBlock = hit.collider.gameObject;
                        int clickBlockNum = selectedBlock.GetComponent<Block>().GetBlockNum();
                        if(clickBlockNum == (int)DefineUtils.BlockType.Munchkin)
                        {
                            
                            Vector3 dirVec = (targetBlock.transform.position - selectedBlock.transform.position).normalized;
                            int count = blockManager.MoveMunchkin(selectedBlock, dirVec);
                            GetStarPoint(count);
                        }
                        else
                        {
                            if (selectedBlock != null)
                            {
                                blockManager.SwapBlock(selectedBlock, targetBlock);

                                //매칭시 실행
                                int point = 0;
                                if ((point += blockManager.DetectBlockMatch(selectedBlock)) != 0 || (point += blockManager.DetectBlockMatch(targetBlock)) != 0)
                                {
                                    GetStarPoint(point);
                                    if(selectedBlock != targetBlock)
                                        MoveCounting();
                                }
                                else
                                {
                                    blockManager.SwapBlock(targetBlock, selectedBlock);
                                }
                            }
                        }
                        selectedBlock = null;
                        targetBlock = null;
                    }
                }
            }
            else
            {
                Touch touch = Input.GetTouch(0);

                Vector3 touchPosToVector3 = new Vector3(touch.position.x, touch.position.y, 1);

                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touchPosToVector3);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (hit.collider != null)
                        {
                            selectedBlock = hit.collider.gameObject;
                        }
                        break;
                    case TouchPhase.Ended:
                        if (hit.collider != null)
                        {
                            targetBlock = hit.collider.gameObject;
                            int clickBlockNum = selectedBlock.GetComponent<Block>().GetBlockNum();
                            if (clickBlockNum == (int)DefineUtils.BlockType.Munchkin)
                            {

                                Vector3 dirVec = (targetBlock.transform.position - selectedBlock.transform.position).normalized;
                                int count = blockManager.MoveMunchkin(selectedBlock, dirVec);
                                GetStarPoint(count);
                            }
                            else
                            {
                                if (selectedBlock != null)
                                {
                                    blockManager.SwapBlock(selectedBlock, targetBlock);

                                    //매칭시 실행
                                    int point = 0;
                                    if ((point += blockManager.DetectBlockMatch(selectedBlock)) != 0 || (point += blockManager.DetectBlockMatch(targetBlock)) != 0)
                                    {
                                        GetStarPoint(point);
                                        if (selectedBlock != targetBlock)
                                            MoveCounting();
                                    }
                                    else
                                    {
                                        blockManager.SwapBlock(targetBlock, selectedBlock);
                                    }
                                }
                            }
                            selectedBlock = null;
                            targetBlock = null;
                        }
                        break;
                }

            }

        }
    }

    void GetStarPoint(int count = 1)
    {
        while(count > 0)
        {
            pointText.text = string.Format($"{++starPoint}");
            if(starPoint >= targetPoint)
            {
                resultWindow.SetActive(true);
                gameEnd = true;
            }
            count--;
        }
    }
    void MoveCounting()
    {
        moveCount++;
        GameObject.FindGameObjectWithTag("MoveCountText").GetComponent<Text>().text = string.Format($"{moveCount}");
    }
}
