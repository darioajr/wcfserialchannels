using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO.Ports;
namespace SerialChannel.Channel
{
    abstract class SerialChannelBase : ChannelBase
    {
        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;

        readonly EndpointAddress address;
        readonly BufferManager bufferManager;
        readonly MessageEncoder encoder;
        readonly long maxReceivedMessageSize;
        readonly string portNumber;

        SerialPort serialPort;

        public SerialChannelBase(BufferManager bufferManager, 
            MessageEncoderFactory encoderFactory, 
            EndpointAddress address,
            string portNumber,
            ChannelManagerBase parent,
            long maxReceivedMessageSize)
            : base(parent)
        {
            this.address = address;
            this.bufferManager = bufferManager;
            this.encoder = encoderFactory.CreateSessionEncoder();
            this.maxReceivedMessageSize = maxReceivedMessageSize;

            this.portNumber = portNumber;

            serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            serialPort.PortName = this.portNumber;
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            serialPort.ReadTimeout = 500;
            serialPort.WriteTimeout = 500;
        }

        protected static Exception ConvertException(Exception exception)
        {
            Type exceptionType = exception.GetType();
            if (exceptionType == typeof(System.IO.DirectoryNotFoundException) ||
                exceptionType == typeof(System.IO.FileNotFoundException) ||
                exceptionType == typeof(System.IO.PathTooLongException))
            {
                return new EndpointNotFoundException(exception.Message, exception);
            }
            return new CommunicationException(exception.Message, exception);
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.address; }
        }

        public SerialPort Port
        {
            get { return serialPort; }
        }

        protected Message ReadMessage()
        {
            byte[] data;
            int bytesRead = 0;
            try
            {
                Stream stream = serialPort.BaseStream;
                data = this.bufferManager.TakeBuffer((int)maxReceivedMessageSize);

                bytesRead = stream.Read(data, bytesRead, (int)maxReceivedMessageSize);
                if (bytesRead == 0)
                {
                    throw new CommunicationException(String.Format("Unexpected end of message after {0} of {1} bytes.", bytesRead, maxReceivedMessageSize));
                }
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
            ArraySegment<byte> buffer = new ArraySegment<byte>(data, 0, (int)bytesRead);
            return this.encoder.ReadMessage(buffer, this.bufferManager);
        }

        //protected void WriteMessage(string path, Message message)
        protected void WriteMessage(Message message)
        {
            ArraySegment<byte> buffer;
            using (message)
            {
                this.address.ApplyTo(message);
                buffer = this.encoder.WriteMessage(message, MaxBufferSize, this.bufferManager);
            }
            try
            {
                Stream stream = serialPort.BaseStream;
                stream.Write(buffer.Array, buffer.Offset, buffer.Count);
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }
    }
}
