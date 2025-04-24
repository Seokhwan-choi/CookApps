using UnityEngine;

namespace HexBlast
{
    class ObjectPool
    {
        ObjectPoolManager mObjPool;

        public ObjectPool()
        {
            mObjPool = new ObjectPoolManager((new GameObject("ObjectPool")).transform);
        }

        public GameObject AcquireObject(string prefabPath, Transform parent = null)
        {
            return mObjPool.Acquire($"Prefabs/{prefabPath}", parent);
        }

        public void ReleaseObject(GameObject go)
        {
            mObjPool.Release(go);
        }

        public void Clear(bool obj = true, bool ui = true)
        {
            if (obj)
                mObjPool.DestroyAllObjects();

            Resources.UnloadUnusedAssets();
        }
    }
}