using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PLLibrary
{
    public enum enPLTcpClientType
    {
        Caller, Callee
    }
    public class PLTcpClientArgs
    {
        public CBuffer Buffer { get; }
        public int Size { get; }
        internal PLTcpClientArgs(CBuffer buffer, int size)
        {
            Buffer = buffer;
            Size = size;
        }
    }

    public class PLTcpClient
    {
        public event EventHandler<PLTcpClientArgs> DataArival;
        public event EventHandler Disconnected;
        private PLThread _thread;
        private TcpClient _client;
        private CBuffer _bfRead;
        public PLTcpClient(TcpClient client)
        {
            _client = client;
            PLTcpClientType = enPLTcpClientType.Callee;
            Init();
        }
        public PLTcpClient()
        {

        }
        public PLTcpClient(string ip, int port)
        {
            Connect(ip, port);            
        }
        
        public Exception Connect(string ip, int port)
        {
            Exception result = null;
            if (_client == null) _client = new TcpClient();
            PLTcpClientType = enPLTcpClientType.Caller;
            try
            {
                if (ip.Ping())
                {
                   _client.Connect(ip, port);
                }
                else
                {
                    result = new Exception("Cannot find " + ip);
                }
            }
            catch (Exception ex)
            {
                result = ex;
            }
            Init();
            if (result == null && !Connected )
            {
                result = new Exception("Unkown error");
            }
            return result;
        }

        private void Init()
        {
            if (_client != null && Connected)
            {
                _bfRead = new CBuffer();
                _thread = new PLThread(5);
                _thread.ThreadRun += Thread_ThreadRun;
                _thread.Start();
            }
        }

        private void Thread_ThreadRun(object sender, EventArgs e)
        {
            if (Connected)
            {
                int size = _client.Available;
                int offset = 0;                         //0
                int sizeAvail = _bfRead.Bytes.Length;   //xxxxxxxxxxxxxxxxxxxXxxxxxxxxxxxxxxxxxxXxxxxxxxxxxxxxxxx
                while (size > 0)
                {
                    if (size > sizeAvail)
                    {
                        _bfRead.ResizeBuffer(size); // !!
                    }
                    int sizeRead = _client.GetStream().Read(_bfRead.Bytes, offset, sizeAvail);
                    if (sizeRead > 0)
                    {
                        offset += sizeRead;
                    }
                    System.Threading.Thread.Sleep(10);
                    size = _client.Available;
                }
                if (offset > 0)
                {
                    PLTcpClientArgs args = new PLTcpClientArgs(_bfRead, offset);
                    DataArival?.Invoke(this, args);
                }
            }
            else if (_client != null)
            {
                Close();
            }
        }

        public void Bind(TcpClient client)
        {
            _client.Client.Bind(client.Client.RemoteEndPoint);
            client.Client.Bind(_client.Client.RemoteEndPoint);
        }

        public void Send(string text)
        {
            byte[] array = Encoding.Unicode.GetBytes(text);
            Send(array, 0, array.Length);
        }
        public void Send(byte[] array)
        {
            Send(array, 0, array.Length);
        }
        public void Send(byte[] array, int offset, int size)
        {
            if (Connected)
            {
                _client.GetStream().Write(array, offset, size);
            }
        }

        public enPLTcpClientType PLTcpClientType { get; private set; }
        

        public bool Connected
        {
            get
            {
                bool connected = false;
                try
                {
                    if (_client == null) return false;
                    Socket socket = _client.Client;
                    connected = _client.Connected && !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
                }
                catch { } 
                return connected;
            }
        }

        public void Close()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
            if (Connected)
            {
                _client.Close();
            }
            _client = null;
            if (_thread != null)
            {
                _thread.Dispose();
                _thread = null;
            }
        }

        public override string ToString()
        {
            if (_client == null)
            {
                return "";
            }
            //return $"TCP Client [RemoteEndPoint: {_client.Client?.RemoteEndPoint}, LocalEndPoint: {_client.Client?.LocalEndPoint}, Connected: {_client.Connected}, Available Data: {_client.Available} bytes]";
            return _client.Client?.RemoteEndPoint.ToString(); 
        }
    }
}
