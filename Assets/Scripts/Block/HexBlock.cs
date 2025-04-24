using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace HexBlast
{
    enum SpecialType
    {
        Normal,
        Rocket,     // 로켓 - 한 라인 블럭 모두 삭제
        UFO,        // UFO - 특정 색상 모두 삭제
        Boomerang,  // 부메랑 - 특정 블럭 삭제
        TNT,        // TNT - 주변 블럭 삭제
        Turtle,     // 거북이 - 주변 블럭과 전방향 라인 모두 삭제
        
    }

    enum ReactiveType
    {
        Normal,
        PegTop,     // 팽이 - 주변 블럭 매칭되면 활성화 & 파괴
    }

    enum BlockColor
    {
        Blue,
        Green,
        Orange,
        Purple,
        Red,
        Yellow,

        Count,
        None,
    }

    class HexBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        int mX, mY, mZ;             // 육각형 큐브 좌표계 ( x,y,z / x + y + z = 0 항상 만족 )
        SpecialType mSpecialType;   // 특수 블럭 구분
        ReactiveType mReactiveType; // 반응형 블럭 구분
        BlockColor mColor;          // 블록의 색을 저장
        ISpecialBehavior mSaveSpecial = new NoneSpecial();  // 특수 블럭이 매치되면서 다시 특수 블럭이 생성될 수 있다. 저장용
        ISpecialBehavior mSpecial = new NoneSpecial();      // 특수 블럭 동작 정의
        IReactiveBehavior mReactive = new NoneReactive();   // 반응형 블럭 동작 정의
        public Vector3Int XYZ => new Vector3Int(mX, mY, mZ);
        public BlockColor Color => mColor;
        public bool IsEmpty => mColor == BlockColor.None;
        public bool IsReactive => mReactiveType != ReactiveType.Normal;
        public bool IsSpecial => SpecialType != SpecialType.Normal;
        public SpecialType SpecialType => mSpecialType;
        public ReactiveType ReactiveType => mReactiveType;
        public void Init(Vector3Int xyz, BlockColor color)
        {
            SetXYZ(xyz);
            SetWorldPos(GetWorldPos());
            SetColor(color);

            mSpecialType = SpecialType.Normal;
            mReactiveType = ReactiveType.Normal;

            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        public Vector3 GetWorldPos()
        {
            return HexUtil.CubeToWorld(mX, mZ);
        }

        public void SetWorldPos(Vector3 pos)
        {
            transform.position = pos;
        }

        public void ActivateSpecial(HexBlock swap = null, List < HexBlock> removes = null)
        {
            mSpecial.Activate(this, swap, removes);
        }

        public void NofifyMatchedNeighbor()
        {
            mReactive.OnNeighborMatched();
        }

        public void SetSpecial(SpecialType specialType)
        {
            if (mSpecialType == SpecialType.Normal)
            {
                mSpecialType = specialType;

                mSpecial = SpecialBlockUtil.CreateSpecialBehavior(this, specialType);

                if (specialType == SpecialType.UFO)
                {
                    SetSprite(HexBlast.Atlas.GetSprite($"UFO_Special"));
                }
                else
                {
                    SetSprite(HexBlast.Atlas.GetSprite($"{mColor}_{specialType}_Special"));
                }
            }
            else
            {
                mSaveSpecial = SpecialBlockUtil.CreateSpecialBehavior(this, specialType);
            }
        }

        public void SetReactive(ReactiveType reactiveType)
        {
            mReactiveType = reactiveType;

            mReactive = ReactiveBlockUtil.CreateReactiveBehavior(this, reactiveType);
        }

        public void SetXYZ(Vector3Int xyz)
        {
            mX = xyz.x;
            mY = xyz.y;
            mZ = xyz.z;

            gameObject.name = $"Hex ({xyz.x},{xyz.y},{xyz.z})";
        }

        public void OnRemove(HexBlock swap = null, List < HexBlock> removes = null)
        {
            if (IsSpecial)
            {
                ActivateSpecial(swap, removes);
            }
            else if (IsReactive)
            {
                mReactive.OnNeighborMatched();
            }
            else
            {
                Clear();
            }
        }

        public void Clear()
        {
            HexBlast.GameManager.OnClearBlock(this);

            SetColor(BlockColor.None);

            mSpecialType = SpecialType.Normal;
            mReactiveType = ReactiveType.Normal;
            mReactive = new NoneReactive();

            // 뭔가 저장되어 있던 특수 블럭 능력이 있다
            if (mSaveSpecial is not NoneSpecial)
            {
                mSpecialType = SpecialBlockUtil.ChangeToSpecialType(mSaveSpecial);

                SetSpecial(mSpecialType);

                mSaveSpecial = new NoneSpecial();
            }
            else
            {
                mSpecial = new NoneSpecial();
            }
        }

        public void SetColor(BlockColor color)
        {
            mColor = color;

            SetSprite(color != BlockColor.None ? HexBlast.Atlas.GetSprite($"{color}_normal") : null);
        }

        public void SetSprite(Sprite sprite)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = sprite;
        }

        public void OnBeginDrag(PointerEventData eventData) { }
        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData)
        {
            var mainCamera = Camera.main;

            // 드롭 위치에서 레이로 충돌 감지
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);

            Vector2 touchWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
            
            RaycastHit2D hit = Physics2D.Raycast(touchWorldPos, ray.direction);
            if (hit.collider != null && hit.collider.TryGetComponent(out HexBlock target))
            {
                if (target != null && target != this)
                {
                    HexBlast.GameManager.TrySwap(this, target);
                    return;
                }
            }
        }

        public Tween MoveToPos(Vector3 pos, float duration = 0.15f)
        {
            transform.DOKill();

            return transform.DOMove(pos, duration).SetEase(Ease.OutQuad);
        }
    }
}