using UnityEngine;

namespace HexBlast
{
    class GameInitializer : MonoBehaviour
    {
        private void Awake()
        {
            HexBlast.Init(gameObject);
        }
    }

    class HexBlast
    {
        static public ObjectPool ObjectPool;
        static public AtlasManager Atlas;
        static public MainUI MainUI;
        static public GameManager GameManager;

        static public void Init(GameObject go)
        {
            ObjectPool = new ObjectPool();

            Atlas = new AtlasManager();
            Atlas.Init();

            var canvasObj = GameObject.Find("MainCanvas");
            MainUI = canvasObj.GetComponent<MainUI>();
            MainUI.Init();

            GameManager = go.AddComponent<GameManager>();
            GameManager.Init();
        }
    }
}