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

            // �̿� ���� �� ���� ������ +1,-1,0 �����Եȴ�.
            // ��, �̿� ���� ���� �� ������ ������ ��� ���ϸ� 2
            return dx + dy + dz == 2;
        }
    }
}
