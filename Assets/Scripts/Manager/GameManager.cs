using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace HexBlast
{
    class GameManager : MonoBehaviour
    {
        const int PegTopScore = 100;
        const int BlockScore = 10;
        const int RadiusX = 3;
        const int RadiusY = 2;

        int mScore;
        int mMove;
        int mMission;

        bool mIsPlay;
        bool mInRecursive;
        Transform mHexGridParent;
        BlockMatchManager mMatchManager;
        BlockGravityManager mGravityManager;

        // x,y,z ť�� ��ǥ ��� �׸���
        Dictionary<Vector3Int, HexBlock> mGrid = new();
        public void Init()
        {
            mHexGridParent = GameObject.Find("HexGrid").transform;

            mMatchManager = new BlockMatchManager();
            mMatchManager.Init(this);

            mGravityManager = new BlockGravityManager();
            mGravityManager.Init(this);

            mScore = 0;
            mMission = 10;
            mMove = 20;
            mIsPlay = true;

            HexBlast.MainUI.SetMission(mMission);
            HexBlast.MainUI.SetMove(mMove);
            HexBlast.MainUI.SetScore(mScore);

            GenerateHexGrid(RadiusX, RadiusY);
        }

        // �ʱ� �� ���� ( -radius ~ radius )
        void GenerateHexGrid(int radiusX, int radiusY)
        {
            for (int x = -radiusX; x <= radiusX; x++)
            {
                // ť�� ��ǥ��� x + y + z = 0 �� �׻� ������
                // y���� �ּ� �ִ븦 ��������
                int yMin = Mathf.Max(-radiusX, -x - radiusX);
                int yMax = Mathf.Min(radiusY, -x + radiusY);

                for (int y = yMin; y <= yMax; y++)
                {
                    int z = -x - y;

                    var xyz = new Vector3Int(x, y, z);

                    GameObject hex = HexBlast.ObjectPool.AcquireObject("HexBlock", mHexGridParent);

                    HexBlock block = hex.GetComponent<HexBlock>();

                    // ù ���� ���� �ٷ� match �߻����� �ʵ��� GetNonMatchingColor�� �� ���� 
                    block.Init(xyz, mMatchManager.GetNonMatchingColor(xyz));

                    // ���̷� ����
                    if (HexUtil.FixedPegTopPos.Contains(xyz))
                    {
                        block.SetReactive(ReactiveType.PegTop);
                    }

                    mGrid[xyz] = block;
                }
            }

            // ��ȿ�� �̵��� ���ٸ� ����
            if (HasAnyValidMove() == false)
                ResetUntilValid();
        }

        public IEnumerable<HexBlock> GetBlocks()
        {
            return mGrid.Values;
        }

        public HexBlock GetBlock(Vector3Int key)
        {
            return mGrid.TryGetValue(key, out HexBlock block) ? block : null;
        }

        public bool IsInsideGrid(Vector3Int key)
        {
            return mGrid.ContainsKey(key);
        }

        public bool IsEmpty(Vector3Int key)
        {
            return mGrid.TryGetValue(key, out var block) && block.IsEmpty;
        }

        public void TrySwap(HexBlock a, HexBlock b)
        {
            // ���� ó���߿��� ���� X
            if (mInRecursive || mIsPlay == false)
                return;

            if (HexBlockUtil.IsNeighbors(a, b))
            {
                StartCoroutine(SwapAndCheck(a, b));
            }
        }

        IEnumerator SwapAndCheck(HexBlock a, HexBlock b)
        {
            yield return SwapNMoveMotion(a, b);

            if (mMatchManager.CanMatch(a, b))
            {
                mMove--;

                HexBlast.MainUI.SetMove(mMove);

                StartMatchResolution(a, b);
            }
            else
            {
                // ���� ����
                yield return SwapNMoveMotion(a, b);
            }
        }

        public void StartMatchResolution(HexBlock swap1, HexBlock swap2)
        {
            StartCoroutine(ResolveMatches(swap1, swap2));
        }

        IEnumerator ResolveMatches(HexBlock swap1, HexBlock swap2)
        {
            yield return new WaitForSeconds(0.2f);

            // �÷��̾��� ���ҿ� ���� �۵�
            if (swap1 != null && swap2 != null)
            {
                // ������ ����� UFO �۵�
                if (swap1.SpecialType == SpecialType.UFO)
                {
                    // UFO �۵�
                    swap1.ActivateSpecial(swap2);
                }
                else if (swap2.SpecialType == SpecialType.UFO)
                {
                    // UFO �۵�
                    swap2.ActivateSpecial(swap1);
                }

                // ���� �۵�
                yield return ResolveMatchesRecursively();

            }
            else 
            {
                // ���� �۵�
                yield return ResolveMatchesRecursively();
            }
        }

        public void StartResolveMatchesRecursively()
        {
            StartCoroutine(ResolveMatchesRecursively());
        }

        int mCombo;
        IEnumerator ResolveMatchesRecursively(HexBlock swap1 = null, HexBlock swap2 = null)
        {
            mInRecursive = true;
            // 1. Ư�� �� ���� �� �Ϲ� ��Ī Ȯ��
            var matched = mMatchManager.FindAllMatches(swap1, swap2);
            if (matched.Count == 0)
            {
                // ���� ��
                mCombo = 0;
                mInRecursive = false;

                // ��ȿ�� �̵��� ���ٸ� ����
                if (HasAnyValidMove() == false)
                {
                    HexBlast.MainUI.ShowNotify(NotifyType.Shuffle);

                    ResetUntilValid();
                }

                CheckStageFinish();

                yield break;
            }

            yield return ResolveMatchesRecursively(matched);
        }

        IEnumerator ResolveMatchesRecursively(List<HexBlock> matched)
        {
            // 2. ��Ī �Ϸ�� ������ ������ �� ( ���� )�鿡�� �˷��ش�.
            NotifyNeighborsOfMatches(matched);

            // 3. ��Ī �Ϸ�� �� ����
            RemoveBlocks(matched);

            yield return new WaitForSeconds(0.2f);

            // 4. �߷� �ۿ�
            yield return mGravityManager.DropExistingBlocks();

            // 5. �� �ڸ� ä���
            yield return SpawnNewBlocks();

            // 6. ���� ���� ó�� ��� ȣ��
            StartResolveMatchesRecursively();

            // ���� ���Ⱑ ���۵Ǳ� ������ Combo++
            mCombo++;
        }

        // Ư�� ���� ������ ������ ���� ��������
        void ResetUntilValid()
        {
            do
            {
                foreach (var block in mGrid.Values.ToList())
                {
                    if (block.IsSpecial || block.IsReactive)
                        continue;

                    block.SetColor(mMatchManager.GetNonMatchingColor(block.XYZ));
                }
            }
            while (HasAnyValidMove() == false);
        }

        // �ѹ� �����ؼ� ��Ī�� �� �ִ��� Ȯ��
        bool HasAnyValidMove()
        {
            foreach (var block in mGrid.Values.ToList())
            {
                if (block.IsEmpty)
                    continue;

                foreach (var dir in HexUtil.HexDirs)
                {
                    var neighborXYZ = block.XYZ + dir;
                    if (IsInsideGrid(neighborXYZ) == false)
                        continue;

                    var neighborBlock = GetBlock(neighborXYZ);
                    if (neighborBlock.IsEmpty || neighborBlock.IsReactive)
                        continue;

                    // �������� ����
                    Swap(block, neighborBlock);

                    var matchResult = mMatchManager.CheckCanMatches(block, neighborBlock);

                    // ���� �� ����
                    Swap(block, neighborBlock);

                    // ���� ��Ī�� �ִٸ� true
                    if (matchResult)
                        return true;
                }
            }

            return false; // ��ȿ�� ������ ����
        }

        void NotifyNeighborsOfMatches(List<HexBlock> matched)
        {
            var notified = new HashSet<HexBlock>();

            foreach (var block in matched)
            {
                foreach (var dir in HexUtil.HexDirs)
                {
                    var neighborDir = block.XYZ + dir;

                    var neighborBlock = GetBlock(neighborDir);
                    if (neighborBlock == null || neighborBlock.IsEmpty)
                        continue;

                    if (neighborBlock.IsReactive == false)
                        continue;

                    if (notified.Contains(neighborBlock) == false)
                    {
                        neighborBlock.NofifyMatchedNeighbor();

                        notified.Add(neighborBlock);
                    }
                }
            }
        }

        public void RemoveBlocks(List<HexBlock> blocks, HexBlock swap = null)
        {
            foreach (var block in blocks)
            {
                block.OnRemove(swap, blocks);
            }
        }

        IEnumerator SpawnNewBlocks()
        {
            // ���� ��
            // yMin = Mathf.Max(-RadiusX, x - RadiusX);
            int yMin = Mathf.Max(-RadiusX, 0 - RadiusX);

            int spawnX = 0;
            int spawnY = yMin;
            int spawnZ = -spawnX - spawnY;
            var spawnKey = new Vector3Int(spawnX, spawnY, spawnZ);

            List<Coroutine> gravities = new();

            while (IsEmpty(spawnKey))
            {
                var randomColor = GetRandomColor();
                // ���� ��ġ�� �ణ ���� ����
                var spawnPos = HexUtil.CubeToWorld(spawnX, spawnZ) + Vector3.up * 2;

                // ���� ��ġ�� �ִ� 12�� ����� ��Ȱ��
                var block = GetBlock(spawnKey);
                block.Init(spawnKey, randomColor);
                block.SetWorldPos(spawnPos);

                // ���� ��ġ���� �ڿ������� �� ��ġ�� �̵�
                var tween = block.MoveToPos(HexUtil.CubeToWorld(spawnX, spawnZ));

                yield return tween.WaitForCompletion();

                gravities.Add(StartCoroutine(mGravityManager.SimulateGravity(block)));
            }

            // ���ÿ� �Ϸ� ���
            foreach (var gravity in gravities)
                yield return gravity;
        }

        BlockColor GetRandomColor()
        {
            return (BlockColor)UnityEngine.Random.Range(0, (int)BlockColor.Count);
        }

        public void Swap(HexBlock a, HexBlock b)
        {
            // ��ǥ�� Swap
            Vector3Int tempAXYZ = a.XYZ;
            Vector3Int tempBXYZ = b.XYZ;

            a.SetXYZ(tempBXYZ);
            b.SetXYZ(tempAXYZ);

            // ���ֵ̹� ���� �ٲ��ֱ�
            a.name = $"Hex ({tempBXYZ.x},{tempBXYZ.y},{tempBXYZ.z})";
            b.name = $"Hex ({tempAXYZ.x},{tempAXYZ.y},{tempAXYZ.z})";

            // ��ųʸ��� ����
            mGrid[tempBXYZ] = a;
            mGrid[tempAXYZ] = b;
        }

        IEnumerator SwapNMoveMotion(HexBlock a, HexBlock b)
        {
            Swap(a, b);

            yield return WaitForMoveMotionCompletion(a, b);
        }

        IEnumerator WaitForMoveMotionCompletion(HexBlock a, HexBlock b)
        {
            var t1 = a.MoveToPos(a.GetWorldPos());
            var t2 = b.MoveToPos(b.GetWorldPos());

            yield return t1.WaitForCompletion();
            yield return t2.WaitForCompletion();
        }

        public void OnUFOActivate(List<HexBlock> matched)
        {
            RemoveBlocks(matched);

            StartCoroutine(ResolveMatchesRecursively(matched));
        }

        public void OnClearBlock(HexBlock block)
        {
            if (block.IsReactive)
            {
                mScore += PegTopScore;
            }
            else
            {
                mScore += (BlockScore * (mCombo + 1));
            }

            HexBlast.MainUI.SetScore(mScore);

            mMission = mGrid.Values.Where(x => x.IsReactive).Count();

            HexBlast.MainUI.SetMission(mMission);
        }

        void CheckStageFinish()
        {
            if (mMission <= 0)
            {
                HexBlast.MainUI.ShowNotify(NotifyType.StageClear);

                mIsPlay = false;
            }
            else
            {
                if (mMove <= 0)
                {
                    HexBlast.MainUI.ShowNotify(NotifyType.GameOver);

                    mIsPlay = false;
                }
            }
        }

        public void RestartStage()
        {
            foreach(var block in mGrid.Values.ToList())
            {
                HexBlast.ObjectPool.ReleaseObject(block.gameObject);
            }

            mGrid.Clear();

            mScore = 0;
            mMission = 10;
            mMove = 20;
            mIsPlay = true;

            HexBlast.MainUI.SetMission(mMission);
            HexBlast.MainUI.SetMove(mMove);
            HexBlast.MainUI.SetScore(mScore);

            GenerateHexGrid(RadiusX, RadiusY);
        }

        public void CheckFullLine(HexBlock pivot, List<HexBlock> line)
        {
            mMatchManager.CheckFullLine(pivot, line);
        }

        public void CheckLine(BlockColor color, Vector3Int cur, Vector3Int dir, List<HexBlock> line)
        {
            mMatchManager.CheckLine(color, cur, dir, line);
        }
    }
}
