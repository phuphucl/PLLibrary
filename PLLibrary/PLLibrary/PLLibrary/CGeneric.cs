using System.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;


#pragma warning disable

namespace PLLibrary
{
    public static unsafe class CGeneric
    {
        //public static string Hello()
        //{
        //    int value = 10;
        //    if (Debugger.IsAttached)
        //    {
        //        MessageBox.Show("Debugger version");
        //    }
        //    return "Hello, my name is Phuc Le";
        //}
        
        private static BindingFlags AllBinding = (BindingFlags)(-1);
        public static object GetFieldValue(this object instance, string fieldName)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                FieldInfo info = type.GetField(fieldName, AllBinding);
                if (info != null)
                {
                    return info.GetValue(instance);
                }
            }
            return null;
        }
        public static void SetFieldValue(this object instance, string fieldName, object value)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                FieldInfo info = type.GetField(fieldName, AllBinding);
                if (info != null)
                {
                    info.SetValue(instance, value);
                }
            }
        }
        public static object GetPropertyValue(this object instance, string propertyName)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                PropertyInfo info = type.GetProperty(propertyName, AllBinding);
                if (info != null && info.GetMethod != null)
                {
                    return info.GetValue(instance);
                }
            }
            return null;
        }
        public static void SetPropertyValue(this object instance, string propertyName, object value)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                PropertyInfo info = type.GetProperty(propertyName, AllBinding);
                if (info != null && info.SetMethod != null)
                {
                    info.SetValue(instance, value);
                }
            }
        }

        public static object CallMethod(this object instance, string fieldName, params object[] args)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                MethodInfo info = type.GetMethod(fieldName, AllBinding);
                if (info != null)
                {
                    return info.Invoke(instance, args);
                }
            }
            return null;
        }

        public static object CallMethod<T>(this object instance, string fieldName, params object[] args)
        {
            if (instance != null)
            {
                Type type = instance.GetType();
                MethodInfo info = type.GetMethod(fieldName, AllBinding);
                if (info != null)
                {
                    MethodInfo templateInfo = info.MakeGenericMethod(typeof(T));
                    if (templateInfo != null)
                    {
                        return templateInfo.Invoke(instance, args);
                    }
                }
            }
            return null;
        }

        public static IntPtr GetPointer(this object obj)
        {
            if (obj == null) return IntPtr.Zero;

            TypedReference reference = __makeref(obj);
            return *(IntPtr*)&reference;
        }
        public static IntPtr GetPointer<T>(ref T obj) where T : struct
        {
            TypedReference reference = __makeref(obj);
            return *(IntPtr*)&reference;
        }

        public static IntPtr GetPointer<T>(this T[] obj) where T : struct
        {
            if (obj == null || obj.Length == 0) return IntPtr.Zero;

            TypedReference reference = __makeref(obj[0]);
            return *(IntPtr*)&reference;
        }

        public static IntPtr GetPointer<T>(this T[,] obj)
        {
            if (obj == null || obj.Length == 0) return IntPtr.Zero;

            TypedReference reference = __makeref(obj[0,0]);
            return *(IntPtr*)&reference;
        }

        public static void WaitUntilTrue(ref bool condition)
        {
            while (!condition)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(1);
            }
        }

        //public static int GetArrsIdx<T>(this T[,] obj, int row, int col)
        //{
        //    if (obj == null || obj.Length == 0) return 0;
        //    TypedReference reference = __makeref(obj[0, 0]);
        //    int colNum = *((int*)(*(IntPtr*)&reference) - 3);
        //    int rowNum = *((int*)(*(IntPtr*)&reference) - 4);
        //    if (rowNum == 0 || colNum == 0 || row > rowNum - 1 || row < 0 || col > colNum - 1 || col < 0) { return 0; }
        //    int index = colNum * row + col;
        //    return index;
        //}

        public static int GetArrsIdx(int rowNum, int colNum, int row, int col)
        {
            if (rowNum == 0 || colNum == 0 || row > rowNum-1 || row < 0 || col > colNum-1 || col < 0) { return 0; }
            int index = colNum * row + col;
            return index;
        }

        public static void GetArrsInfo<T>(this T[,] obj, int index)
        {
            if (obj == null || obj.Length == 0 || index > obj.Length-1) return;
            TypedReference reference = __makeref(obj[0, 0]);
            int colNum = *((int*)(*(IntPtr*)&reference) - 3);
            int rowNum = *((int*)(*(IntPtr*)&reference) - 4);
            int column = index / colNum;
            int row = (index - column) / colNum;
          
            
        }

        public static bool Ping(this string nameOrAddress)
        {
            try
            {
                using (Ping pinger = new Ping())
                {
                    PingReply reply = pinger.Send(nameOrAddress);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (PingException)
            {
                return false;
            }
        }

        public static List<IPAddressWrapper> GetAvailableIPs()
        {
            List<IPAddressWrapper> LstIP = new List<IPAddressWrapper>();

            LstIP.Add(new IPAddressWrapper(IPAddress.Any, "All"));
            IPAddress[] ips = Dns.GetHostAddresses("localhost");
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    LstIP.Add(new IPAddressWrapper(ip, "localhost"));
                }
            }
            ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    LstIP.Add(new IPAddressWrapper(ip));
                }
            }
            return LstIP;
        }
    }
    public class IPAddressWrapper
    {
        public IPAddressWrapper(IPAddress ip, string toString)
        {
            IPAddress = ip;
            Text = toString;
        }
        public IPAddressWrapper(IPAddress ip)
        {
            IPAddress = ip;
            Text = ip.ToString();
        }
        public IPAddress IPAddress { get; }
        public string Text { get; }
        public override string ToString() => Text;
    }

    
}
