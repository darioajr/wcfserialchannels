using System;
using System.IO;
using System.IO.Ports;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;

namespace SerialChannel.Channel
{
    /// <summary>
    /// Abstract Serial Channel Base class. <br />
    /// Creates serial port and performs read and write operations.
    /// </summary>
    abstract class SerialChannelBase : ChannelBase
    {
        const int MaxBufferSize = 64 * 1024;
        const int MaxSizeOfHeaders = 4 * 1024;

        readonly EndpointAddress address;
        readonly BufferManager bufferManager;
        readonly MessageEncoder encoder;
        readonly long maxReceivedMessageSize;
        readonly string portNumber;

        static string lastMessage = null;

        SerialPort serialPort;

        /// <summary>
        /// SerialChannel Base
        /// </summary>
        /// <param name="bufferManager">Buffer manager created by factory and listener</param>
        /// <param name="encoderFactory">Referece to encoder factory as returned by encoder element</param>
        /// <param name="address">Remote address</param>
        /// <param name="portNumber">COM port number</param>
        /// <param name="parent">reference to factory/listener</param>
        /// <param name="maxReceivedMessageSize">Some settings for transport channel</param>
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

            // Create port
            serialPort = new SerialPort();

            // Set the appropriate properties.
            serialPort.PortName = this.portNumber;
            serialPort.BaudRate = 9600; //TODO: Allow users to specify this
            serialPort.Parity = Parity.None; //TODO: Allow users to specify this
            serialPort.DataBits = 8; //TODO: Allow users to specify this
            serialPort.StopBits = StopBits.One; //TODO: Allow users to specify this
            serialPort.Handshake = Handshake.None; //TODO: Allow users to specify this

            // Set the read/write timeouts
            serialPort.ReadTimeout = 500; //TODO: Allow users to specify this
            serialPort.WriteTimeout = 500; //TODO: Allow users to specify this
        }

        /// <summary>
        /// Wrap general expections as communication exceptions
        /// </summary>
        /// <param name="exception">Exception to be wrapped</param>
        /// <returns>Return communication exception wrapped with general exception</returns>
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

        /// <summary>
        /// Remote address used to filter messages
        /// </summary>
        public EndpointAddress RemoteAddress
        {
            get { return this.address; }
        }

        /// <summary>
        /// Reference to Port. Used to handle open, close and other events in derived classes
        /// </summary>
        public SerialPort Port
        {
            get { return serialPort; }
        }

        /// <summary>
        /// Read Message
        /// </summary>
        /// <returns>Message object constructed using data wrapped as SOAP message</returns>
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

            ArraySegment<byte> buffer = BuildReply(Encoding.UTF8.GetString(data,0,bytesRead));
//            ArraySegment<byte> buffer = new ArraySegment<byte>(data, 0, (int)bytesRead);
            return this.encoder.ReadMessage(buffer, this.bufferManager);
        }

        /// <summary>
        /// Data written to SOAP body before transmitting
        /// </summary>
        /// <param name="message"></param>
        protected void WriteMessage(Message message)
        {
            ArraySegment<byte> buffer;
            using (message)
            {
                this.address.ApplyTo(message);
                buffer = this.encoder.WriteMessage(message, MaxBufferSize, this.bufferManager);

                lastMessage = message.ToString();
            }
            try
            {
                //Stream stream = serialPort.BaseStream;
                //stream.Write(buffer.Array, buffer.Offset, buffer.Count);

                MemoryStream ms = new MemoryStream(buffer.Array);
                ms.Position = 0;

                String bodyContent = null;

                XmlTextReader xtr = new XmlTextReader(ms);

                while (xtr.Read())
                {
                    if (xtr.Name == "request")
                    {
                        bodyContent = xtr.ReadElementContentAsString();
                    }
                }

                xtr.Close();
                ms.Close();

                Stream stream = serialPort.BaseStream;
                byte [] bytes = Encoding.UTF8.GetBytes(bodyContent);
                stream.Write(bytes, 0, bytes.Length);
            }
            catch (IOException exception)
            {
                throw ConvertException(exception);
            }
        }

        ArraySegment<byte> BuildReply(string reply)
        {
            string action = null;
            UniqueId messageId = null;
            Uri to = null;
            string ns = null;
            string responseId = null;
            string resultId = null;

            Uri replyId = null;

            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(lastMessage));

            XmlReader reader = XmlReader.Create(ms);

            Message message = Message.CreateMessage(MessageVersion.Soap12WSAddressing10,
                    null, reader);
            
            while (reader.Read())
            {
                if(reader.Name == "a:Action")
                    action = reader.ReadElementContentAsString("Action", "http://www.w3.org/2005/08/addressing");
                if (reader.Name == "a:MessageID")
                    messageId = new UniqueId(reader.ReadElementContentAsString("MessageID", "http://www.w3.org/2005/08/addressing"));
                if (reader.Name == "a:To")
                    to = new Uri(reader.ReadElementContentAsString("To", "http://www.w3.org/2005/08/addressing"));
            }
            message.Headers.Clear();

            message.Headers.Action = action + "Response";
            message.Headers.RelatesTo = messageId;
            message.Headers.To = to;

            replyId = new Uri(action);
            ns = replyId.Scheme + "://" + replyId.Host + "/";
            responseId = replyId.Segments[replyId.Segments.Length - 1] + "Response";
            resultId = replyId.Segments[replyId.Segments.Length - 1] + "Result";

            XElement bodyContent = new XElement(XName.Get(responseId, ns),
                    new XElement(XName.Get(resultId, ns), reply));

            lastMessage = message.ToString();

            XElement el = XElement.Parse(lastMessage);

            el.Element(XName.Get("Body", "http://www.w3.org/2003/05/soap-envelope")).RemoveAll();
            lastMessage = el.ToString();

            string body = "<s:Body>\n" + bodyContent.ToString() + "\n</s:Body>";
            lastMessage = lastMessage.Replace("<s:Body />", body);

            return new ArraySegment<byte>(Encoding.UTF8.GetBytes(lastMessage));
        }
    }
}
