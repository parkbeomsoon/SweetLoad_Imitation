using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    private int blockNum = 0;

    public Block(int num)
    {
        blockNum = num;
    }

    public void SetBlockNum(int num)
    {
        blockNum = num;
    }
    public int GetBlockNum()
    {
        return blockNum;
    }

    
}
