using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DefineUtils;

public class BlockManager : MonoBehaviour
{
    [SerializeField] Sprite[] blockSprites;
    [SerializeField] GameObject blockPrefab;
    [SerializeField] Tilemap tilemap;

    private int maxLengthBoard = 7;
    private float tileMoveSpeed = 1.5f;
    private GameObject BlockParent;
    Dictionary<Vector2, GameObject> blockList = new Dictionary<Vector2, GameObject>();
    public bool nextSpecialBlock = false;

    void Awake()
    {
        InitGame();
    }
    void Start()
    {
    }

    #region[시작전 세팅함수]

    private void InitGame()
    {
        BlockParent = GameObject.FindGameObjectWithTag("BlockParent");
        SetFirstBlocks();
    }
    //시작블럭 세팅용 함수
    public void SetFirstBlocks()
    {
        //초기블럭 생성
        for (int i = 0; i < maxLengthBoard; i++)
        {
            for(int j = 0; j < maxLengthBoard; j++)
            {
                Vector2 posIdxVec = new Vector2(i, j);
                int blockType = FirstGenBlockInfo.firstBlockDic[posIdxVec];
                GameObject blockGo = Instantiate(blockPrefab, BlockParent.transform);
                Block goBlock = blockGo.GetComponent<Block>();
                goBlock.SetBlockNum(blockType);
                goBlock.GetComponent<SpriteRenderer>().sprite = blockSprites[blockType];

                blockGo.transform.position = tilemap
                    .GetCellCenterWorld(new Vector3Int(j, -i, 0));

                blockList.Add(posIdxVec, blockGo);
            }
        }
    }

    #endregion

    #region[게임진행중 사용함수]

    public void SortBlock(GameObject baseBlock, MatchType matchType, Direction dir = Direction.None)
    {
        //빈블럭 탐색
        List<Vector2> emptyPos;
        //매칭타입에 따라 위에있는 블럭 아래로 이동
        bool endSort = false;
        bool regenSpecial = false;
        while (true)
        {
            emptyPos = DetectEmptyBlock();
            foreach (Vector2 vec in emptyPos)
            {
                int offset = 0;
                if (matchType != MatchType.SpecialMatch)
                    offset = -(int)matchType;
                else
                {
                    switch (dir)
                    {
                        case Direction.Up:
                            endSort = true;
                            break;
                        case Direction.Down:
                            offset = -emptyPos.Count;
                            break;
                        case Direction.Left:
                        case Direction.Right:
                            offset = -1;
                            break;
                    }
                }
                int posX = (int)vec.x + offset;
                if (vec.x + offset < 0) 
                {
                    posX = (int)vec.x;
                    endSort = true;
                }
                Vector2 replaceBlockPos = new Vector2(posX, vec.y);

                Vector3 targetPos = tilemap.GetCellCenterWorld(new Vector3Int((int)vec.y, (int)-vec.x, 0));

                if (baseBlock.transform.position == targetPos && matchType == MatchType.Match2X2)
                {
                    if (new Vector2(vec.x, vec.y) == replaceBlockPos)
                    {
                        regenSpecial = true;
                    }
                    else
                    {
                        MakeSpecialBlock(blockList[replaceBlockPos], matchType);
                    }
                }
                if (endSort) break;

                if (!blockList.ContainsKey(replaceBlockPos))
                {
                    endSort = true;
                    break;
                }

                else
                {
                    StartCoroutine(MoveTo(blockList[replaceBlockPos], targetPos));
                    blockList[vec] = blockList[replaceBlockPos];
                    blockList[replaceBlockPos] = null;
                }

            }
            if (endSort)
            {
                break;
            }
        }

        if (regenSpecial)
        {
            RegenBlock(matchType, baseBlock);
        }
        else RegenBlock(matchType);
    }
    public void RegenBlock(MatchType matchType, GameObject baseBlock = null)
    {
        //빈블럭 탐색
        List<Vector2> emptyPos = DetectEmptyBlock();

        //블럭 생성후 빈곳으로 이동
        foreach (Vector2 vec in emptyPos)
        {
            GameObject genBlock = GenerateBlock((int)vec.y, matchType);

            Vector3 targetPos = tilemap.GetCellCenterWorld(new Vector3Int((int)vec.y, (int)-vec.x, 0));
            StartCoroutine(MoveTo(genBlock, targetPos));
            blockList[vec] = genBlock;
        }
        if(baseBlock != null)
        {
            Vector3 targetPos = tilemap.WorldToCell(baseBlock.transform.position);

            blockList[new Vector2(-targetPos.y, targetPos.x)].GetComponent<Block>().SetBlockNum((int)BlockType.Munchkin);
            blockList[new Vector2(-targetPos.y, targetPos.x)].GetComponent<SpriteRenderer>().sprite = blockSprites[(int)BlockType.Munchkin];
        }
    }
    List<Vector2> DetectEmptyBlock()
    {
        List<Vector2> emptyPos = new List<Vector2>();
        emptyPos.Clear();

        for (int i = 0; i < maxLengthBoard; i++)
        {
            for (int j = 0; j < maxLengthBoard; j++)
            {
                Vector2 posVec = new Vector2(i, j);
                if (blockList[posVec] == null)
                {
                    emptyPos.Add(posVec);
                }
            }
        }

        if (emptyPos.Count == 0) return null;

        //위에서 부터 검사했으므로 역순으로 블럭이동
        emptyPos.Reverse();

        return emptyPos;
    }
    public GameObject GenerateBlock(int line, MatchType matchType, bool specialBlock = false)
    {
        GameObject blockGo = Instantiate(blockPrefab, GameObject.FindGameObjectWithTag("BlockParent").transform);
        SetBlock(blockGo);

        blockGo.transform.position = tilemap.GetCellCenterWorld(new Vector3Int(line, 1, 0));
        return blockGo;
    }

