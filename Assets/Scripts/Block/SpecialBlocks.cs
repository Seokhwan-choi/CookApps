using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace HexBlast
{
    interface ISpecialBehavior
    {
        void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes);
    }

    class NoneSpecial : ISpecialBehavior
    {
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes) { }
    }

    // 로켓 - 한 라인 블럭 모두 삭제
    class RocketSpeicalBlock : ISpecialBehavior
    {
        Vector3Int mDir;        // 로켓 진행 방향
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes)
        {
            var line = new List<HexBlock>();

            var cur = owner.XYZ;
            var dir = mDir;
            // 색 상관없이 라인 정방향 검사
            HexBlast.GameManager.CheckLine(BlockColor.None, cur, dir, line);

            var revCur = owner.XYZ;
            var revDir = HexUtil.GetRevDir(mDir);
            // 색 상관없이 라인 역방향 검사
            HexBlast.GameManager.CheckLine(BlockColor.None, revCur, revDir, line);

            owner.Clear();

            HexBlast.GameManager.RemoveBlocks(line);
        }
        public void SetRocketDir(Vector3Int dir)
        {
            mDir = dir;
        }
    }

    // UFO - 특정 색상 모두 삭제
    class UFOSpecialBlocks : ISpecialBehavior
    {
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes)
        { 
            // UFO를 스왑해서 건든게 아니라면 터지지않는다.
            if (swap != null)
            {
                var sameColorBlocks = HexBlast.GameManager.GetBlocks().Where(b => b.Color == swap.Color).ToList();

                owner.Clear();

                HexBlast.GameManager.OnUFOActivate(sameColorBlocks);
            }
        }
    }

    // 부메랑 - 특정 블럭 삭제
    class BoomerangSpecialBlock : ISpecialBehavior
    {
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes)
        {
            List<HexBlock> targets = new List<HexBlock>();
            if (removes != null)
                targets = HexBlast.GameManager.GetBlocks().Where(b => !b.IsEmpty && b != owner).Except(removes).ToList();
            else
                targets = HexBlast.GameManager.GetBlocks().Where(b => !b.IsEmpty && b != owner).Except(removes).ToList();
            if (targets.Count > 0)
            {
                var randomTarget = targets[Random.Range(0, targets.Count)];

                owner.Clear();

                randomTarget.OnRemove(removes:removes);
            }
        }
    }

    // TNT - 주변 블럭 삭제
    class TNTSpecialBlock : ISpecialBehavior
    {
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes)
        {
            // 다음과 같은 모양 총 8개의 블록이 삭제된다.
            // ==========
            //  ㅁ    ㅁ
            //   ㅁㅁㅁ
            //  ㅁ ㅁ ㅁ
            //   ㅁㅁㅁ
            //  ㅁ    ㅁ
            // ==========

            var range = new List<HexBlock>();
            foreach (var dir in HexUtil.TNTRanges)
            {
                var p = owner.XYZ + dir;

                if (HexBlast.GameManager.IsInsideGrid(p) == false)
                    break;

                var block = HexBlast.GameManager.GetBlock(p);
                if (block.IsEmpty)
                    break;

                range.Add(block);
            }

            owner.Clear();

            HexBlast.GameManager.RemoveBlocks(range);
        }
    }

    // 거북이 - 주변 블럭과 전방향 라인 모두 삭제
    class TurtleSpecialBlock : ISpecialBehavior
    {
        public void Activate(HexBlock owner, HexBlock swap, List<HexBlock> removes)
        {
            // 6개 방향 라인 모두 삭제
            var line6 = new List<HexBlock>();

            HexBlast.GameManager.CheckFullLine(owner, line6);

            owner.Clear();

            HexBlast.GameManager.RemoveBlocks(line6);
        }
    }

    static class SpecialBlockUtil
    {
        public static ISpecialBehavior CreateSpecialBehavior(HexBlock hexBlock, SpecialType specialType)
        {
            switch (specialType)
            {
                case SpecialType.Rocket:
                    var rocketSpecial = new RocketSpeicalBlock();

                    var randomDir = HexUtil.GetRandomDir();

                    rocketSpecial.SetRocketDir(randomDir);

                    var curRotationZ = hexBlock.transform.eulerAngles.z;
                    var nextRotationZ = curRotationZ - HexUtil.GetDegree(randomDir);

                    hexBlock.transform.rotation = Quaternion.Euler(0f, 0f, nextRotationZ);

                    return rocketSpecial;
                case SpecialType.UFO:
                    return new UFOSpecialBlocks();
                case SpecialType.Boomerang:
                    return new BoomerangSpecialBlock();
                case SpecialType.TNT:
                    return new TNTSpecialBlock();
                case SpecialType.Turtle:
                    return new TurtleSpecialBlock();
                case SpecialType.Normal:
                default:
                    return new NoneSpecial();
            }
        }

        static public SpecialType ChangeToSpecialType(ISpecialBehavior special)
        {
            if (special is RocketSpeicalBlock)
                return SpecialType.Rocket;
            else if (special is UFOSpecialBlocks)
                return SpecialType.UFO;
            else if (special is BoomerangSpecialBlock)
                return SpecialType.Boomerang;
            else if (special is TNTSpecialBlock)
                return SpecialType.TNT;
            else if (special is TurtleSpecialBlock)
                return SpecialType.Turtle;
            else
                return SpecialType.Normal;
        }
    }
}
