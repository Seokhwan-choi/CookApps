using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace HexBlast
{
    class BlockMatchManager
    {
        GameManager mParent;
        public void Init(GameManager parent)
        {
            mParent = parent;
        }
        
        public bool CheckCanMatches(HexBlock swap1, HexBlock swap2)
        {
            return AnyCreateSpecialBlock(swap1) || CheckLineMatch(swap1, 3) ||
                AnyCreateSpecialBlock(swap2) || CheckLineMatch(swap2, 3);
        }

        public List<HexBlock> FindAllMatches(HexBlock swap1 = null, HexBlock swap2 = null)
        {
            List<HexBlock> finalMatched = new();
            List<HexBlock> newSpecialBlocks = new();

            // Ư�� �� ����
            foreach (var block in mParent.GetBlocks())
            {
                // �� �� ����
                if (block.IsEmpty) 
                    continue;

                // Ư�� �� ���� ( UFO, ���� )
                if (block.SpecialType == SpecialType.UFO || 
                    block.ReactiveType == ReactiveType.PegTop)
                    continue;

                // ��ġ ó���Ǿ� ���� �����̶�� �������ش�.
                if (finalMatched.Contains(block))
                    continue;

                // ���� ���� Ư�� ���� �������ش�.
                if (newSpecialBlocks.Contains(block))
                    continue;

                // ��� ���� ���ؼ� Ư�� �� ���� �õ�
                if (TryCreateSpecialBlock(swap1, swap2, block, out HexBlock specialBlock, out List<HexBlock> specialMatched))
                {
                    newSpecialBlocks.Add(specialBlock);

                    finalMatched.AddRange(specialMatched);
                }

                // �Ϲ� �� ��Ī�Ǵ��� Ȯ��
                var normalMatched = GetLineMatchedBlocks(block, 3);
                if (normalMatched.Count > 0)
                {
                    finalMatched.AddRange(normalMatched);
                }
            }

            // 1. �ߺ� ����, Ư�� �� ������ �� ������ ���� �Ϲ� ��Ī�� ���� ������ ���� �ߺ��� �� �ִ�.
            // 2. ���� ������ Ư�� ���� ����
            finalMatched = finalMatched.Distinct().Except(newSpecialBlocks).ToList();
            
            return finalMatched;
        }

        bool AnyCreateSpecialBlock(HexBlock pivot)
        {
            if (CheckLineMatch(pivot, 5))
                return true;

            if (CheckLineMatch(pivot, 4))
                return true;

            if (CheckYShapeMatch(pivot))
                return true;

            if (CheckVShapeMatch(pivot))
                return true;

            if (CheckDiamondMatch(pivot.XYZ, pivot.Color))
                return true;

            return false;
        }

        bool TryCreateSpecialBlock(HexBlock swap1, HexBlock swap2, HexBlock pivot, out HexBlock specialBlock, out List<HexBlock> specialMatched)
        {
            specialMatched = new List<HexBlock>();
            specialBlock = null;

            // 1. ���� 5�� �̻� - UFO
            var line5 = GetLineMatchedBlocks(pivot, 5);
            if (line5.Count > 0)
            {
                specialBlock = ConfigureSpecialBlock(SpecialType.UFO, swap1, swap2, pivot, line5);

                specialMatched.AddRange(line5);

                return true;
            }

            // 2. ���� 4�� - ����
            var line4 = GetLineMatchedBlocks(pivot, 4);
            if (line4.Count > 0)
            {
                specialBlock = ConfigureSpecialBlock(SpecialType.Rocket, swap1, swap2, pivot, line4);

                specialMatched.AddRange(line4);

                return true;
            }

            // 3. Y�� ���� - �ź���
            var yBlocks = GetYShapeMatchedBlocks(pivot);
            if (yBlocks.Count > 0)
            {
                specialBlock = ConfigureSpecialBlock(SpecialType.Turtle, swap1, swap2, pivot, yBlocks);

                specialMatched.AddRange(yBlocks);

                return true;
            }

            // 4. V�� ���� - TNT
            var vBlocks = GetVShapeMatchedBlocks(pivot);
            if (vBlocks.Count > 0)
            {
                specialBlock = ConfigureSpecialBlock(SpecialType.TNT, swap1, swap2, pivot, vBlocks);

                specialMatched.AddRange(vBlocks);

                return true;
            }

            // 5. ������ ���� - �θ޶�
            var diamondBlocks = GetDiamondMatchedBlocks(pivot);
            if (diamondBlocks.Count > 0)
            {
                specialBlock = ConfigureSpecialBlock(SpecialType.Boomerang, swap1, swap2, pivot, diamondBlocks);

                specialMatched.AddRange(diamondBlocks);

                return true;
            }

            return false;

            HexBlock ConfigureSpecialBlock(SpecialType specialType, HexBlock swap1, HexBlock swap2, HexBlock center, List< HexBlock> matched)
            {
                // ������ ���ؼ� ������ Ư�� ���̶��
                // ������ �� ���� Ư�� ���� �Ǿ���Ѵ�.
                if (swap1 != null && swap1.IsEmpty == false && matched.Contains(swap1))
                {
                    swap1.SetSpecial(specialType);

                    return swap1;
                }
                else if (swap2 != null && swap2.IsEmpty == false && matched.Contains(swap2))
                {
                    swap2.SetSpecial(specialType);

                    return swap2;
                }
                else
                {
                    center.SetSpecial(specialType);

                    return center;
                }
            }
        }

        // ========== ���� ��Ī =============
        bool CheckLineMatch(HexBlock pivot, int requiredCount)
        {
            if (pivot.IsReactive)
                return false;

            return CheckLineMatch(pivot.XYZ, pivot.Color, requiredCount);
        }

        bool CheckLineMatch(Vector3Int pivot, BlockColor color, int requiredCount)
        {
            var result = GetLineMatchedBlocks(pivot, color, requiredCount);

            return result.Count > 0;
        }

        List<HexBlock> GetLineMatchedBlocks(HexBlock pivot, int requireCount)
        {
            var matched = GetLineMatchedBlocks(pivot.XYZ, pivot.Color, requireCount);
            if (matched.Count > 0)
                matched.Add(pivot);

            return matched;
        }
        List<HexBlock> GetLineMatchedBlocks(Vector3Int pivot, BlockColor color, int requiredCount)
        {
            List<HexBlock> line = new List<HexBlock>();

            // ���� ��ġ�� �������� �������� ��� ���� üũ�ؾ��Ѵ�.
            // ��, ������ ������ ��� Ȯ���ϸ� ��
            // �� ��, ������ ������ Ȯ���ϸ鼭 ������ 3���⿡ ���ؼ��� üũ���� �ʾƵ� ��
            foreach (var dir in HexUtil.HexDirs.Take(3))
            {
                line.Clear();

                var cur = pivot;
                // ������
                CheckLineMatch(color, cur, dir, line);

                // ������
                var revCur = pivot;
                var revDir = HexUtil.GetRevDir(dir);
                CheckLineMatch(color, revCur, revDir, line);

                int count = line.Count(x => x.IsReactive == false);

                // ���� �����ϸ� �߰��� �ٷ� ������
                if (count >= requiredCount - 1)
                {
                    return line;
                }
            }

            line.Clear();

            return line;
        }

        void CheckLineMatch(BlockColor pivotColor, Vector3Int cur, Vector3Int dir, List<HexBlock> line)
        {
            CheckLine(pivotColor, cur, dir, line);
        }

        public void CheckFullLine(HexBlock pivot, List<HexBlock> line)
        {
            foreach (var dir in HexUtil.HexDirs.Take(3))
            {
                // ������
                var cur = pivot.XYZ;
                CheckLine(BlockColor.None, cur, dir, line);

                // ������
                var revCur = pivot.XYZ;
                var revDir = new Vector3Int(-dir.x, -dir.y, -dir.z);
                CheckLine(BlockColor.None, revCur, revDir, line);
            }
        }

        public void CheckLine(BlockColor pivotColor, Vector3Int cur, Vector3Int dir, List<HexBlock> line)
        {
            while (true)
            {
                cur += dir;

                if (HexBlast.GameManager.IsInsideGrid(cur) == false)
                    break;

                var block = HexBlast.GameManager.GetBlock(cur);
                if (pivotColor != BlockColor.None && pivotColor != block.Color)
                    break;

                line.Add(block);
            }
        }

        // ========== ������(���̾Ƹ��) ��Ī =============
        bool CheckDiamondMatch(Vector3Int pivot, BlockColor color)
        {
            return GetDiamondMatchedBlocks(pivot, color).Count > 0;
        }

        public List<HexBlock> GetDiamondMatchedBlocks(HexBlock pivot)
        {
            var matched = GetDiamondMatchedBlocks(pivot.XYZ, pivot.Color);
            if (matched.Count > 0)
                matched.Add(pivot);

            return matched;
        }

        public List<HexBlock> GetDiamondMatchedBlocks(Vector3Int pivot, BlockColor color)
        {
            // =========
            //  �� ��
            //   �� ��
            // =========
            //    ��
            //  �� ��
            //   ��
            // =========
            // ���� ���� ������ ����� ��� ã�� ���� �������� ��� �˻�
            List<HexBlock> matched = new List<HexBlock>();
            foreach (var dir in HexUtil.HexDirs)
            {
                // ===== ù ��° ���̽� (���� + �� + (����+��))
                var front = dir;
                var right = HexUtil.Rotate60(dir, 1);
                var frontfront = dir + right;

                if (IsDiamondMatch(pivot, color, front, right, frontfront, out matched))
                    return matched;

                // ===== �� ��° ���̽� (�¿� + ����)
                var front2 = dir;
                var left2 = HexUtil.Rotate60(dir, -1);
                var right2 = HexUtil.Rotate60(dir, 1);

                if (IsDiamondMatch(pivot, color, left2, right2, front2, out matched))
                    return matched;
            }

            return matched;
        }

        bool IsDiamondMatch(Vector3Int pivot, BlockColor color, Vector3Int dirA, Vector3Int dirB, Vector3Int dirC, out List<HexBlock> matched)
        {
            matched = new List<HexBlock>();

            var b1 = mParent.GetBlock(pivot + dirA);
            var b2 = mParent.GetBlock(pivot + dirB);
            var b3 = mParent.GetBlock(pivot + dirC);

            if (b1 != null && b2 != null && b3 != null)
            {
                if (b1.IsEmpty == false && b2.IsEmpty == false && b3.IsEmpty == false)
                {
                    if (b1.IsReactive == false && b2.IsReactive == false && b3.IsReactive == false)
                    {
                        if (b1.Color == color && b2.Color == color && b3.Color == color)
                        {
                            matched = new List<HexBlock> { b1, b2, b3 };
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // ========== Y ��� ��Ī =============
        bool CheckYShapeMatch(HexBlock pivot)
        {
            return GetYShapeMatchedBlocks(pivot).Count > 0;
        }
        List<HexBlock> GetYShapeMatchedBlocks(HexBlock pivot)
        {
            // ========
            //  ��  ��
            //    ��
            //    ��
            //    ��
            // ========
            // ���� ���� ����� ���� �������� ��� �˻�
            var matched = new List<HexBlock>();
            foreach (var dir in HexUtil.HexDirs)
            {
                var leftUp = HexUtil.Rotate60(dir, -1);             // ���� ���
                var rightUp = HexUtil.Rotate60(dir, 1);             // ���� ���
                var rev = HexUtil.GetRevDir(dir);                   // �Ʒ�
                var rev2 = rev * 2;                                 // 2ĭ �Ʒ�

                var centerXYZ = pivot.XYZ;
                var p1 = centerXYZ + leftUp;
                var p2 = centerXYZ + rightUp;
                var p3 = centerXYZ + rev;
                var p4 = centerXYZ + rev2;

                var b1 = mParent.GetBlock(p1);
                var b2 = mParent.GetBlock(p2);
                var b3 = mParent.GetBlock(p3);
                var b4 = mParent.GetBlock(p4);

                if (b1 != null && b2 != null && b3 != null && b4 != null)
                {
                    if (b1.IsEmpty == false && b2.IsEmpty == false && 
                        b3.IsEmpty == false && b4.IsEmpty == false)

                    {
                        if (b1.IsReactive == false && b2.IsReactive == false && 
                            b3.IsReactive == false && b4.IsReactive == false)
                        {
                            if (b1.Color == pivot.Color && b2.Color == pivot.Color &&
                                b3.Color == pivot.Color && b4.Color == pivot.Color)
                            {
                                matched = new List<HexBlock> { b1, b2, b3, b4, pivot };
                                return matched;
                            }
                        }
                    }
                }
            }

            return matched;
        }

        // ========== V ��� ��Ī =============
        bool CheckVShapeMatch(HexBlock pivot)
        {
            return GetVShapeMatchedBlocks(pivot).Count > 0;
        }
        List<HexBlock> GetVShapeMatchedBlocks(HexBlock pivot)
        {
            // ==========
            // ��      ��
            //   ��  ��
            //     ��
            // ==========
            // ���� ���� ����� ���� �������� ��� �˻�
            List<HexBlock> matched = new List<HexBlock>();
            foreach (var dir in HexUtil.HexDirs)
            {
                var front = dir;                                    // ����
                var frontfront = dir * 2;                           // ���� ����
                var leftUp = HexUtil.Rotate60(dir, -1);             // ���� ���
                var leftUpUp = leftUp * 2;                          // ���� ��� ���
                var rightUp = HexUtil.Rotate60(dir, 1);             // ���� ���
                var rightUpUp = rightUp * 2;                        // ���� ��� ���

                // ù��° ���̽� ( ��, �� )
                if (IsVShapeMatch(pivot.XYZ, pivot.Color, leftUp, leftUpUp, rightUp, rightUpUp, out matched))
                {
                    matched.Add(pivot);

                    return matched;
                }
                    
                // �ι�° ���̽� ( ����, �� )
                if (IsVShapeMatch(pivot.XYZ, pivot.Color, front, frontfront, leftUp, leftUpUp, out matched))
                {
                    matched.Add(pivot);

                    return matched;
                }

                // ����° ���̽� ( ����, �� )
                if (IsVShapeMatch(pivot.XYZ, pivot.Color, front, frontfront, rightUp, rightUpUp, out matched))
                {
                    matched.Add(pivot);

                    return matched;
                }
            }

            return matched;
        }

        bool IsVShapeMatch(Vector3Int pivot, BlockColor color, Vector3Int dirA, Vector3Int dirB, Vector3Int dirC, Vector3Int dirD, out List<HexBlock> matched)
        {
            matched = new List<HexBlock>();

            var b1 = mParent.GetBlock(pivot + dirA);
            var b2 = mParent.GetBlock(pivot + dirB);
            var b3 = mParent.GetBlock(pivot + dirC);
            var b4 = mParent.GetBlock(pivot + dirD);

            if (b1 != null && b2 != null && b3 != null && b4 != null)
            {
                if (b1.IsEmpty == false && b2.IsEmpty == false &&
                    b3.IsEmpty == false && b4.IsEmpty == false)
                {
                    if (b1.IsReactive == false && b2.IsReactive == false &&
                        b3.IsReactive == false && b4.IsReactive == false)
                    {
                        if (b1.Color == color && b2.Color == color &&
                            b3.Color == color && b4.Color == color)
                        {
                            matched = new List<HexBlock> { b1, b2, b3, b4 };
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // ��Ī�� ���� �ʴ� ���� ����
        public BlockColor GetNonMatchingColor(Vector3Int xyz)
        {
            var colors = Enum.GetValues(typeof(BlockColor)).Cast<BlockColor>()
                .Where(c => c != BlockColor.None && c != BlockColor.Count)
                .OrderBy(c => UnityEngine.Random.value);

            foreach (var color in colors)
            {
                if (CanMatch(xyz, color) == false)
                    return color;
            }

            return colors.First();
        }

        bool CanMatch(Vector3Int xyz, BlockColor color)
        {
            return CheckLineMatch(xyz, color, 3) || CheckDiamondMatch(xyz, color);
        }

        public bool CanMatch(HexBlock swap1, HexBlock swap2)
        {
            // ���ҵ� ���� ��������
            // UFO ������ Ȯ��
            // 3match Ȯ��
            // ������(���̾Ƹ��)match Ȯ��
            // ���� match�� �߻��ߴٸ� ���� ó�� ����
            return swap1.SpecialType == SpecialType.UFO || swap2.SpecialType == SpecialType.UFO ||
                CheckLineMatch(swap1, 3) || CheckLineMatch(swap2, 3) ||
                CheckDiamondMatch(swap1.XYZ, swap1.Color) || CheckDiamondMatch(swap2.XYZ, swap2.Color);
        }
    }
}