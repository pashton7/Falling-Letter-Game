using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Desktop_App;
public partial class Form1 : Form
{

    // Custom Form that listens for global key presses and triggers the OnAnyGlobalKeyPressed event.
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104; // For Alt key combinations

    private LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public Form1()
    {
        InitializeComponent();
        _proc = HookCallback;
        this.DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();

    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _hookID = SetHook(_proc); // Start listening globally
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        UnhookWindowsHookEx(_hookID); // Essential cleanup!
        base.OnFormClosing(e);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        // Check if it's a standard keydown or system keydown (Alt combinations)
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Keys pressedKey = (Keys)vkCode;

            // Safe cross-thread execution to handle the key on your UI thread
            this.BeginInvoke(new Action(() => {
                OnAnyGlobalKeyPressed(pressedKey);
            }));
        }

        // Pass the key message along so you don't freeze the user's computer input
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private void OnAnyGlobalKeyPressed(Keys key)
    {
        // --- YOUR CODE HERE ---
        // This will trigger for ANY key pressed, anywhere on the PC
        Console.WriteLine($"Key detected globally: {key}");
        MainGame.Form1_KeyPress(key.ToString());
    }

    // Windows API Imports
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
