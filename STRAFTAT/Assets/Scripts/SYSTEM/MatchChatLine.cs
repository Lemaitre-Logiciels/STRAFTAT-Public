using System.Collections;
using System.Collections.Generic;
using TMPro;
using FishNet;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Goodgulf.Graphics
{
    public class MatchChatLine : MonoBehaviour {
        public float duration;
        public float fadeDuration;
        public bool deleteMe = false;
        public bool chatMsg = false;
        public bool destroyWhenDone = false;

        private bool startCounting = false;
        private bool startFading = false;
        private float currentTime = 0f;
        private float currentFadeTime = 0f;
        private float alpha;
        private float ialpha;
        private bool initialized;

        public float tweenSize = 1.01f;
        public Ease easeType;

        void Awake()
        {
            EnsureInitialized();
        }

        void Start()
        {
            EnsureInitialized();

            if (deleteMe) StartDuration();

            if (chatMsg) SoundManager.Instance.PlaySound(PauseManager.Instance.matchChatClip);

            transform.DOPunchScale(new Vector3(tweenSize,tweenSize,tweenSize), 0.3f).SetEase(easeType);
        }

        private void EnsureInitialized()
        {
            if (initialized) { return; }

            if (_canvasGroup != null)
            {
                ialpha = _canvasGroup.alpha;
            }
            else if (matchImg != null)
            {
                ialpha = matchImg.color.a;
            }
            else
            {
                ialpha = 1f;
            }

            alpha = ialpha;
            initialized = true;
        }

        [SerializeField] private TMP_Text matchLine;
        [SerializeField] private TMP_Text nameLine;
        [SerializeField] private RawImage matchImg;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public void StartDuration()
        {
            startCounting = true;
        }
        


        // Update is called once per frame
        void Update()
        {
            if (stop) { return; }

            if(!startCounting)
                return;
            
            currentTime += Time.deltaTime;
            if (currentTime > duration)
                startFading = true;

            if (startFading)
            {
                if (currentFadeTime < fadeDuration) {
                    alpha = Mathf.Lerp(ialpha, 0f, currentFadeTime / fadeDuration);
                    _canvasGroup.alpha = alpha;
                    currentFadeTime += Time.deltaTime;
                }
                else
                {
                    deleteMe = true;
                }
            }

            if (currentTime > duration + fadeDuration) {
                if (destroyWhenDone) { Destroy(gameObject); }
                gameObject.SetActive(false);
            }
        }

        private bool stop = false;
        
        public void ForceShow() {
            EnsureInitialized();
            stop = true;
            if (_canvasGroup != null) { _canvasGroup.alpha = ialpha; }
            gameObject.SetActive(true);
        }

        public void DisableForceShow() {
            EnsureInitialized();
            stop = false;
            if (_canvasGroup != null) { _canvasGroup.alpha = alpha; }
        }
    }
}
