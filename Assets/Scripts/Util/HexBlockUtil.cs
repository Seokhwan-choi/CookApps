using UnityEngine;


namespace HexBlast
{
    static class HexBlockUtil
    {
        public static bool IsNeighbors(HexBlock a, HexBlock b)
        {
            var aXYZ = a.XYZ;
            var bXYZ = b.XYZ;

            int dx = Mathf.Abs(aXYZ.x - bXYZ.x);
            int dy = Mathf.Abs(aXYZ.y - bXYZ.y);
            int dz = Mathf.Abs(aXYZ.z - bXYZ.z);

            // 이웃 블럭의 각 축은 무조건 +1,-1,0 가지게된다.
            // 즉, 이웃 블럭에 대한 축 차이의 절댓값을 모두 더하면 2
            return dx + dy + dz == 2;
        }
    }
}
