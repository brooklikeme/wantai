using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;	// For RegKey

namespace WanTai.View
{
    public class APIHelper
    {
        public enum HookType
        {
            Keyboard = 2,//键盘操作             
            CBT = 5,//窗口操作          
            Mouse = 7, //鼠标操作
        }
        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();//得到当前的线程ID        
        [DllImport("user32.dll")]
        static extern int GetDlgItem(IntPtr hDlg, int nIDDlgItem);//得到Dialog窗口的子项        
        [DllImport("user32", EntryPoint = "SetDlgItemText")]
        static extern int SetDlgItemTextA(IntPtr hDlg, int nIDDlgItem, string lpString);//设置Dialog窗口子项的文本     
        [DllImport("user32.dll")]
        static extern void UnhookWindowsHookEx(IntPtr handle);//解掉挂钩         
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, [MarshalAs(UnmanagedType.FunctionPtr)] HookProc lpfn, IntPtr hInstance, int threadID);//设置挂钩       
        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr handle, int code, IntPtr wparam, IntPtr lparam);//进行下一个挂钩，如果有的话 
        //三、定义局部变量
        static IntPtr _nextHookPtr;
        static HookProc myProc = new HookProc(MyHookProc);//must be global, or it will be Collected by GC, then no callback func can be used for the Hook
        delegate IntPtr HookProc(int code, IntPtr wparam, IntPtr lparam);
        static IntPtr MyHookProc(int code, IntPtr wparam, IntPtr lparam)
        {
            IntPtr hChildWnd;// msgbox is "child"             // notification that a window is about to be activated
            int result; hChildWnd = wparam;
            // window handle is wParam
            if (code == 5)//HCBT_ACTIVATE = 5
            {
                // set window handles of messagebox
               
                //to get the text of yes button             
               
                if (GetDlgItem(hChildWnd, 6) != 0)//IDYES = 6
                {
                    //result = SetDlgItemTextA(hChildWnd, 6, Properties.Resources.YES);//在Project.Resources里自定义文本
                    result = SetDlgItemTextA(hChildWnd, 6,"是");//在Project.Resources里自定义文本
                }
                if (GetDlgItem(hChildWnd, 7) != 0)//IDNO = 7
                {
                 //   result = SetDlgItemTextA(hChildWnd, 7, Properties.Resources.NO);
                    result = SetDlgItemTextA(hChildWnd, 7, "否");
                }
                if (GetDlgItem(hChildWnd, 7) != 0)//IDNO = 7
                {
                    //   result = SetDlgItemTextA(hChildWnd, 7, Properties.Resources.NO);
                    result = SetDlgItemTextA(hChildWnd, 7, "否");
                }
                
            }
            else
            {
                CallNextHookEx(_nextHookPtr, code, wparam, lparam);// otherwise, continue with any possible chained hooks
            } 
         
                //   result = SetDlgItemTextA(hChildWnd, 7, Properties.Resources.NO);
           //     result = SetDlgItemTextA(hChildWnd, 1, "确认");
            
 
            //return (IntPtr)1; //直接返回了，该消息就处理结束了
            return IntPtr.Zero;//返回，让后面的程序处理该消息       
        }
        //五、提供给外部调用的Hook方法，如在Form_Load时SetHook，Form_Closing时UnHook.
        public static void SetHook()
        {
            if (_nextHookPtr != IntPtr.Zero)//Hooked already
            {
                return;
            } _nextHookPtr = SetWindowsHookEx((int)HookType.CBT, myProc, IntPtr.Zero, GetCurrentThreadId());
        }
        public static void UnHook()
        {
            if (_nextHookPtr != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_nextHookPtr);
                _nextHookPtr = IntPtr.Zero;
            }
        }
    }
}
