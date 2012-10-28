using System;
using System.IO;
using System.IO.Ports;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace SerialChannel.Binding.Transport
{
    /// <summary>
    /// Channel fore request messages
    /// </summary>
    /// <remarks>Used by client side WCF runtime.</remarks>
    class SerialRequestChannel : SerialChannelBase, IRequestChannel
    {
        readonly Uri via;
        readonly object writeLock;

        AutoResetEvent aev;

        /// <summary>
        /// Constructor called by factory.
        /// </summary>
        /// <param name="bufferManager">
        /// Buffer manager used to send receive messages</param>
        /// <param name="encoderFactory">
        /// Encoder factory as received by parent</param>
        /// <param name="address">Remote address</param>
        /// <param name="parent">Reference to parent</param>
        /// <param name="via">via in case of routing</param>
        public SerialRequestChannel(BufferManager bufferManager, 
          MessageEncoderFactory encoderFactory, EndpointAddress address, 
           SerialRequestChannelFactory parent, Uri via)
            : base(bufferManager, encoderFactory, address, 
          parent.PortNumber, parent, parent.MaxReceivedMessageSize)
        {
            this.via = via;
            this.writeLock = new object();
        }

        #region ICommunicationObject Members
        
        #region Open Methods
        protected override void OnOpen(TimeSpan timeout)
        {
            Console.WriteLine("SerialRequestChannel:OnOpen");
            Port.Open();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, 
          AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:OnBeginOpen");
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Console.WriteLine("SerialRequestChannel:OnEndOpen");
        }
        #endregion

        #region Close Methods
        protected override void OnClose(TimeSpan timeout)
        {
            Console.WriteLine("SerialRequestChannel:OnClose");
            Port.Close();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, 
          AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:OnBeginClose");
            throw new NotImplementedException();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            Console.WriteLine("SerialRequestChannel:OnEndClose");
        }
        #endregion

        #region Abort
        protected override void OnAbort()
        {
            Console.WriteLine("SerialRequestChannel:OnAbort");
        }
        #endregion

        #endregion

        #region IRequestChannel Members

        /// <summary>
        /// Asynchronous begin operation
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="timeout">Timeout. 
        /// If not specified defaults to DefaultReceiveTimeout of channel base 
        /// </param>
        /// <param name="callback">
        /// Callback when request is made in asynchronous mode.</param>
        /// <param name="state">Object that contains state</param>
        /// <returns>Return async token.</returns>
        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, 
          AsyncCallback callback, object state)
        {
          throw new NotImplementedException("Async not implemented");
        }

        /// <summary>
        /// Asynchronous begin operation with default timeout of channel base
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <param name="callback">
        /// Callback when request is made in asynchronous mode.</param>
        /// <param name="state">Object that contains state</param>
        /// <returns>Return async token.</returns>
        public IAsyncResult BeginRequest(Message message, 
          AsyncCallback callback, object state)
        {
          throw new NotImplementedException("Async not implemented");
        }

        /// <summary>
        /// Returns Message when asynchronous operation is finished.
        /// </summary>
        /// <param name="result">Token recieved during BeginRequest Call</param>
        /// <returns>Returns message.</returns>
        public Message EndRequest(IAsyncResult result)
        {
          throw new NotImplementedException("Async not implemented");
        }

        /// <summary>
        /// Calls base class to perform write and read operations.
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <param name="timeout">Timout to write and read</param>
        /// <returns>Returns reply if received else throws exception</returns>
        public Message Request(Message message, TimeSpan timeout)
        {
          ThrowIfDisposedOrNotOpen();
          lock (this.writeLock)
          {
            try
            {
              if (this.State == CommunicationState.Closed)
                this.Open();

              WriteMessage(message);

              Port.DataReceived +=
                new SerialDataReceivedEventHandler(Port_DataReceived);
              aev = new AutoResetEvent(false);
              if (aev.WaitOne(timeout))
              {
                return ReadMessage();
              }
              else
              {
                throw ConvertException(new TimeoutException());
              }
            }
            catch (IOException exception)
            {
              throw ConvertException(exception);
            }
          }

        }

        /// <summary>
        /// Signal waiting thread in Read to continue execution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Port_DataReceived(object sender, 
          System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            aev.Set();
        }

        /// <summary>
        /// Sunchronous read with default time out
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Message Request(Message message)
        {
            return Request(message, DefaultReceiveTimeout);
        }

        /// <summary>
        /// Interface property.
        /// </summary>
        public Uri Via
        {
            get { return this.via; }
        }

        #endregion
    }
}
