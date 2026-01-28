using System;
using System.Collections;
using UnityEngine;

namespace UI.Canvas.MenuSelection
{
    public class MenuSelectionFlow
    {
        public event Action MapPanelShown;
        public event Action MapPanelHidden;
        public event Action CarPanelShown;
        public event Action OpponentPanelShown;
        public event Action OpponentPanelReturnedFromRace;

        private readonly MenuSelectionWindow _window;

        private RectTransform _mainPanelTransform;
        private RectTransform _mapPanelTransform;
        private RectTransform _carPanelTransform;
        private RectTransform _racePanelTransform;
        private RectTransform _opponentPanelTransform;

        private Vector2 _mainPanelBasePosition;
        private Vector2 _mapPanelBasePosition;
        private Vector2 _carPanelBasePosition;
        private Vector2 _racePanelBasePosition;
        private Vector2 _opponentPanelBasePosition;

        private Coroutine _mainPanelSlideRoutine;
        private Coroutine _mapPanelSlideRoutine;
        private Coroutine _carPanelSlideRoutine;
        private Coroutine _racePanelSlideRoutine;
        private Coroutine _opponentPanelSlideRoutine;
        private Coroutine _returnFromRaceRoutine;
        private Coroutine _opponentPanelEntryRoutine;
        private Coroutine _racePanelEntryRoutine;
        private Coroutine _mapPanelReturnRoutine;

        private bool _isReady;

        public MenuSelectionFlow(MenuSelectionWindow window)
        {
            _window = window;
        }

        public void Initialize()
        {
            CachePanelTransforms();
            if (!_isReady)
                return;
            ResetPanelPositions();
        }

        public void ShowMainPanel()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(false);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.OpponentSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();

            ResetPanelPositions();
            SetMenuButtonsInteractable(true);
        }

        public void ShowMainPanelFromMap()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(false);

