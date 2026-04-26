using uk.vroad.api;
using uk.vroad.api.input;
using uk.vroad.rvr;
using uk.vroad.ucm;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace uk.vroad.urvr
{
    public class UMouseRvr : UaMouse
    {
        private const float SHOW_MOUSE_FOR_TIME_AFTER_MOVE = 2.0f;

        private Game game;
        private Vector2 prevPosPlayer;
        private bool wasDragPlayer;
        private bool wasStoppedNoMouse;
        private float mouseVisibleCountdown;

        protected override void Awake()
        {
            game = Game.AwakeInstance();
            base.Awake();
        }
        protected override App App() { return game; }

        protected override void Update()
        {
            if (Application.isFocused)
            {
                // If we have a gamepad, or are playing with just keyboard, we don't need a mouse cursor
                // so make the cursor visible only if it has been moved in the last SHOW_MOUSE_... seconds

                // bool mouseMoved = Input.GetAxis(SA.MOUSE_X) != 0 || Input.GetAxis(SA.MOUSE_Y) != 0;
                bool mouseMoved = Mouse.current != null && Mouse.current.wasUpdatedThisFrame;
                
                if (mouseMoved)
                {
                    if (!Cursor.visible) Cursor.visible = true;

                    mouseVisibleCountdown = SHOW_MOUSE_FOR_TIME_AFTER_MOVE;
                }
                else if (Cursor.visible)
                {
                    mouseVisibleCountdown -= Time.unscaledDeltaTime;
                    if (mouseVisibleCountdown < 0) Cursor.visible = false;
                }

            }
            else
            {
                Cursor.visible = true;
                mouseVisibleCountdown = SHOW_MOUSE_FOR_TIME_AFTER_MOVE;
            }
            
            base.Update();
        }
       
        protected override void HandleMouse(Mouse mouse, Keyboard kb)
        {
            base.HandleMouse(mouse, kb);
            
            ButtonControl msButtonPlayer = mouse.middleButton;
            AppInputHandler aih = App().Aih();

            if (msButtonPlayer.wasPressedThisFrame)
            {
                prevPosPlayer = mouse.position.ReadValue();
            }
            
            else if (msButtonPlayer.isPressed)
            {
                Vector2 currentPos =  mouse.position.ReadValue();
                float laneX = SCALE_MOUSE_DRAG * (currentPos.x - prevPosPlayer.x);
                float speedY = SCALE_MOUSE_DRAG * (currentPos.y - prevPosPlayer.y);

                if (game.Gsc().IsFFwd() && speedY > -0.1) speedY = 1f;
                
                aih.FireAnalogEvent(GameAnalogFn.Lane, laneX);
                aih.FireAnalogEvent(GameAnalogFn.Speed, speedY);
                wasDragPlayer = true;
                
                // As with stick, you need to release mouse after stopping and then drag down again to U-Turn
                if (wasStoppedNoMouse && speedY < 0)
                {
                    aih.FireDigitalEvent(GameDigitalFn.UTurnStopped, true);
                    wasStoppedNoMouse = false; // prevent a second immediate U-Turn
                }
                prevPosPlayer = currentPos;
            }
            else
            {
                wasStoppedNoMouse = game.Gsc().IsStop();

                if (wasDragPlayer)
                {
                    aih.FireAnalogEvent(GameAnalogFn.Lane, 0);
                    aih.FireAnalogEvent(GameAnalogFn.Speed, 0);
                }

                wasDragPlayer = false;
            }

        }
    }
}
