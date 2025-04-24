using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HexBlast
{
    class BlockGravityManager
    {
        GameManager mParent;
        public void Init(GameManager parent)
        {
            mParent = parent;
        }

        public IEnumerator SimulateGravity(HexBlock block)
        {
            while (true)
            {
                var to = GetGravityTarget(block);
                if (to.HasValue == false)
                    break;

                var fromBlock = block;
                var toBlock = mParent.GetBlock(to.Value);
                var toPos = HexUtil.CubeToWorld(to.Value.x, to.Value.z);

                mParent.Swap(fromBlock, toBlock);

                var tween = fromBlock.MoveToPos(toPos, duration: 0.05f);

                yield return tween.WaitForCompletion();
            }
        }

        public IEnumerator DropExistingBlocks()
        {
            bool moved;
            do
            {
                moved = false;

                var moveList = new List<(Vector3Int from, Vector3Int to)>();
                var excludeTargets = new HashSet<Vector3Int>(); // 중복 방지
                var movingTweens = new List<Tween>();

                foreach (var block in mParent.GetBlocks().Where(b => !b.IsEmpty))
                {
                    var from = block.XYZ;
                    var to = GetGravityTarget(block);

                    if (to.HasValue && excludeTargets.Contains(to.Value) == false)
                    {
                        moved = true;

                        moveList.Add((from, to.Value));
                        excludeTargets.Add(to.Value);
                    }
                }

                foreach (var (from, to) in moveList)
                {
                    var fromBlock = mParent.GetBlock(from);
                    var toBlock = mParent.GetBlock(to);
                    var toPos = HexUtil.CubeToWorld(to.x, to.z);

                    mParent.Swap(fromBlock, toBlock);

                    var tween = fromBlock.MoveToPos(toPos, duration: 0.05f);

                    movingTweens.Add(tween);
                }

                foreach (var t in movingTweens)
                    yield return t.WaitForCompletion();

            } while (moved);
        }

        Vector3Int? GetGravityTarget(HexBlock block, bool ignoreLeftRight = false)
        {
            // 아래
            var below = block.XYZ + HexUtil.GetHexDir(HexDir.Down);
            if (mParent.IsInsideGrid(below) && mParent.IsEmpty(below))
                return below;

            if (ignoreLeftRight == false)
            {
                // 왼쪽
                var leftDown = block.XYZ + HexUtil.GetHexDir(HexDir.LeftDown);
                if (mParent.IsInsideGrid(leftDown) && mParent.IsEmpty(leftDown))
                    return leftDown;

                // 오른쪽
                var rightDown = block.XYZ + HexUtil.GetHexDir(HexDir.RightDown);
                if (mParent.IsInsideGrid(rightDown) && mParent.IsEmpty(rightDown))
                    return rightDown;
            }

            return null;
        }
    }
}