            SetMenuButtonsInteractable(true);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition - new Vector2(0f, _window.MapSlideOffset),
                ref _mapPanelSlideRoutine, true);

            MapPanelHidden?.Invoke();
        }

        public void ShowMainPanelFromCar()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(false);

            SetMenuButtonsInteractable(true);
            SlidePanel(_carPanelTransform, _carPanelBasePosition + new Vector2(0f, _window.CarSlideOffset),
                ref _carPanelSlideRoutine, true);
        }

        public void ShowMapPanel()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(false);

            ResetPanelPositions();
            SetMenuButtonsInteractable(false);

            _mapPanelTransform.anchoredPosition = _mapPanelBasePosition - new Vector2(0f, _window.MapSlideOffset);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);

            MapPanelShown?.Invoke();
        }

        public void ShowCarPanel()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(false);
            _window.CarSelectionView.SetPanelActive(true);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(false);

            SetMenuButtonsInteractable(false);

            _carPanelTransform.anchoredPosition = _carPanelBasePosition + new Vector2(0f, _window.CarSlideOffset);
            SlidePanel(_carPanelTransform, _carPanelBasePosition, ref _carPanelSlideRoutine);

            CarPanelShown?.Invoke();
        }

        public void ShowOpponentPanel()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(true);

            SetMenuButtonsInteractable(false);
            MapPanelHidden?.Invoke();

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_window.MenuLeftShift, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_window.MenuLeftShift, 0f),
                ref _mapPanelSlideRoutine);

            _window.OpponentSelectionView.SetPanelActive(false);

            if (_opponentPanelEntryRoutine != null)
                _window.StopCoroutine(_opponentPanelEntryRoutine);

            _opponentPanelEntryRoutine = _window.StartCoroutine(ShowOpponentAfterMapSlide());
        }

        public void ShowRacePanelFromOpponent()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(true);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(true);

            SetMenuButtonsInteractable(false);

            _racePanelTransform.anchoredPosition = _racePanelBasePosition + new Vector2(_window.RaceSlideOffset, 0f);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_window.MenuLeftShift * 2f, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_window.MenuLeftShift * 2f, 0f),
                ref _mapPanelSlideRoutine);
            SlidePanel(_opponentPanelTransform, _opponentPanelBasePosition + new Vector2(-_window.MenuLeftShift, 0f),
                ref _opponentPanelSlideRoutine);

            if (_racePanelEntryRoutine != null)
                _window.StopCoroutine(_racePanelEntryRoutine);

            _racePanelEntryRoutine = _window.StartCoroutine(ShowRaceAfterOpponentShift());
        }

        public void ShowMapPanelFromOpponent()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();

            SetMenuButtonsInteractable(false);

            SlidePanel(_opponentPanelTransform, _opponentPanelBasePosition + new Vector2(0f, _window.OpponentSlideOffset),
                ref _opponentPanelSlideRoutine, true);

            if (_mapPanelReturnRoutine != null)
                _window.StopCoroutine(_mapPanelReturnRoutine);

            _mapPanelReturnRoutine = _window.StartCoroutine(ReturnMapAfterOpponentExit());
        }

        public void ShowOpponentPanelFromRace()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(true);
            _window.CarSelectionView.SetPanelActive(false);
            _window.SettingsPanelController.HideInstantly();
            _window.OpponentSelectionView.SetPanelActive(true);

            SetMenuButtonsInteractable(false);
            MapPanelHidden?.Invoke();

            if (_returnFromRaceRoutine != null)
                _window.StopCoroutine(_returnFromRaceRoutine);

            _returnFromRaceRoutine = _window.StartCoroutine(ReturnFromRaceRoutine());
        }

        public void ShowSettingsPanel()
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetPanelActive(true);
            _window.MapSelectionView.SetPanelActive(false);
            _window.CarSelectionView.SetPanelActive(false);
            _window.RaceSelectionView.SetPanelActive(false);
            _window.OpponentSelectionView.SetPanelActive(false);

            ResetPanelPositions();
            SetMenuButtonsInteractable(false);
            _window.SettingsPanelController.OpenSettings();
        }

        public void SetMenuButtonsInteractable(bool isInteractable)
        {
            if (!_isReady)
                return;

            _window.MainMenuView.SetButtonsInteractable(isInteractable);
        }

        private void CachePanelTransforms()
        {
            _mainPanelTransform = _window.MainMenuView.PanelTransform;
            _mapPanelTransform = _window.MapSelectionView.PanelTransform;
            _carPanelTransform = _window.CarSelectionView.PanelTransform;
            _racePanelTransform = _window.RaceSelectionView.PanelTransform;
            _opponentPanelTransform = _window.OpponentSelectionView.PanelTransform;

            if (_mainPanelTransform == null
                || _mapPanelTransform == null
                || _carPanelTransform == null
                || _racePanelTransform == null
                || _opponentPanelTransform == null)
            {
                _isReady = false;
                return;
            }

            _mainPanelBasePosition = _mainPanelTransform.anchoredPosition;
            _mapPanelBasePosition = _mapPanelTransform.anchoredPosition;
            _carPanelBasePosition = _carPanelTransform.anchoredPosition;
            _racePanelBasePosition = _racePanelTransform.anchoredPosition;
            _opponentPanelBasePosition = _opponentPanelTransform.anchoredPosition;

            _isReady = true;
        }

        private void ResetPanelPositions()
        {
            SetPanelPosition(_mainPanelTransform, _mainPanelBasePosition, ref _mainPanelSlideRoutine);
            SetPanelPosition(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);
            SetPanelPosition(_carPanelTransform, _carPanelBasePosition, ref _carPanelSlideRoutine);
            SetPanelPosition(_racePanelTransform, _racePanelBasePosition, ref _racePanelSlideRoutine);
            SetPanelPosition(_opponentPanelTransform, _opponentPanelBasePosition, ref _opponentPanelSlideRoutine);
        }

        private void SetPanelPosition(RectTransform rect, Vector2 position, ref Coroutine routine)
        {
            if (routine != null)
            {
                _window.StopCoroutine(routine);
                routine = null;
            }

            rect.anchoredPosition = position;
        }

        private void SlidePanel(RectTransform rect, Vector2 target, ref Coroutine routine, bool deactivateOnComplete = false)
        {
            if (routine != null)
                _window.StopCoroutine(routine);

            routine = _window.StartCoroutine(SlidePanelRoutine(rect, rect.anchoredPosition, target, deactivateOnComplete));
        }

        private IEnumerator SlidePanelRoutine(RectTransform rect, Vector2 from, Vector2 to, bool deactivateOnComplete)
        {
            var duration = Mathf.Max(0.01f, _window.PanelSlideDuration);
            var ease = _window.PanelSlideEase != null && _window.PanelSlideEase.length > 0
                ? _window.PanelSlideEase
                : AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var eased = ease.Evaluate(t);
                rect.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            rect.anchoredPosition = to;

            if (deactivateOnComplete)
                rect.gameObject.SetActive(false);
        }

        private IEnumerator ShowOpponentAfterMapSlide()
        {
            var delay = Mathf.Max(0f, _window.OpponentPanelDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            _window.OpponentSelectionView.SetPanelActive(true);
            _opponentPanelTransform.anchoredPosition = _opponentPanelBasePosition + new Vector2(0f, _window.OpponentSlideOffset);
            SlidePanel(_opponentPanelTransform, _opponentPanelBasePosition, ref _opponentPanelSlideRoutine);

            OpponentPanelShown?.Invoke();
        }

        private IEnumerator ReturnMapAfterOpponentExit()
        {
            yield return new WaitForSeconds(_window.PanelSlideDuration);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition, ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition, ref _mapPanelSlideRoutine);

            MapPanelShown?.Invoke();
        }

        private IEnumerator ShowRaceAfterOpponentShift()
        {
            var delay = Mathf.Max(0f, _window.RacePanelDelay);
            if (delay > 0f)
                yield return new WaitForSeconds(delay);

            SlidePanel(_racePanelTransform, _racePanelBasePosition, ref _racePanelSlideRoutine);
        }

        private IEnumerator ReturnFromRaceRoutine()
        {
            SlidePanel(_racePanelTransform, _racePanelBasePosition + new Vector2(_window.RaceSlideOffset, 0f),
                ref _racePanelSlideRoutine, true);

            yield return new WaitForSeconds(_window.PanelSlideDuration);

            SlidePanel(_mainPanelTransform, _mainPanelBasePosition + new Vector2(-_window.MenuLeftShift, 0f),
                ref _mainPanelSlideRoutine);
            SlidePanel(_mapPanelTransform, _mapPanelBasePosition + new Vector2(-_window.MenuLeftShift, 0f),
                ref _mapPanelSlideRoutine);
            SlidePanel(_opponentPanelTransform, _opponentPanelBasePosition, ref _opponentPanelSlideRoutine);

            OpponentPanelReturnedFromRace?.Invoke();
        }
    }
}
