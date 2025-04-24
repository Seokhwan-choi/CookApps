using UnityEngine;
using TMPro;

namespace HexBlast
{
    class MainUI : MonoBehaviour
    {
        TextMeshProUGUI mTextMission;
        TextMeshProUGUI mTextMove;
        TextMeshProUGUI mTextScore;

        NotifyUI mNotifyUI;
        public void Init()
        {
            mTextMission = gameObject.FindComponent<TextMeshProUGUI>("Text_Mission");
            mTextMove = gameObject.FindComponent<TextMeshProUGUI>("Text_MoveCount");
            mTextScore = gameObject.FindComponent<TextMeshProUGUI>("Text_Score");

            var notifyObj = gameObject.FindGameObject("Notify");

            mNotifyUI = notifyObj.AddComponent<NotifyUI>();
            mNotifyUI.Init();
        }

        public void SetMission(int mission)
        {
            mTextMission.text = $"{mission}";
        }

        public void SetMove(int move)
        {
            mTextMove.text = $"{move}";
        }

        public void SetScore(int score)
        {
            mTextScore.text = $"{score}";
        }

        public void ShowNotify(NotifyType type)
        {
            mNotifyUI.Show(type);
        }
    }
}