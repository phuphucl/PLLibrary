using PLLibrary;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PLLibrary
{
    public class CObjectPtr<T>: IDisposable
    {
        GCHandle _gcHandle;
        T _value;
        public CObjectPtr()
        {
            Type type = typeof(T);
            object obj = Activator.CreateInstance(type);
            if (obj != null && obj is T value) Init(value);
        }
        public CObjectPtr(T value) => Init(value);
        private void Init(T value)
        {
            _value = value;
            _gcHandle = GCHandle.Alloc(value);
        }
        public unsafe IntPtr Pointer
        {
            get
            {
                IntPtr* arr = (IntPtr*)Handle;
                return arr[0] + sizeof(IntPtr);
            }
        }
        public IntPtr Handle => GCHandle.ToIntPtr(_gcHandle);
        public T Value => _value;

        public void Dispose()
        {
            if (_gcHandle.IsAllocated) _gcHandle.Free();
        }

        public static T From(IntPtr ptr)
        {
            GCHandle gch = GCHandle.FromIntPtr(ptr);
            if (gch.IsAllocated && gch.Target is T value )
            {
                return value;
            }
            return default;
        }
    }

    public class PLHandle
    {
        private static int index = 0;
        private static Dictionary<IntPtr, PLHandle> MyHandler = new Dictionary<IntPtr, PLHandle>();
        public static PLHandle Alloc(object obj)
        {
            index++;
            IntPtr ptr = (IntPtr)index;
            PLHandle handle = new PLHandle(ptr, obj);
            MyHandler[ptr] = handle;
            return handle;
        }
        public static PLHandle FromIntPtr(IntPtr intPtr)
        {
            if (MyHandler.ContainsKey(intPtr))
                return MyHandler[intPtr];
            return null;
        }
        public static IntPtr ToIntPtr(PLHandle handle) => handle._intPtr;
        
        private static void Remove(PLHandle handle)
        {
            if (MyHandler.ContainsKey(handle._intPtr))
            {
                MyHandler.Remove(handle._intPtr);
            }
        }
        //=======================================
        private IntPtr _intPtr;
        private object _value;
        private PLHandle(IntPtr intPtr, object value)
        {
            _intPtr = intPtr;
            _value = value;
        }
        public object Target => _value;
        public bool IsAllocated => MyHandler.ContainsKey(_intPtr);
        public void Free()
        {
            Remove(this);
        }
    }
}
