using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using SerialChannel.Channel.Listener;

namespace SerialChannel.Channel.Reply
{
    /// <summary>
    /// Reply channel. Includes RequestContext as sub class
    /// </summary>
    partial class SerialReplyChannel : SerialChannelBase , IReplyChannel
    {
        readonly EndpointAddress localAddress;
        readonly object readLock;

        AutoResetEvent aev;

        /// <summary>
        /// Constructor called by listener
        /// </summary>
        /// <param name="bufferManager">Buffer created by Listener</param>
        /// <param name="encoderFactory">Encoder factory as received by encoding element</param>
        /// <param name="address">Remote address</param>
        /// <param name="portNumber">Server port</param>
        /// <param name="parent">Reference to listner</param>
        public SerialReplyChannel(BufferManager bufferManager, MessageEncoderFactory encoderFactory, EndpointAddress address, string portNumber, 
           SerialReplyChannelListener parent)
            : base(bufferManager, encoderFactory, address, parent.PortNumber, parent, parent.MaxReceivedMessageSize)
        {
            this.localAddress = address;
            this.readLock = new object();
        }

        #region ICommunicationObject Members

        #region Open Methods
        protected override void OnOpen(TimeSpan timeout)
        {
            Console.WriteLine("SerialReplyChannel:OnOpen");
            Port.Open();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannel:OnBeginOpen");
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannel:OnEndOpen");
        }
        #endregion

        #region Close Methods
        protected override void OnClose(TimeSpan timeout)
        {
            Console.WriteLine("SerialReplyChannel:OnClose");
            if (Port.IsOpen)
                Port.Close();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannel:OnBeginClose");
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannel:OnEndClose");
        }
        #endregion

        #region Abort Methods
        protected override void OnAbort()
        {
            Console.WriteLine("SerialReplyChannel:OnAbort");
        }
        #endregion

        #endregion

        #region IReplyChannel Members

        #region TryReceiveRequest Methods

        /// <summary>
        /// Waits for request and then receives request.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="context"></param>
        /// <returns>Return true if received else false.</returns>
        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            context = null;
            bool complete = WaitForRequest(timeout);
            if (!complete)
            {
                return false;
            }
            context = ReceiveRequest(DefaultReceiveTimeout);
            return true;

        }

        /// <summary>
        /// Waits for request and then receives request.
        /// </summary>
        /// <param name="timeout">Timeout for Receive</param>
        /// <param name="callback">Asynchronouse call</param>
        /// <param name="state">state</param>
        /// <returns>Returns token</returns>
        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("BeginTryReceiveRequest");

            ChannelAsyncResult result = new ChannelAsyncResult(callback, state);

            ThreadPool.QueueUserWorkItem(
                delegate(object asyncResult)
                {
                    try
                    {
                        RequestContext context;
                        if (TryReceiveRequest(TimeSpan.FromMinutes(3), out context))
                        {
                            result.Context = context;
                        }
                        else
                        {
                            result.Context = null;
                            Console.WriteLine("Receive failed");
                        }
                    }
                    finally
                    {
                        result.OnCompleted();
                    }

                }
                , result);

