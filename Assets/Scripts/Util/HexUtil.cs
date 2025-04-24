using UnityEngine;
using System;

namespace HexBlast
{
    enum HexDir
    {
        Up,
        RightUp,
        RightDown,
        Down,
        LeftDown,
        LeftUp,

        Right,
        Left,
    }

    static class HexUtil
    {
        // 테스트용 팽이 고정 좌표
        public static readonly Vector3Int[] FixedPegTopPos = new Vector3Int[10]
        {
            new Vector3Int(0, 0, 0), 
            new Vector3Int(1, -3, 2), 
            new Vector3Int(2, -3, 1), 
            new Vector3Int(2, 0, -2), 
            new Vector3Int(1, 1, -2), 
            new Vector3Int(0, 2, -2), 
            new Vector3Int(-1, 2, -1), 
            new Vector3Int(-2, 2, 0), 
            new Vector3Int(-2, -1, 3), 
            new Vector3Int(-1, -2, 3), 
        };

        public static readonly Vector3Int[] HexDirs = new Vector3Int[6]
        {
            new Vector3Int(0, -1, 1), // 위
            new Vector3Int(1, -1, 0), // 오른쪽 위
            new Vector3Int(1, 0, -1), // 오른쪽 아래
            new Vector3Int(0, 1, -1), // 아래
            new Vector3Int(-1, 1, 0), // 왼쪽 아래
            new Vector3Int(-1, 0, 1), // 왼쪽 위
        };

        public static readonly Vector3Int[] TNTRanges = new Vector3Int[12]
        {
            new Vector3Int(0, -1, 1),  // 위
            new Vector3Int(1, -1, 0),  // 오른쪽 위
            new Vector3Int(2, -2, 0),  // 오른쪽 위위
            new Vector3Int(2, -1, -1), // 오른쪽
            new Vector3Int(1, 0, -1),  // 오른쪽 아래
            new Vector3Int(2, 0, -2),  // 오른쪽 아래아래
            new Vector3Int(0, 1, -1),  // 아래
            new Vector3Int(-1, 1, 0),  // 왼쪽 아래
            new Vector3Int(-2, 2, 0),  // 왼쪽 아래아래
            new Vector3Int(-2, 1, 1),  // 왼쪽
            new Vector3Int(-1, 0, 1),  // 왼쪽 위
            new Vector3Int(-2, 0, 2),  // 왼쪽 위위
        };

        public static float GetDegree(Vector3Int dir)
        {
            int index = Array.IndexOf(HexDirs, dir);
            if (index < 0)
                return 0f;

            // 총 6개의 방향 360 기준으로 60도씩
            return index * 60f;
        }

        public static Vector3Int Rotate60(Vector3Int dir, int times)
        {
            int index = Array.IndexOf(HexDirs, dir);
            if (index < 0) 
                return dir;

            // 시계방향 회전 (times > 0), 반시계방향 (times < 0)
            int newIndex = (index + times + 6) % 6;

            return HexDirs[newIndex];
        }

        public static Vector3Int GetHexDir(HexDir dir)
        {
            switch (dir)
            {
                case HexDir.Right:
                    return new Vector3Int(2, -1, -1);
                case HexDir.Left:
                    return new Vector3Int(-1, 1, 1);
                case HexDir.Up:
                case HexDir.RightUp:
                case HexDir.RightDown:
                case HexDir.Down:
                case HexDir.LeftDown:
                case HexDir.LeftUp:
                default:
                    int dirInedx = Math.Clamp((int)dir, 0, HexDirs.Length - 1);
                    return HexDirs[dirInedx];
            }
        }

        public static Vector3Int GetRevDir(Vector3Int dir)
        {
            return new Vector3Int(-dir.x, -dir.y, -dir.z);
        }

        public static Vector3Int GetRandomDir()
        {
            int randomDirIndex = UnityEngine.Random.Range(0, HexDirs.Length);

            return HexDirs[randomDirIndex];
        }

        public static Vector3 CubeToWorld(Vector3Int cubePos)
        {
            return CubeToWorld(cubePos.x, cubePos.z);
        }

        public static Vector3 CubeToWorld(int x, int z)
        {
            // 사용되는 블럭의 스프라이트 크기 68x68
            // PPU를 68로 설정하여 1유닛으로 맞춤
            // 즉 width, height 값이 1이 됨
            float width = 1f;
            float height = 1f;

            // 자연스러운 배치를 위해
            // 적절한 값을 넣어줌 0.75f / 2f
            float xPos = width * 0.75f * x;
            float yPos = height * (z + x / 2f);

            return new Vector2(xPos, yPos);
        }
    }
}