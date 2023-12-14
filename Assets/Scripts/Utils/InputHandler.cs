using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : Singleton<InputHandler>
{
    private enum ButtonIndices
    {
        Interact_Confirm,
        Cancel,
        Menu,
        LightAttack,
        HeavyAttack,
        Jump,
        Roll,
        NextMode,
        PrevMode
    }

    public Vector2 Direction
    {
        get;
        private set;
    }

    public ButtonState Interact_Confirm => buttons[(int)ButtonIndices.Interact_Confirm];
    public ButtonState Cancel => buttons[(int)ButtonIndices.Cancel];
    public ButtonState Menu => buttons[(int)ButtonIndices.Menu];
    public ButtonState LightAttack => buttons[(int)ButtonIndices.LightAttack];
    public ButtonState HeavyAttack => buttons[(int)ButtonIndices.HeavyAttack];
    public ButtonState Jump => buttons[(int)ButtonIndices.Jump];
    public ButtonState Roll => buttons[(int)ButtonIndices.Roll];
    public ButtonState NextMode => buttons[(int)ButtonIndices.NextMode];
    public ButtonState PrevMode => buttons[(int)ButtonIndices.PrevMode];

    private int buttonCount = -1; //Size of ButtonIndices enum
    [SerializeField] private short bufferFrames = 5;
    [SerializeField] private bool bufferEnabled = false;
    private short IDSRC = 0;
    private ButtonState[] buttons;
    private Queue<Dictionary<short, short>> inputBuffer = new Queue<Dictionary<short, short>>();
    private Dictionary<short, short> currentFrame;

    public void Start()
    {
        buttonCount = System.Enum.GetValues(typeof(ButtonIndices)).Length;

        buttons = new ButtonState[buttonCount];
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Init(ref IDSRC, this);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < buttonCount; i++)
            buttons[i].Reset();

        if (bufferEnabled)
        {
            UpdateBuffer();
        }
    }

    //Input functions
    public void CTX_Direction(InputAction.CallbackContext _ctx)
    {
        Direction = _ctx.ReadValue<Vector2>();
    }

    public void CTX_Interact_Or_Confirm(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Interact_Confirm].Set(_ctx);
    }
    public void CTX_Cancel(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Cancel].Set(_ctx);
    }
    public void CTX_Menu(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Menu].Set(_ctx);
    }

    public void CTX_LightAttack(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.LightAttack].Set(_ctx);
    }
    public void CTX_HeavyAttack(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.HeavyAttack].Set(_ctx);
    }
    public void CTX_Jump(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Jump].Set(_ctx);
    }

    public void CTX_Roll(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.Roll].Set(_ctx);
    }

    public void CTX_NextMode(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.NextMode].Set(_ctx);
    }

    public void CTX_PrevMode(InputAction.CallbackContext _ctx)
    {
        buttons[(int)ButtonIndices.PrevMode].Set(_ctx);
    }

    //public struct AnalogToDigitalButtonState
    //{
    //    private const float cutoff = 0.75f;

    //    private bool firstFrame;
    //    public bool Holding { get; private set; }

    //    public readonly bool Down => (Holding && firstFrame);
    //    public readonly bool Up => (!Holding && firstFrame);

    //    public void SetIsHolding(float _value)
    //    {
    //        bool _holding = (_value >= cutoff);

    //        if (Holding == _holding)
    //        {
    //            //Same state
    //            firstFrame = false;
    //            return;
    //        }

    //        //New state
    //        Holding = _holding;
    //        firstFrame = true;
    //    }

    //    public void Reset()
    //    {
    //        firstFrame = false;
    //    }
    //}

    //Buffer Functions
    public void FlushBuffer()
    {
        inputBuffer.Clear();
    }

    public void UpdateBuffer()
    {
        if (inputBuffer.Count >= bufferFrames)
            inputBuffer.Dequeue();
        currentFrame = new Dictionary<short, short>();
        inputBuffer.Enqueue(currentFrame);
    }

    public void PrintBuffer()
    {
        string bufferData = $"InputBuffer: count-{inputBuffer.Count}";
        foreach (var frame in inputBuffer)
            if (frame.Count > 0)
                bufferData += $"\n{frame.Count}";
        Debug.Log(bufferData);
    }

    public struct ButtonState
    {
        private short id;
        private static short STATE_PRESSED = 0,
                                STATE_RELEASED = 1;
        private InputHandler handler;
        private bool firstFrame;
        public bool Holding
        {
            get;
            private set;
        }
        public bool Pressed
        {
            get
            {
                if (handler.bufferEnabled && handler.inputBuffer != null)
                {
                    foreach (var frame in handler.inputBuffer)
                    {
                        if (frame.ContainsKey(id) && frame[id] == STATE_PRESSED)
                        {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return Holding && firstFrame;
            }
        }

        public bool Released
        {
            get
            {
                if (handler.bufferEnabled && handler.inputBuffer != null)
                {
                    foreach (var frame in handler.inputBuffer)
                    {
                        if (frame.ContainsKey(id) && frame[id] == STATE_RELEASED)
                        {
                            return frame.Remove(id);
                        }
                    }
                    return false;
                }
                return !Holding && firstFrame;
            }
        }

        public void Set(InputAction.CallbackContext ctx)
        {
            Holding = !ctx.canceled;
            firstFrame = true;

            if (handler.bufferEnabled && handler.currentFrame != null)
            {
                handler.currentFrame.TryAdd(id, Holding ? STATE_PRESSED : STATE_RELEASED);
            }
        }

        public void Reset()
        {
            firstFrame = false;
        }

        public void Init(ref short IDSRC, InputHandler handler)
        {
            id = IDSRC++;
            this.handler = handler;
        }
    }
}