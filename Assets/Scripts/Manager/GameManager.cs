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

        // x,y,z 큐브 좌표 기반 그리드
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

        // 초기 맵 생성 ( -radius ~ radius )
        void GenerateHexGrid(int radiusX, int radiusY)
        {
            for (int x = -radiusX; x <= radiusX; x++)
            {
                // 큐브 좌표계는 x + y + z = 0 을 항상 만족함
                // y값의 최소 최대를 조정해줌
                int yMin = Mathf.Max(-radiusX, -x - radiusX);
                int yMax = Mathf.Min(radiusY, -x + radiusY);

                for (int y = yMin; y <= yMax; y++)
                {
                    int z = -x - y;

                    var xyz = new Vector3Int(x, y, z);

                    GameObject hex = HexBlast.ObjectPool.AcquireObject("HexBlock", mHexGridParent);

                    HexBlock block = hex.GetComponent<HexBlock>();

                    // 첫 생성 부터 바로 match 발생하지 않도록 GetNonMatchingColor로 색 지정 
                    block.Init(xyz, mMatchManager.GetNonMatchingColor(xyz));

                    // 팽이로 지정
                    if (HexUtil.FixedPegTopPos.Contains(xyz))
                    {
                        block.SetReactive(ReactiveType.PegTop);
                    }

                    mGrid[xyz] = block;
                }
            }

            // 유효한 이동이 없다면 리셋
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
            // 아직 처리중에는 스왑 X
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
                // 스왑 원복
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

            // 플레이어의 스왑에 의한 작동
            if (swap1 != null && swap2 != null)
            {
                // 스왑한 대상이 UFO 작동
                if (swap1.SpecialType == SpecialType.UFO)
                {
                    // UFO 작동
                    swap1.ActivateSpecial(swap2);
                }
                else if (swap2.SpecialType == SpecialType.UFO)
                {
                    // UFO 작동
                    swap2.ActivateSpecial(swap1);
                }

                // 연쇄 작동
                yield return ResolveMatchesRecursively();

            }
            else 
            {
                // 연쇄 작동
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
            // 1. 특수 블럭 생성 및 일반 매칭 확인
            var matched = mMatchManager.FindAllMatches(swap1, swap2);
            if (matched.Count == 0)
            {
                // 연쇄 끝
                mCombo = 0;
                mInRecursive = false;

                // 유효한 이동이 없다면 리셋
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
            // 2. 매칭 완료된 블럭들을 반응형 블럭 ( 팽이 )들에게 알려준다.
            NotifyNeighborsOfMatches(matched);

            // 3. 매칭 완료된 블럭 삭제
            RemoveBlocks(matched);

            yield return new WaitForSeconds(0.2f);

            // 4. 중력 작용
            yield return mGravityManager.DropExistingBlocks();

            // 5. 빈 자리 채우기
            yield return SpawnNewBlocks();

            // 6. 다음 연쇄 처리 재귀 호출
            StartResolveMatchesRecursively();

            // 다음 연쇄가 시작되기 때문에 Combo++
            mCombo++;
        }

        // 특수 블럭을 제외한 블럭들의 색을 섞어주자
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

        // 한번 스왑해서 매칭할 수 있는지 확인
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

                    // 가상으로 스왑
                    Swap(block, neighborBlock);

                    var matchResult = mMatchManager.CheckCanMatches(block, neighborBlock);

                    // 스왑 후 원복
                    Swap(block, neighborBlock);

                    // 무언가 매칭이 있다면 true
                    if (matchResult)
                        return true;
                }
            }

            return false; // 유효한 스왑이 없음
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
            // 제일 위
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
                // 스폰 위치를 약간 위에 설정
                var spawnPos = HexUtil.CubeToWorld(spawnX, spawnZ) + Vector3.up * 2;

                // 스폰 위치에 있는 12시 블록을 재활용
                var block = GetBlock(spawnKey);
                block.Init(spawnKey, randomColor);
                block.SetWorldPos(spawnPos);

                // 스폰 위치에서 자연스럽게 블럭 위치로 이동
                var tween = block.MoveToPos(HexUtil.CubeToWorld(spawnX, spawnZ));

                yield return tween.WaitForCompletion();

                gravities.Add(StartCoroutine(mGravityManager.SimulateGravity(block)));
            }

            // 동시에 완료 대기
            foreach (var gravity in gravities)
                yield return gravity;
        }

        BlockColor GetRandomColor()
        {
            return (BlockColor)UnityEngine.Random.Range(0, (int)BlockColor.Count);
        }

        public void Swap(HexBlock a, HexBlock b)
        {
            // 좌표계 Swap
            Vector3Int tempAXYZ = a.XYZ;
            Vector3Int tempBXYZ = b.XYZ;

            a.SetXYZ(tempBXYZ);
            b.SetXYZ(tempAXYZ);

            // 네이밍도 같이 바꿔주기
            a.name = $"Hex ({tempBXYZ.x},{tempBXYZ.y},{tempBXYZ.z})";
            b.name = $"Hex ({tempAXYZ.x},{tempAXYZ.y},{tempAXYZ.z})";

            // 딕셔너리도 적용
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
