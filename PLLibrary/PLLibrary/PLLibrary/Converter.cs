using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PLLibrary
{
    public static unsafe class Converter
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr Memcpy(void* dest, void* src, int count);
        public unsafe static byte[] FromInt(int number)
        {
            byte[] res = new byte[sizeof(int)];
            fixed (byte* pDest = res)
            {
                Memcpy(pDest, (void*)&number, sizeof(int));
            }
            return res;
        }
        public unsafe static int ToInt(byte[] array)
        {
            int res = 0;
            fixed (byte* pSrc = array)
            {
                Memcpy(&res, pSrc, sizeof(int));
            }
            return res;
        }

        public static int ToInt(this string text, int defaultValue = -99999)
        {
            if (int.TryParse(text, out int result)) return result;
            return defaultValue;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
