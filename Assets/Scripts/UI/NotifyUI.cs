using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

namespace HexBlast
{
    enum NotifyType
    {
        Shuffle,
        StageClear,
        GameOver,
    }

    class NotifyUI : MonoBehaviour
    {
        TextMeshProUGUI mTextNotifyMessage;
        public void Init()
        {
            mTextNotifyMessage = gameObject.FindComponent<TextMeshProUGUI>("Text_Message");
            var buttonRestart = GetComponent<Button>();
            buttonRestart.onClick.RemoveAllListeners();
            buttonRestart.onClick.AddListener(() =>
            {
                HideMotion();
                HexBlast.GameManager.RestartStage();
            });
        }

        public void Show(NotifyType notifyType)
        {
            if (notifyType == NotifyType.Shuffle)
            {
                mTextNotifyMessage.text = "Shuffle!";

                ShowMotion(HideMotion);
            }
            else if (notifyType == NotifyType.GameOver)
            {
                mTextNotifyMessage.text = "GameOver...";

                ShowMotion();
            }
            else
            {
                mTextNotifyMessage.text = "StageClear!";

                ShowMotion();
            }
        }

        void ShowMotion(Action onComplete = null)
        {
            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        void HideMotion()
        {
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
        }
    }
}