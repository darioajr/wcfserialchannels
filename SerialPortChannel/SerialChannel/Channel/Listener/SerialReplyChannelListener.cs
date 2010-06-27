using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using SerialChannel.Binding.Transport;
using SerialChannel.Channel.Reply;

namespace SerialChannel.Channel.Listener
{
    /// <summary>
    /// Reply Channel Listener
    /// </summary>
    class SerialReplyChannelListener : ChannelListenerBase, IChannelListener<IReplyChannel>
    {
        readonly BufferManager bufferManager;
        readonly MessageEncoderFactory encoderFactory;
        public readonly long MaxReceivedMessageSize;
        readonly string scheme;
        readonly Uri uri;

        int count = 0;
        AsyncResult result;

        public readonly string PortNumber;

        public SerialReplyChannelListener(SerialTransportBindingElement transportElement, BindingContext context)
            : base(context.Binding)
        {
            MessageEncodingBindingElement messageEncodingElement = context.Binding.Elements.Remove<MessageEncodingBindingElement>();
            this.bufferManager = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, int.MaxValue);
            this.encoderFactory = messageEncodingElement.CreateMessageEncoderFactory();
            MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
            this.scheme = transportElement.Scheme;
            this.uri = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            this.PortNumber = transportElement.ListenerPort;
        }


        #region IChannelListener<IReplyChannel> Members

        public IReplyChannel AcceptChannel(TimeSpan timeout)
        {
            EndpointAddress address = new EndpointAddress(Uri);
            SerialReplyChannel channel =
            new SerialReplyChannel(this.bufferManager, this.encoderFactory, address, PortNumber, this);
            return channel;
        }

        public IReplyChannel AcceptChannel()
        {
            //TODO: Change to defatul time
            return AcceptChannel(TimeSpan.Zero);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            count++;
            if (count == 1)
            {
                result = new AsyncResult(callback, state);

                ThreadPool.QueueUserWorkItem(
                    delegate(object asyncResult)
                    {
                        try
                        {
                            result.Channel = AcceptChannel(timeout);
                        }
                        finally
                        {
                            result.OnCompleted();
                        }
                    }
                    , result);

                return result;
            }
            else
            {
                Console.WriteLine("BeginAcceptChannel count = " + count);
                return result;
            }

        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IReplyChannel EndAcceptChannel(IAsyncResult result)
        {
            using (AsyncResult asyncResult =
            result as AsyncResult)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
                return (IReplyChannel)asyncResult.Channel;
            }

        }

        #endregion

        #region ListnerBase members
        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannelListener:OnBeginWaitForChannel");
            throw new NotImplementedException();
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannelListener:OnEndWaitForChannel");
            throw new NotImplementedException();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            Console.WriteLine("SerialReplyChannelListener:OnWaitForChannel");
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannelListener:OnBeginOpen");
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannelListener:OnEndOpen");
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            Console.WriteLine("SerialReplyChannelListener:OnOpen");
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialReplyChannelListener:OnBeginClose");
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            Console.WriteLine("SerialReplyChannelListener:OnEndClose");
        }

        protected override void OnClose(TimeSpan timeout)
        {
            Console.WriteLine("SerialReplyChannelListener:OnClose");
        }

        public override Uri Uri
        {
            get { return this.uri; }
        }

        protected override void OnAbort()
        {
            Console.WriteLine("SerialReplyChannelListener:OnAbort");
        }

        #endregion
    }

    /// <summary>
    /// The result of an asynchronous operation.
    /// </summary>
    public class AsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback m_AsyncCallback;
        private object m_State;
        private ManualResetEvent m_ManualResetEvent;
        private IReplyChannel channel;

        public IReplyChannel Channel
        {
            get { return channel; }
            set { channel = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncResult"/> class.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        public AsyncResult(
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