            return result;

        }

        /// <summary>
        /// End try receive request
        /// </summary>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns>Returns true of request received else false</returns>
        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            Console.WriteLine("EndTryReceiveRequest");

            using (ChannelAsyncResult asyncResult = result as ChannelAsyncResult)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
                context = asyncResult.Context;
            }

            return true;

        }


        #endregion

        #region WaitForRequest Methods
        /// <summary>
        /// Wait for request
        /// </summary>
        /// <param name="timeout">Timout for waiting thread</param>
        /// <param name="callback">Asynchronous call back</param>
        /// <param name="state">State</param>
        /// <returns>Returns token</returns>
        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannel:BeginWaitForRequest");
            throw new NotImplementedException();
        }

        /// <summary>
        /// End waiting request
        /// </summary>
        /// <param name="result">Token</param>
        /// <returns>Retuns true of a new request is received else false.</returns>
        public bool EndWaitForRequest(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannel:EndWaitForRequest");
            //TODO:
            return true;
        }

        /// <summary>
        /// Wait for request.
        /// </summary>
        /// <param name="timeout">Request timeout</param>
        /// <returns>Return true if message is received. Else false. May throw Communication execeoption in case of error.</returns>
        public bool WaitForRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            try
            {
                Port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Port_DataReceived);
                aev = new AutoResetEvent(false);
                return aev.WaitOne(timeout);
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        /// <summary>
        /// Signal wait for request.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            aev.Set();
        }
        #endregion

        #region Receive Request
        /// <summary>
        /// Begin Request
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannel:BeginReceiveRequest");
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannel:BeginReceiveRequest");
            //TODO: Not implemented
            return BeginReceiveRequest(DefaultReceiveTimeout, callback, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannel:EndReceiveRequest");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive Request
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();
            lock (readLock)
            {
                Message message = ReadMessage();
                return new SerialRequestContext(message, this);
            }
        }

        /// <summary>
        /// Receive Request with default timeout
        /// </summary>
        /// <returns></returns>
        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(DefaultReceiveTimeout);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        #endregion
    }



    /// <summary>
    /// The result of an asynchronous operation.
    /// </summary>
    class ChannelAsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback m_AsyncCallback;
        private object m_State;
        private ManualResetEvent m_ManualResetEvent;
        private RequestContext context;

        public RequestContext Context
        {
            get { return context; }
            set { context = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public ChannelAsyncResult(
            AsyncCallback callback,
            object state)
        {
            this.m_AsyncCallback = callback;
            this.m_State = state;
            this.m_ManualResetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Completes this instance.
        /// </summary>
        public virtual void OnCompleted()
        {
            m_ManualResetEvent.Set();
            if (m_AsyncCallback != null)
            {
                m_AsyncCallback(this);
            }
        }

        #region IAsyncResult Members
        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <value></value>
        /// <returns>A user-defined object that qualifies or contains information about an asynchronous operation.</returns>
        public object AsyncState
        {
            get
            {
                return m_State;
            }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.</returns>
        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get
            {
                return m_ManualResetEvent;
            }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <value></value>
        /// <returns>true if the asynchronous operation completed synchronously; otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        /// <value></value>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get
            {
                return m_ManualResetEvent.WaitOne(0, false);
            }
        }
        #endregion IAsyncResult Members

        #region IDisposable Members
        private bool m_IsDisposed = false;
        private event System.EventHandler m_Disposed;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        [System.ComponentModel.Browsable(false)]
        public bool IsDisposed
        {
            get
            {
                return m_IsDisposed;
            }
        }

        /// <summary>
        /// Occurs when this instance is disposed.
        /// </summary>
        public event System.EventHandler Disposed
        {
            add
            {
                m_Disposed += value;
            }
            remove
            {
                m_Disposed -= value;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                this.Dispose(true);
                System.GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    //m_ManualResentEvent.Close();
                    //m_ManualResentEvent = null;
                    m_State = null;
                    m_AsyncCallback = null;

                    System.EventHandler handler = m_Disposed;
                    if (handler != null)
                    {
                        handler(this, System.EventArgs.Empty);
                        handler = null;
                    }
                }
            }
            finally
            {
                m_IsDisposed = true;
            }
        }

        /// <summary>
        ///    <para>
        ///        Checks if the instance has been disposed of, and if it has, throws an <see cref="ObjectDisposedException"/>; otherwise, does nothing.
        ///    </para>
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">
        ///    The instance has been disposed of.
        ///    </exception>
        ///    <remarks>
        ///    <para>
        ///        Derived classes should call this method at the start of all methods and properties that should not be accessed after a call to <see cref="Dispose()"/>.
        ///    </para>
        /// </remarks>
        protected void CheckDisposed()
        {
            if (m_IsDisposed)
            {
                string typeName = GetType().FullName;

                throw new System.ObjectDisposedException(
                    typeName,
                    String.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "Cannot access a disposed {0}.",
                        typeName));
            }
        }
        #endregion IDisposable Members
    }    
}
