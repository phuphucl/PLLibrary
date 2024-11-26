using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;

using static PLLibrary.CGeneric;

namespace PLLibrary
{
    public class PLThread : IDisposable
    {
        private Thread _rThread;
        private bool _bRunning = false;
        private bool _bDispose = false;
        private int _interval = 0;

        public event EventHandler ThreadRun;
        private event EventHandler ThreadStart;
        public event EventHandler ThreadStop;
        private int _iThreadID = 0;

        public PLThread(int interval)
        {
            _interval = interval;

        }

        public void Start()
        {
            Debug.Print("Thread Id = " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            _rThread = new Thread(ThreadFunc);
            _rThread.Start();
            WaitUntilTrue(ref _bRunning);
        }

        public int Interval
        {
            get => _interval;
            set
            {
                if (value > 0)
                {
                    _interval = value;
                }
            }
        }

        private void ThreadFunc()
        {
            _bRunning = true;
            ThreadStart?.Invoke(this, EventArgs.Empty);
            _iThreadID = Thread.CurrentThread.ManagedThreadId;
            while (_bRunning)
            {
                if (_interval > 0) System.Threading.Thread.Sleep(_interval);
                if (ThreadRun != null) ThreadRun(this, EventArgs.Empty);
            }
            ThreadStop?.Invoke(this, EventArgs.Empty);
            _bDispose = true;
        }

        public void Dispose()
        {
            _bDispose = false;
            _bRunning = false;
            if (_iThreadID != Thread.CurrentThread.ManagedThreadId)
            {
                WaitUntilTrue(ref _bDispose);
            }
        }
    }
    //public class PLThread : IDisposable
    //{
    //    private Thread _rThread;
    //    private bool _bRunning = false;
    //    private bool _bDispose = false;
    //    private int _interval = 0;

    //    public class ThreadStartEventArgs
    //    {
    //        public int ThreadId { get; }
    //        public bool Cancel { get; set; }
    //        internal ThreadStartEventArgs(int threadId)
    //        {
    //            ThreadId = threadId;
    //        }
    //    }

    //    //public delegate void MyEvent(string abc, DateTime tm);

    //    public event EventHandler ThreadRun;
    //    private event EventHandler<ThreadStartEventArgs> _threadStart;
    //    //public event MyEvent ThreadStop;
    //    public event EventHandler ThreadStop;

    //    public PLThread(int interval)
    //    {
    //        _interval = interval;

    //    }

    //    public void Start()
    //    {
    //        Debug.Print("Thread Id = " + System.Threading.Thread.CurrentThread.ManagedThreadId);
    //        _rThread = new Thread(ThreadFunc);
    //        _rThread.Start();
    //        WaitUntilTrue(ref _bRunning);
    //    }

    //    public int Interval
    //    {
    //        get => _interval;
    //        set
    //        {
    //            if (value > 0)
    //            {
    //                _interval = value;
    //            }
    //        }
    //    }

    //    public event EventHandler<ThreadStartEventArgs> ThreadStart
    //    {
    //        add
    //        {
    //            _threadStart += value;
    //        }
    //        remove
    //        {
    //            _threadStart -= value;
    //        }
    //    }
    //    private void ThreadFunc()
    //    {
    //        Debug.Print("ThreadFunc: Thread Id = " + System.Threading.Thread.CurrentThread.ManagedThreadId);
    //        _bRunning = true;
    //        ThreadStartEventArgs evt = new ThreadStartEventArgs(Thread.CurrentThread.ManagedThreadId);
    //        _threadStart?.Invoke(this, evt);
    //        if (evt.Cancel) return;
    //        while (_bRunning)
    //        {
    //            if (_interval > 0) System.Threading.Thread.Sleep(_interval);
    //            if (ThreadRun != null) ThreadRun(this, EventArgs.Empty);
    //        }
    //        ThreadStop?.Invoke(this, EventArgs.Empty);
    //        _bDispose = true;
    //    }

    //    public void Dispose()
    //    {
    //        _bDispose = false;
    //        _bRunning = false;
    //        CGeneric.WaitUntilTrue(ref _bDispose);
    //    }
    //}
}
