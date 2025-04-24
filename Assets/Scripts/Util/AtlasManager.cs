using UnityEngine.U2D;
using UnityEngine;

namespace HexBlast
{
    class AtlasManager
    {
        SpriteAtlas mAtlas;
        public void Init()
        {
            mAtlas = Resources.Load<SpriteAtlas>("Atlas/Blocks");
        }

        public Sprite GetSprite(string name)
        {
            return mAtlas.GetSprite(name);
        }
    }
}