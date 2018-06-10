﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/06/07 21:49
// License Copyright (c) Daniele Giardini

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DG.De2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DeSpriteButton : MonoBehaviour
    {
        public enum TransitionType
        {
            ColorTint,
            BounceScale
        }

        enum State
        {
            Normal,
            Rollover,
            Press
        }

        #region EVENTS

        public event Action OnEditorRefreshRequired;
        void Dispatch_OnEditorRefreshRequired() { if (OnEditorRefreshRequired != null) OnEditorRefreshRequired(); }

        #endregion

        #region Serialized

        [SerializeField] bool _interactable = true;
        [SerializeField] TransitionType _transition;
        // Transition ► Scale
        [SerializeField] float _highlightedScaleFactor = 1.1f;
        [SerializeField] float _pressedScaleFactor = 1.2f;
        [SerializeField] float _disabledScaleFactor = 1f;
        [SerializeField] float _duration = 0.4f; // Ignored in case of ColorTint transition (who has it internally to ColorBlock)
        // Transition ► Color
        /// <summary>Only used in case of <see cref="TransitionType.ColorTint"/> transitions</summary>
        public ColorBlock colors = ColorBlock.defaultColorBlock;
        //
        [SerializeField] bool _showOnClick = true; // Editor-only
        [SerializeField] bool _showOnPress, _showOnRelease; // Editor-only
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onPress = new UnityEvent();
        public UnityEvent onRelease = new UnityEvent();

        #endregion

        // Extra non-serialized events
        public UnityEvent onEnter { get { if (_onEnter == null) _onEnter = new UnityEvent(); return _onEnter; } }
        public UnityEvent onExit { get { if (_onExit == null) _onExit = new UnityEvent(); return _onExit; } }
        UnityEvent _onEnter;
        UnityEvent _onExit;
        //
        public bool interactable { get { return _interactable; } set { SetInteractable(value); } }

        bool _initialized;
        State _state = State.Normal;
        SpriteRenderer _spriteR;
        Vector3 _defLocalScale;
        bool _isOver;
        bool _isDown;
        Coroutine _coTransitionTween;

        #region Unity + INIT

        void Init()
        {
            if (_initialized) return;

            _initialized = true;

            _spriteR = this.GetComponent<SpriteRenderer>();
            _defLocalScale = this.transform.localScale;
        }

        void OnEnable()
        {
            Refresh(true);
        }

        void Awake()
        {
            Init();
        }

        void Reset()
        {
            Dispatch_OnEditorRefreshRequired();
        }

        void OnMouseEnter()
        {
            _state = _isDown ? State.Press : State.Rollover;
            _isOver = true;
            if (!_interactable) return;

            Refresh();
            if (onEnter != null) onEnter.Invoke();
        }

        void OnMouseDown()
        {
            _state = State.Press;
            _isDown = true;
            if (!_interactable) return;

            Refresh();
            if (onPress != null) onPress.Invoke();
        }

        void OnMouseUpAsButton()
        {
            _state = State.Rollover;
            _isDown = false;
            if (!_interactable) return;

            Refresh();
            if (onClick != null) onClick.Invoke();
        }

        void OnMouseUp()
        {
            _state = _isOver ? State.Rollover : State.Normal;
            _isDown = false;
            if (!_interactable) return;

            Refresh();
            if (onRelease != null) onRelease.Invoke();
        }

        void OnMouseExit()
        {
            _state = _isDown ? State.Press : State.Normal;
            _isOver = false;
            if (!_interactable) return;

            Refresh();
            if (onExit != null) onExit.Invoke();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Force-refreshes the graphics of this button
        /// </summary>
        public void Refresh(bool immediate = false)
        {
            Init();

            if (!_interactable) {
                switch (_transition) {
                case TransitionType.BounceScale:
                    TweenScaleTo(_defLocalScale * _disabledScaleFactor, immediate ? -1 : 0.1f);
                    break;
                case TransitionType.ColorTint:
                    TweenColorTo(colors.disabledColor, immediate ? -1 : 0.1f);
                    break;
                }
            } else {
                switch (_state) {
                case State.Rollover:
                    switch (_transition) {
                    case TransitionType.BounceScale:
                        TweenScaleTo(_defLocalScale * _highlightedScaleFactor, immediate ? -1 : _duration, true);
                        break;
                    case TransitionType.ColorTint:
                        TweenColorTo(colors.highlightedColor, immediate ? -1 : colors.fadeDuration);
                        break;
                    }
                    break;
                case State.Press:
                    switch (_transition) {
                    case TransitionType.BounceScale:
                        TweenScaleTo(_defLocalScale * _pressedScaleFactor, -1);
                        break;
                    case TransitionType.ColorTint:
                        TweenColorTo(colors.pressedColor, -1);
                        break;
                    }
                    break;
                default:
                    switch (_transition) {
                    case TransitionType.BounceScale:
                        TweenScaleTo(_defLocalScale, immediate ? -1 : _duration);
                        break;
                    case TransitionType.ColorTint:
                        TweenColorTo(colors.normalColor, immediate ? -1 : colors.fadeDuration);
                        break;
                    }
                    break;
                }
            }
        }

        #endregion

        #region Methods

        void SetInteractable(bool value, bool immediate = false)
        {
            if (_interactable == value) return;

            _interactable = value;
            Refresh(immediate);
            if (_isOver && onExit != null) onExit.Invoke();
            if (_isDown && onRelease != null) onRelease.Invoke();
        }

        #region Tweens

        void TweenScaleTo(Vector3 scale, float duration, bool loop = false)
        {
            if (_coTransitionTween != null) {
                this.StopCoroutine(_coTransitionTween);
                _coTransitionTween = null;
            }
            if (duration <= 0) {
                this.transform.localScale = scale;
                return;
            }
            if (this.transform.localScale == scale) return;

            _coTransitionTween = this.StartCoroutine(CO_ScaleTo(scale, duration, loop));
        }

        IEnumerator CO_ScaleTo(Vector3 scale, float duration, bool loop = false)
        {
            Vector3 startScale = this.transform.localScale;
            float startTime = Time.realtimeSinceStartup;
            bool complete = false;
            while (!complete) {
                float elapsed = Time.realtimeSinceStartup - startTime;
                float elapsedPerc = elapsed / duration;
                if (elapsedPerc > 1) {
                    if (loop) {
                        int loopCount = (int)elapsedPerc;
                        elapsedPerc %= 1;
                        if (loopCount % 2 != 0) elapsedPerc = 1 - elapsedPerc; // Yoyo loop
                    } else elapsedPerc = 1;
                }
                this.transform.localScale = Vector3.Slerp(startScale, scale, elapsedPerc);
                if (!loop && elapsed > duration) complete = true;
                else yield return null;
            }
            _coTransitionTween = null;
        }

        void TweenColorTo(Color color, float duration)
        {
            if (_coTransitionTween != null) {
                this.StopCoroutine(_coTransitionTween);
                _coTransitionTween = null;
            }
            if (duration <= 0) {
                _spriteR.color = color;
                return;
            }
            if (_spriteR.color.Equals(color)) return;

            _coTransitionTween = this.StartCoroutine(CO_ColorTo(color, duration));
        }

        IEnumerator CO_ColorTo(Color color, float duration)
        {
            Color startColor = _spriteR.color;
            float startTime = Time.realtimeSinceStartup;
            bool complete = false;
            while (!complete) {
                float elapsed = Time.realtimeSinceStartup - startTime;
                float elapsedPerc = elapsed / duration;
                if (elapsedPerc > 1) elapsedPerc = 1;
                _spriteR.color = Color.Lerp(startColor, color, elapsedPerc);
                if (elapsed > duration) complete = true;
                else yield return null;
            }
            _coTransitionTween = null;
        }

        #endregion

        #endregion
    }
}