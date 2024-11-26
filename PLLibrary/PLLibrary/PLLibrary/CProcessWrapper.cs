using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PLLibrary.CWindow;

namespace PLLibrary
{
    public class CProcessWrapper: IComparable<CProcessWrapper>
    {
        public CProcessWrapper(Process p) 
        {
            Process = p;
        }
        public Process Process { get; }

        public int CompareTo(CProcessWrapper other)
        {
            if (other == null) return 1;
            return Process.ProcessName.CompareTo(other.Process.ProcessName);
        }

        public override string ToString()
        {
            return Process.ProcessName + ", [" + Process.MainWindowTitle + "], " + Process.MainWindowHandle;
        }
        //=======================================================
        public static implicit operator string (CProcessWrapper wrapper) => wrapper.Process.ProcessName;
        public static implicit operator IntPtr(CProcessWrapper wrapper) => wrapper.Process.MainWindowHandle;
    }

    public class CProcessList: List<CProcessWrapper>
    {
        public CProcessList(IEnumerable<Process> list) 
        {
            foreach (Process process in list)
            {
                Add(new CProcessWrapper( process));
            }
        }
        public CProcessList(List<CProcessWrapper> list)
        {
            AddRange(list);
        }
        public CProcessList RemoveZeroHandle() => new CProcessList(FindAll(item => item.Process.MainWindowHandle != IntPtr.Zero)); 
        public CProcessWrapper this[string processName] => Find(it => it.Process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
        //=======================================================
        public static CProcessList Processes => new CProcessList(Process.GetProcesses());

    }

    public class CWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner

            public static implicit operator Rectangle(RECT rc)
            {
                return new Rectangle(rc.Left, rc.Top, rc.Right - rc.Left + 1, rc.Bottom - rc.Top + 1);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, int p);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(IntPtr hwnd, string lpString);

        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        private const int WM_GETTEXT = 0xD;
        private const int WM_GETTEXTLENGTH = 0xE;

        public CWindow(IntPtr handle)
        {
            Handle = handle;
        }
        public IntPtr Handle { get; }
        public IntPtr Parent
        {
            get
            {
                return GetParent(Handle);
            }
            set
            {
                SetParent(Handle, value);
            }
        }
        public override string ToString()
        {
            return Handle + ", " + Bounds + "[" +Text + "]";
        }
        public Rectangle Bounds
        {
            get
            {
                GetWindowRect(Handle, out RECT rc);
                return (Rectangle)rc;
                //int[] arr = { 0, 0, 0, 0 };
                //GetWindowRect(Handle, (int)arr.GetPointer());
                //return new Rectangle(arr[0], arr[1], arr[2] - arr[0]+1, arr[3] - arr[1]+1);
            }
            set
            {
                MoveWindow(Handle, value.X, value.Y, value.Width, value.Height, true);
            }
        }
        public Image Image
        {
            get
            {
                Rectangle rc = Bounds;
                Bitmap bm = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //Graphics graphics = Graphics.FromImage(bm);
                //graphics.Dispose();
                using(Graphics g = Graphics.FromImage(bm))
                {
                    g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size);
                }
                return bm;
            }
        }
        public string ClassName
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(255);
                GetClassName(Handle, stringBuilder, 255);
                return stringBuilder.ToString();
            }
        }
        public string Text
        {
            get
            {
                // StringBuilder stringBuilder = new StringBuilder(32000);
                // GetWindowText(Handle, stringBuilder, 32000);
                // return stringBuilder.ToString();
                return WindowText(Handle);
            }
            set
            {
                SetWindowText(Handle, value);
            }
        }

        unsafe string WindowText(IntPtr hWnd)
        {
            int len = (int)SendMessage(hWnd, WM_GETTEXTLENGTH, 0, IntPtr.Zero);
            string text = new string((char)0, len + 1);
            fixed (char* textPtr = text) 
            {
                SendMessage(hWnd, WM_GETTEXT, len+1, (IntPtr)textPtr);
            }
            return text.Substring(0, len);
        }
           
      

        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr parameter);
        public List<CWindow> GetChildWindows()
        {
            //List<CWindow> list = new List<CWindow>();
            //GCHandle listHandle = GCHandle.Alloc(list);
            //IntPtr ptrList = GCHandle.ToIntPtr(listHandle);
            //try
            //{
            //    EnumChildWindows(Handle, EnumWindow, ptrList);
            //}
            //finally
            //{
            //    listHandle.Free();
            //}

            //List<CWindow> list = new List<CWindow>();
            //using (CObjectPtr<List<CWindow>> obj = new CObjectPtr<List<CWindow>>(list))
            //{
            //    EnumChildWindows(Handle, EnumWindow, obj.Handle);
            //    return obj.Value;
            //}

            //CWindowList list = new CWindowList();
            //using (CObjectPtr<CWindowList> obj = new CObjectPtr<CWindowList>(list))
            //{
            //    EnumChildWindows(Handle, EnumWindow, obj.Handle);
            //    return obj.Value;
            //}

            using (CObjectPtrWindowList obj = new CObjectPtrWindowList())
            {
                EnumWin(Handle, obj.Handle);
                return obj.Value;
            }
        }
        private void EnumWin(IntPtr hWnd, IntPtr customArgs)
        {
            EnumChildWindows(hWnd, EnumWindow, customArgs);
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr pointer)
        {
            //GCHandle gch = GCHandle.FromIntPtr(pointer);
            //List<CWindow> list = gch.Target as List<CWindow>;

            //if (list != null)
            //{
            //    list.Add(new CWindow(hWnd));
            //}

            //List<CWindow>  list = CObjectPtr<List<CWindow>>.From(pointer);
            //if (list != null)
            //{
            //    list.Add(new CWindow(hWnd));
            //}

            CWindowList list = CObjectPtrWindowList.From(pointer);
            if (list != null)
            {
                list.Add(new CWindow(hWnd));

                //To Get all children of this hWnd, call the function below
                //EnumWin(hWnd, pointer);
            }
            return true;
        }


        //C1
        //  C1_1
        //      C1_1_1
        //  C1_2
        //....
        //  C1_24
        private class CWindowList : List<CWindow> { }
        //private class CObjectPtrWindowList : CObjectPtr<CWindowList>
        //{
        //    public CObjectPtrWindowList(CWindowList list) : base(list)
        //    {

        //    }
        //}
        private class CObjectPtrWindowList : CObjectPtr<CWindowList> { }
    }
    
}
