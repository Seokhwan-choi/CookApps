using UnityEngine;

namespace HexBlast
{
    interface IReactiveBehavior
    {
        void OnNeighborMatched();
    }

    class NoneReactive : IReactiveBehavior
    {
        public void OnNeighborMatched() { }
    }

    // 팽이 - 주변 블럭 매칭 발생시 활성화 & 제거
    class PegTop : IReactiveBehavior
    {
        HexBlock mOwner;
        bool mActivated;
        public PegTop(HexBlock owner)
        {
            mOwner = owner;

            mOwner.SetSprite(HexBlast.Atlas.GetSprite($"PegTop_Lock_Reactive"));

            mActivated = false;
        }

        public void OnNeighborMatched()
        {
            if (mActivated)
            {
                mOwner.Clear();
            }
            else
            {
                mActivated = true;

                mOwner.SetSprite(HexBlast.Atlas.GetSprite($"PegTop_Unlock_Reactive"));
            }
        }
    }

    static class ReactiveBlockUtil
    {
        public static IReactiveBehavior CreateReactiveBehavior(HexBlock owner, ReactiveType reactiveType)
        {
            switch (reactiveType)
            {
                case ReactiveType.PegTop:
                    return new PegTop(owner);
                case ReactiveType.Normal:
                default:
                    return new NoneReactive();
            }
        }
    }
}