    public int DetectBlockMatch(GameObject centerBlock)
    {
        if (centerBlock == null) return 0;

        Vector3 centerBlockPos = centerBlock.transform.position;
        Vector3Int testPos = tilemap.WorldToCell(centerBlockPos);
        testPos = new Vector3Int(-testPos.y, testPos.x, testPos.z);
        int detectNum = centerBlock.GetComponent<Block>().GetBlockNum(); //찾을 블럭타입
        bool resultDetect = false;
        int totalPoint = 0;

        // 중앙타일 기준으로 3x3으로 검사 후 2x2 존재 시 블럭제거 후 true 반환

        List<GameObject> removeList = new List<GameObject>();
        
        while (true)
        {
            removeList.Clear();
            removeList.Add(centerBlock);
            nextSpecialBlock = false;

            #region[3매칭 검사]

            bool isRow = false;
            bool isCol = false;

            /*//위
            if ((testPos[0] - 1 > -1) && (blockList[new Vector2(testPos[0] - 1, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum))
            {
                //위
                if ((testPos[0] - 2 > -1) && (blockList[new Vector2(testPos[0] - 2, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum))
                {
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1])].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0] - 2, testPos[1])].gameObject);
                    isCol = true;
                }
                //아래
                if ((testPos[0] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0] + 1, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1])].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1])].gameObject);
                    isCol = true;
                }
            }
            //아래
            else if ((testPos[0] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0] + 1, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //아래
                if ((testPos[0] + 2 < maxLengthBoard) && blockList[new Vector2(testPos[0] + 2, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1])].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0] + 2, testPos[1])].gameObject);
                    isCol = true;
                }
            }

            //좌
            if ((testPos[1] - 1 > -1) && blockList[new Vector2(testPos[0], testPos[1] - 1)].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //좌
                if ((testPos[1] - 2 > -1) && blockList[new Vector2(testPos[0], testPos[1] - 2)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] - 1)].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] - 2)].gameObject);
                    isRow = true;
                }
                //우
                if ((testPos[1] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0], testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] - 1)].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] + 1)].gameObject);
                    isRow = true;
                }
            }
            //우
            else if ((testPos[1] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0], testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //우
                if ((testPos[1] + 2 < maxLengthBoard) && blockList[new Vector2(testPos[0], testPos[1] + 2)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] + 1)].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] + 2)].gameObject);
                    isRow = true;
                }
            }*/
            #endregion

            #region[2X2매칭검사]
            // 위 확인
            if ((testPos[0] - 1 > -1) && (blockList[new Vector2(testPos[0] - 1, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum))
            {
            //Debug.Log("위일치");
            //좌 확인
            if ((testPos[1] - 1 > -1) && blockList[new Vector2(testPos[0], testPos[1] - 1)].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //Debug.Log("좌일치");
                //좌대각 확인
                if (blockList[new Vector2(testPos[0] - 1, testPos[1] - 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    //Debug.Log("좌상일치");
                    //블럭제거
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1])].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] - 1)].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1] - 1)].gameObject);
                    nextSpecialBlock = true;
                }
            }
            //우 확인
            if ((testPos[1] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0], testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //Debug.Log("우일치");
                //우대각 확인
                if (blockList[new Vector2(testPos[0] - 1, testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    //Debug.Log("우상일치");
                    //블럭제거
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1])].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0], testPos[1] + 1)].gameObject);
                    removeList.Add(blockList[new Vector2(testPos[0] - 1, testPos[1] + 1)].gameObject);
                    nextSpecialBlock = true;
                }
            }
            }
            // 아래칸 확인
            if ((testPos[0] + 1 < maxLengthBoard) && blockList[new Vector2(testPos[0] + 1, testPos[1])].GetComponent<Block>().GetBlockNum() == detectNum)
            {
                //Debug.Log("하일치");
                //좌 확인
                if ((testPos[1] - 1 > -1) && blockList[new Vector2(testPos[0], testPos[1] - 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    //Debug.Log("좌일치");
                    //좌대각 확인
                    //Debug.Log("좌하일치");
                    if (blockList[new Vector2(testPos[0] + 1, testPos[1] - 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                    {
                        //블럭제거
                        removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1])].gameObject);
                        removeList.Add(blockList[new Vector2(testPos[0], testPos[1] - 1)].gameObject);
                        removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1] - 1)].gameObject);
                        nextSpecialBlock = true;
                    }
                }
                //우 확인
                if ((testPos[1] + 1 < 7) && blockList[new Vector2(testPos[0], testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                {
                    //Debug.Log("우일치");
                    //우대각 확인
                    if (blockList[new Vector2(testPos[0] + 1, testPos[1] + 1)].GetComponent<Block>().GetBlockNum() == detectNum)
                    {
                        //Debug.Log("우하일치");
                        //블럭제거
                        removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1])].gameObject);
                        removeList.Add(blockList[new Vector2(testPos[0], testPos[1] + 1)].gameObject);
                        removeList.Add(blockList[new Vector2(testPos[0] + 1, testPos[1] + 1)].gameObject);
                        nextSpecialBlock = true;
                    }
                }
            }
            bool match = false;

            //중복대상 제거
            removeList = removeList.Distinct().ToList();
            int matchType = 0;

            if (nextSpecialBlock)
            {
                matchType = (int)MatchType.Match2X2;
            }
            else
            {
                if (isCol) matchType = (int)MatchType.Match3X1;
                if (isRow) matchType = (int)MatchType.Match1X3;
            }

            if (removeList.Count > 1)
            {
                RemoveBlock(centerBlock, removeList, (MatchType)matchType);
                match = true;
                totalPoint += removeList.Count;
            }
            else break;
            //블럭제거 함수에 먼치킨블록생성 함수 넣기
            #endregion
        }

        
        return totalPoint;
    }
    public IEnumerator MoveTo(GameObject go, Vector3 toPos)
    {
        float count = 0;
        
        Vector3 wasPos = go.transform.position;
        while (true)
        {
            count += Time.deltaTime * tileMoveSpeed;
            go.transform.position = Vector3.Lerp(wasPos, toPos, count);

            if(count >= 1)
            {
                go.transform.position = toPos;
                DetectBlockMatch(go);
                break;
            }
            yield return null;
        }
    }
    public void SwapBlock(GameObject selectedBlock, GameObject targetBlock)
    {
        float count = 0;
        Vector3 selectedBlockPos = selectedBlock.transform.position;
        Vector3 targetBlockPos = targetBlock.transform.position;
        while (true)
        {
            count += Time.deltaTime * tileMoveSpeed;
            selectedBlock.transform.position = Vector3.Lerp(selectedBlockPos, targetBlockPos, count);
            targetBlock.transform.position = Vector3.Lerp(targetBlockPos, selectedBlockPos, count);

            if (count >= 1)
            {
                selectedBlock.transform.position = targetBlockPos;
                targetBlock.transform.position = selectedBlockPos;
                break;
            }
        }

        Vector3Int vecSelected = tilemap.WorldToCell(selectedBlockPos);
        Vector3Int vecTarget = tilemap.WorldToCell(targetBlockPos);

        // y값은 음수처리
        blockList[new Vector2(-vecSelected.y, vecSelected.x)] = targetBlock;
        blockList[new Vector2(-vecTarget.y, vecTarget.x)] = selectedBlock;

    }
    public void RemoveBlock(GameObject baseBlock, List<GameObject> targetList, MatchType matchType, Direction dir = Direction.None)
    {
        foreach(GameObject go in targetList)
        {
            Vector3Int vec3 = new Vector3Int(tilemap.WorldToCell(go.transform.position).x, tilemap.WorldToCell(go.transform.position).y, 0);
            Destroy(go);
            blockList[new Vector2(-vec3.y, vec3.x)] = null;
        }

        SortBlock(baseBlock, matchType, dir);

    }
    public void SetBlock(GameObject targetObject)
    {
        int newBlockNum = Random.Range(BlockInfo.minBlockNum, BlockInfo.maxBlockNum + 1);
        targetObject.GetComponent<Block>().SetBlockNum(newBlockNum);
        targetObject.GetComponent<SpriteRenderer>().sprite = blockSprites[newBlockNum];
    }
    public void MakeSpecialBlock(GameObject go, MatchType matchType)
    {
        switch (matchType)
        {
            case MatchType.Match2X2:
                go.GetComponent<Block>().SetBlockNum((int)BlockType.Munchkin);
                go.GetComponent<SpriteRenderer>().sprite = blockSprites[(int)BlockType.Munchkin];
                break;
        }
    }

    //먼치킨블럭 수행 제거포함 제거된오브젝트 수 리턴
    public int MoveMunchkin(GameObject blockGo, Vector3 dirVec)
    {
        int count = 0;
        Vector3 baseBlockPos = tilemap.WorldToCell(blockGo.transform.position); // y좌표 -로 처리(타일맵)

        List<GameObject> removeList = new List<GameObject>();
        Direction dir = Direction.None;
        if(dirVec == Vector3.up)
        {
            dir = Direction.Up;
            for (int i = -(int)baseBlockPos.y; i >= 0; i--)
            {
                removeList.Add(blockList[new Vector2(i, baseBlockPos.x)]);
                count++;
            }
        }
        else if (dirVec == Vector3.down)
        {
            dir = Direction.Down;
            for (int i = -(int)baseBlockPos.y; i < maxLengthBoard; i++)
            {
                removeList.Add(blockList[new Vector2(i, baseBlockPos.x)]);
                count++;
            }
        }
        else if (dirVec == Vector3.right)
        {
            dir = Direction.Right;
            for (int i = (int)baseBlockPos.x; i < maxLengthBoard; i++)
            {
                removeList.Add(blockList[new Vector2(-baseBlockPos.y, i)]);
                count++;
            }
        }
        else
        {
            dir = Direction.Left;
            for (int i = (int)baseBlockPos.x; i >= 0; i--)
            {
                removeList.Add(blockList[new Vector2(-baseBlockPos.y, i)]);
                count++;
            }
        }

        RemoveBlock(blockGo, removeList, MatchType.SpecialMatch, dir);

        return count;
    }

    #endregion


}
