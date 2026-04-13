using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Common.Manager;

namespace UI.Popup
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform = null;
        [SerializeField] private Transform pivot = null;
        [SerializeField] private Image effImage = null;
        [SerializeField] private Image image = null;
        [SerializeField] private Image shadow = null;
        [SerializeField] private Image m = null;
        [SerializeField] private GameObject lockObj = null;

        [SerializeField] private GameObject fireObj = null;
        [SerializeField] private GameObject fire2 = null;
        [SerializeField] private GameObject fire4 = null;
        [SerializeField] private GameObject fire10 = null;
        [SerializeField] private GameObject fire20 = null;

        [SerializeField] private Button button = null;
        [SerializeField] private AudioClip hitSound = null;
        [SerializeField] private AudioClip notHitSound = null;
        [SerializeField] private AudioClip dragSound = null;
        [SerializeField] private AudioClip drawSound = null;

        public RectTransform Rect
        {
            get => rectTransform;
        }

        private Action onClick = null;

        public Action OnClick
        {
            get => onClick;
            set
            {
                onClick = value;
                if(button != null)
                    button.enabled = onClick != null;
                image.raycastTarget = onClick != null;
            }
        }
        
        public NxCard NXCard { get; set; } = null;

        private float flipDuration = 0.1f;
        private bool isFront = false;
        private Ease moveEase = Ease.InOutCubic;
        private Ease hitEase = Ease.Linear;
        public bool IsHit { get; set; } = false;

        private Sequence sequence = null;
        private Func<int, bool> isMissionCardFunc = null;

        public void OnDisable()
        {
            Clear();
        }

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                OnClick?.Invoke();
            });
        }

        public void Clear()
        {
            isMissionCardFunc = null;
            if (sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }
            effImage.gameObject.SetActive(false);
            SetBackside();
            OnClick = null;
            IsHit = false;
            lockObj.SetActive(false);
            fireObj.SetActive(false);
            pivot.localScale = Vector3.one;
            SetDisable(false);
            m.gameObject.SetActive(false);
            DOTween.Kill(transform);           // transform 관련 트윈 전부 제거
            DOTween.Kill(pivot);
        }

        public bool IsPlaying
        {
            get
            {
                return sequence != null && sequence.IsActive() && sequence.IsPlaying();
            }
        }

        public void SetDisable(bool _flag)
        {
            image.color = _flag ? Color.gray * 1.5f : Color.white;
            m.color = _flag ? Color.gray * 1.5f : Color.white;
        }

        public void OnHitEffect()
        {
            effImage.color = new Color(1, 1, 1, 0.23f);
            effImage.gameObject.SetActive(true);
            effImage.DOFade(0, 0.1f).OnComplete(()=>
            {
                effImage.gameObject.SetActive(false);
            });
        }

        public void Lock()
        {
            lockObj.SetActive(true);
        }

        public void SetCard(int _sn, bool _scale = true)
        {
            if(_scale)
                pivot.localScale = new(-1, 1, 1);
            image.sprite = Resources.Load<Sprite>(string.Format("Image/Cards/Card{0:D2}", _sn + 1));
            isFront = true;
            UpdateShadowPos();
        }

        private void UpdateShadowPos()
        {
            shadow.transform.localPosition = new(pivot.localScale.x * 3.2f, -3.2f, 0);
        }

        public void Initialize(Func<int, bool> _isMissionCardFunc, int _multiple)
        {
            isMissionCardFunc = _isMissionCardFunc;
            SetFire(_multiple);
        }

        public void SetFire(int _multiple)
        {
            if (isFront)
                return;

            fireObj.SetActive(_multiple > 1);

            fire2.SetActive(_multiple == 2);
            fire4.SetActive(_multiple == 4);
            fire10.SetActive(_multiple == 10);
            fire20.SetActive(_multiple == 20);

            //if (_multiple > 1)
            //{
            //    fireText.text = $"x{_multiple}";
            //}
        }

        public void SetBackside()
        {
            m.gameObject.SetActive(false);
            pivot.localScale = Vector3.one;
            lockObj.SetActive(false);
            image.sprite = Resources.Load<Sprite>("Image/Cards/Card52");
            isFront = false;
            UpdateShadowPos();
        }

        public void RefreshMission()
        {
            if(isFront && isMissionCardFunc != null)
            {
                m.gameObject.SetActive(isMissionCardFunc.Invoke(NXCard.Sn));
            }
        }

        public void RefreshMission(bool _flag)
        {
            if (isFront)
            {
                m.gameObject.SetActive(_flag);
            }
        }

        public Tween FlipCard(int _sn, bool _isMission = false)
        {
            if (_sn == -1 || isFront)
            {
                return null; // 아무 동작도 하지 않을 경우 `null` 반환
            }

            // 카드가 반으로 접히는 효과
            return pivot.DOScaleX(-0.2f, flipDuration * 0.2f).OnUpdate(()=>
            {
                UpdateShadowPos();
            })
            .OnComplete(() =>
            {
                UpdateShadowPos();
                if (isFront)
                {
                    SetBackside();
                }
                else
                {
                    fireObj.SetActive(false);
                    m.gameObject.SetActive(_isMission);
                    SetCard(_sn, false);
                }
                pivot.DOScaleX(-1, flipDuration * 0.8f).OnUpdate(() =>
                {
                    UpdateShadowPos();
                }).SetAutoKill(true);
            });
        }


        //카드 뒤집기, 카드 치기
        public void ScaleTweenMoveToTarget(Transform _parent, Transform _target, float _duration, float _delay = 0, bool _isHit = true, Action _onComplete = null)
        {
            CompleteTween();
            
            sequence = DOTween.Sequence();
            sequence.AppendInterval(_delay);
            sequence.AppendCallback(() =>
            {
                transform.SetParent(_parent);
            });
            sequence.Append(transform.DOScale(transform.localScale * 2f, _duration));
            sequence.Join(transform.DORotate(Vector3.zero, _duration));
            Tween flipTween = FlipCard(NXCard.Sn, isMissionCardFunc.Invoke(NXCard.Sn));
            if (flipTween != null)
            {
                sequence.Append(flipTween);
            }

            sequence.AppendInterval(0.1f);
            sequence.Append(transform.DOMove(_target.position, _duration).SetEase(Ease.InOutCubic));
            sequence.Join(transform.DOScale(transform.localScale * 2.4f, _duration * 0.3f).SetEase(Ease.InOutCubic));
            sequence.Join(transform.DOScale(_target.localScale, _duration * 0.7f).SetDelay(_duration * 0.3f).SetEase(Ease.Linear));
            sequence.Join(transform.DORotate(_target.eulerAngles, _duration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                if(_isHit)
                {
                    OnHitEffect();
                }
                SoundManager.Instance.PlayFX(_isHit ? hitSound : notHitSound);
                transform.SetParent(_target);
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                _onComplete?.Invoke();
            });
            sequence.SetAutoKill(true);
            sequence.Play();
        }

        //카드 뺏어오기
        public void TweenMoveToTarget(Transform _parent, Transform _target, float _duration, float _delay = 0, Action _onComplete = null)
        {
            CompleteTween();
            
            sequence = DOTween.Sequence();
            
            sequence.AppendInterval(_delay);
            sequence.AppendCallback(() =>
            {
                transform.SetParent(_parent);
            });
            Tween flipTween = FlipCard(NXCard.Sn, isMissionCardFunc.Invoke(NXCard.Sn));
            if (flipTween != null)
            {
                sequence.Append(flipTween);
            }
            sequence.Append(transform.DOMove(_target.position, _duration).OnStart(()=>
            {
                SoundManager.Instance.PlayFX(dragSound);
            }).SetEase(hitEase));
            sequence.Join(transform.DOScale(transform.localScale * 1.6f, _duration * 0.5f).SetEase(hitEase));
            sequence.Join(transform.DORotate(_target.eulerAngles, _duration).SetEase(Ease.InOutQuad));
            sequence.Append(transform.DOScale(_target.localScale, _duration * 0.5f).SetEase(hitEase));
            sequence.OnComplete(() =>
            {
                transform.SetParent(_target);
                transform.localEulerAngles = Vector3.zero;
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one;
                _onComplete?.Invoke();
            });


            sequence.SetAutoKill(true);
            sequence.Play();
        }

        public void TweenDrawToTarget(Transform _parent, Transform _target, float _duration, float _delay = 0, Action _onComplete = null)
        {
            CompleteTween();

            transform.SetParent(_parent);
            sequence = DOTween.Sequence();
            sequence.AppendInterval(_delay);
            
            Tween flipTween = FlipCard(NXCard.Sn);
            if (flipTween != null)
            {
                sequence.Append(flipTween);
            }
            sequence.Append(transform.DOMove(_target.position, _duration).OnStart(() =>
            {
                SoundManager.Instance.PlayFX(drawSound, _overlap: false);
            }).SetEase(moveEase));
            sequence.Join(transform.DOScale(_target.localScale, _duration).SetEase(moveEase));
            sequence.Join(transform.DORotate(_target.eulerAngles, _duration).SetEase(Ease.InOutQuad));
            sequence.OnComplete(() =>
            {
                transform.SetParent(_target);
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                transform.localScale = Vector3.one;
                _onComplete?.Invoke();
            });
            sequence.SetAutoKill(true);
            sequence.Play();
        }

        public void TweenLocalMoveToTarget(Transform _target, float _duration, float _delay = 0, Action _onUpdate = null, Action _onComplete = null)
        {
            CompleteTween();

            sequence = DOTween.Sequence();
            sequence.AppendInterval(_delay);
            sequence.OnUpdate(() =>
            {
                _onUpdate?.Invoke();
            });
            sequence.AppendCallback(() =>
            {
                transform.SetParent(_target);
                transform.SetAsFirstSibling();
            });
            sequence.Append(transform.DOLocalMove(Vector3.zero, _duration).SetEase(Ease.InOutQuad));
            sequence.Join(transform.DOScale(Vector3.one, _duration).SetEase(Ease.InOutQuad));
            //sequence.Join(transform.DOLocalRotate(Vector3.zero, _duration).SetEase(Ease.InOutQuad));

            sequence.OnComplete(() =>
            {
                transform.SetParent(_target);
                transform.SetAsFirstSibling();
                transform.localPosition = Vector3.zero;
                //transform.localEulerAngles = Vector3.zero;
                transform.localScale = Vector3.one;
                _onComplete?.Invoke();
            });
            sequence.SetAutoKill(true);
            sequence.Play();
        }

        public void CompleteTween()
        {
            if (sequence != null && sequence.IsActive())
            {
                sequence.Complete(true); // 모든 트윈 강제 완료
                sequence.Kill();         // 트윈 제거
                sequence = null;         // 새 트윈을 만들 수 있도록 초기화
            }
        }
    }
}