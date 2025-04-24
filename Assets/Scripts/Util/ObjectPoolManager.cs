using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HexBlast
{
    class ObjectPoolManager
    {
        public class PoolEntry
        {
            public GameObject Prefab;
            public Stack<GameObject> Items = new Stack<GameObject>();
        }

        int mUniqueId;

        Dictionary<string, PoolEntry> mPathToPool;
        Dictionary<GameObject, PoolEntry> mActiveObjects;

        Transform mContainer;

        public ObjectPoolManager(Transform container)
        {
            mUniqueId = 100;
            mContainer = container;

            mPathToPool = new Dictionary<string, PoolEntry>();
            mActiveObjects = new Dictionary<GameObject, PoolEntry>();
        }

        public GameObject Acquire(string prefabPath, Transform parent = null)
        {
            // 이전에 사용했던 pool이 있는지 확인
            mPathToPool.TryGetValue(prefabPath, out PoolEntry pool);
            if (pool == null)
            {
                // 사용했더 pool이 없으면 새롭게 생성
                GameObject prefabObj = Resources.Load(prefabPath) as GameObject;
                if (prefabObj == null)
                    return null;

                pool = new PoolEntry() { Prefab = prefabObj };

                mPathToPool.Add(prefabPath, pool);
            }

            return Acquire(pool, parent);
        }

        public GameObject Acquire(PoolEntry pool, Transform parent = null)
        {
            Debug.Assert(pool != null);

            GameObject go;

            // 풀에 담겨있는 오브젝트가 있다면
            if (pool.Items.Count > 0)
            {
                // 바로 꺼내서 사용
                go = pool.Items.Pop();
            }
            else
            {
                // 풀에 담겨져있는 오브젝트가 없다면 새로 생성해서사용
                go = GameObject.Instantiate<GameObject>(pool.Prefab, parent);

                go.name = $"{pool.Prefab.name}_{mUniqueId++}";
            }

            go.transform.SetParent(parent, false);

            go.SetActive(true);

            // 오브젝트를 활성화하고 어떤 풀에서 나왔는지 기억
            mActiveObjects[go] = pool;

            return go;
        }

        public void Release(GameObject go)
        {
            mActiveObjects.TryGetValue(go, out PoolEntry pool);
            if (pool != null)
            {
                // 반납
                go.transform.SetParent(mContainer, false);

                // 비활성화
                go.SetActive(false);

                // 풀에 다시 담아 놓고
                pool.Items.Push(go);

                // 활성화 목록에서 제거
                mActiveObjects.Remove(go);
            }
            else
            {
                Debug.LogError($"활성화 안된 오브젝트를 Release하려고 시도함. ({go.name})");
            }
        }

        public void DestroyAllObjects()
        {
            foreach (PoolEntry pool in mPathToPool.Values)
            {
                foreach (var go in pool.Items)
                {
                    if (go != null)
                        GameObject.Destroy(go);
                }

                pool.Items.Clear();
            }

            foreach (var item in mActiveObjects)
            {
                if (item.Key != null)
                    GameObject.Destroy(item.Key);
            }

            mPathToPool.Clear();
            mActiveObjects.Clear();
        }
    }
}