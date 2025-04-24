using UnityEngine;

namespace HexBlast
{
    static class Util
    {
        public static GameObject Find(this GameObject go, string name, bool includeinactive = false)
        {
            if (go != null)
            {
                var tmList = go.GetComponentsInChildren<Transform>(includeinactive);
                foreach (var tm in tmList)
                {
                    if (tm.name == name)
                        return tm.gameObject;
                }
            }

            return null;
        }

        public static GameObject FindGameObject(this GameObject go, string name, bool ignoreAssert = false)
        {
            GameObject find = go.Find(name, true);

            if (ignoreAssert == false)
                Debug.Assert(find != null, $"{go.name}�� {name} GameObject�� �������� ����");

            return find;
        }

        public static T FindComponent<T>(this GameObject go, string name, bool ignoreAssert = false)
        {
            GameObject componentObj = go.FindGameObject(name, ignoreAssert);

            T component = componentObj.GetComponent<T>();

            if (ignoreAssert == false)
            {
                Debug.Assert(component != null, $"{go.name}�� {typeof(T).Name} {name}�� �������� ����");
            }

            return component;
        }
    }
}


