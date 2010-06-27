using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using SerialChannel.Channel.Factory;
using System.IO;
using System.Threading;

namespace SerialChannel.Channel.Request
{
    class SerialRequestChannel : SerialChannelBase, IRequestChannel
    {
        readonly Uri via;
        readonly object writeLock;

        AutoResetEvent aev;

        public SerialRequestChannel(BufferManager bufferManager, MessageEncoderFactory encoderFactory, EndpointAddress address, 
           SerialRequestChannelFactory parent, Uri via)
            : base(bufferManager, encoderFactory, address, parent.PortNumber, parent, parent.MaxReceivedMessageSize)
        {
            this.via = via;
            this.writeLock = new object();
        }

        #region ICommunicationObject Members
        protected override void OnAbort()
        {
            Console.WriteLine("SerialRequestChannel:OnAbort");
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:OnBeginClose");
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:OnBeginOpen");
            throw new NotImplementedException();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            Port.Close();
            Console.WriteLine("SerialRequestChannel:OnClose");
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            Console.WriteLine("SerialRequestChannel:OnEndClose");
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            Console.WriteLine("SerialRequestChannel:OnEndOpen");
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            Port.Open();
            Console.WriteLine("SerialRequestChannel:OnOpen");
        }
        #endregion

        #region IRequestChannel Members

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:BeginRequest");
            return BeginRequest(message, timeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            Console.WriteLine("SerialRequestChannel:BeginRequest - Default Timeout");
            return BeginRequest(message, DefaultReceiveTimeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return (Message)result.AsyncState;
        }

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

                    Port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Port_DataReceived);
                    aev = new AutoResetEvent(false);
                    if (aev.WaitOne(timeout))
                        return ReadMessage();
                    else
                        throw ConvertException(new TimeoutException());
                }
                catch (IOException exception)
                {
                    throw ConvertException(exception);
                }
            }

        }

        void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            aev.Set();
        }

        public Message Request(Message message)
        {
            return Request(message, DefaultReceiveTimeout);
        }

        public Uri Via
        {
            get { return this.via; }
        }

        #endregion
    }
}
