using System.Collections.Generic;
using UnityEngine;

namespace DefineUtils
{
    public struct BlockInfo
    {
        public static int minBlockNum = 0;
        public static int maxBlockNum = 3;
    }

    public enum BlockType
    {
        Red         = 0,
        Green,
        Blue,
        Orange,
        Munchkin,
        End
    }

    public enum Direction
    {
        Up          = 0,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        None,
        End
    }

    public enum MatchType
    {
        Match1X3         = 1,
        Match2X2         = 2,
        Match3X1         = 3,
        SpecialMatch     = 4,
        End
    }
    public static class FirstGenBlockInfo
    {
        
        public static Dictionary<Vector2, int> firstBlockDic = new Dictionary<Vector2, int>()
        {
            //레드 블루 그린 오렌지                                                                          레드 블루 그린 오렌지
            {new Vector2(0,0), 0 }, {new Vector2(0,1), 1 }, {new Vector2(0,2), 0 }, {new Vector2(0,3), 1 }, {new Vector2(0,4), 0 }, {new Vector2(0,5), 3 }, {new Vector2(0,6), 3 },
            {new Vector2(1,0), 3 }, {new Vector2(1,1), 1 }, {new Vector2(1,2), 2 }, {new Vector2(1,3), 3 }, {new Vector2(1,4), 2 }, {new Vector2(1,5), 0 }, {new Vector2(1,6), 1 },
            {new Vector2(2,0), 1 }, {new Vector2(2,1), 2 }, {new Vector2(2,2), 0 }, {new Vector2(2,3), 0 }, {new Vector2(2,4), 3 }, {new Vector2(2,5), 1 }, {new Vector2(2,6), 3 },
            {new Vector2(3,0), 3 }, {new Vector2(3,1), 3 }, {new Vector2(3,2), 0 }, {new Vector2(3,3), 2 }, {new Vector2(3,4), 0 }, {new Vector2(3,5), 3 }, {new Vector2(3,6), 2 },
            {new Vector2(4,0), 1 }, {new Vector2(4,1), 2 }, {new Vector2(4,2), 2 }, {new Vector2(4,3), 3 }, {new Vector2(4,4), 2 }, {new Vector2(4,5), 2 }, {new Vector2(4,6), 1 },
            {new Vector2(5,0), 2 }, {new Vector2(5,1), 3 }, {new Vector2(5,2), 1 }, {new Vector2(5,3), 2 }, {new Vector2(5,4), 3 }, {new Vector2(5,5), 2 }, {new Vector2(5,6), 3 },
            {new Vector2(6,0), 2 }, {new Vector2(6,1), 2 }, {new Vector2(6,2), 1 }, {new Vector2(6,3), 0 }, {new Vector2(6,4), 1 }, {new Vector2(6,5), 3 }, {new Vector2(6,6), 3 }
        };
    }

}


