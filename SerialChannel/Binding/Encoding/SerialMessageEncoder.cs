using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Linq;

namespace SerialChannel.Binding.Encoding
{
  class SerialMessageEncoder : MessageEncoder
  {
    readonly SerialMessageEncoderFactory factory;
    static string lastMessage = null;

    public SerialMessageEncoder(SerialMessageEncoderFactory factory)
    {
      this.factory = factory;
    }

    public override string ContentType
    {
      get { return this.factory.ContentType; }
    }

    public override string MediaType
    {
      get { return this.factory.MediaType; }
    }

    public override MessageVersion MessageVersion
    {
      get { return this.factory.MessageVersion; }
    }

    public override Message ReadMessage(ArraySegment<byte> buffer,
      BufferManager bufferManager, string contentType)
    {
      ArraySegment<byte> data = BuildReply(buffer);

      bufferManager.ReturnBuffer(buffer.Array);

      MemoryStream stream = new MemoryStream(data.Array);
      return ReadMessage(stream, int.MaxValue);
    }

    public override Message ReadMessage(Stream stream, int maxSizeOfHeaders,
      string contentType)
    {
      XmlReader reader = XmlReader.Create(stream);
      return Message.CreateMessage(reader, maxSizeOfHeaders,
        this.MessageVersion);
    }

    public override ArraySegment<byte> WriteMessage(Message message,
      int maxMessageSize, BufferManager bufferManager, int messageOffset)
    {
      String bodyContent = null;

      lastMessage = message.Headers.Action;

      XmlDictionaryReader reader = message.GetReaderAtBodyContents();
      while (reader.Read())
      {
        if (reader.Name == "request")
        {
          bodyContent = reader.ReadElementContentAsString();
          break;
        }
      }
      reader.Close();

      byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(bodyContent);

      int totalLength = messageBytes.Length + messageOffset;
      byte[] totalBytes = bufferManager.TakeBuffer(totalLength);
      Array.Copy(messageBytes, 0,
        totalBytes, messageOffset, messageBytes.Length);

      ArraySegment<byte> buffer =
        new ArraySegment<byte>(
          totalBytes, messageOffset, messageBytes.Length);

      return buffer;
    }

    public override void WriteMessage(Message message, Stream stream)
    {
      throw new NotImplementedException("Streamed mode not implemented");
    }


    /// <summary>
    /// Helper function.
    /// TODO: major refactoring
    /// </summary>
    /// <param name="reply"></param>
    /// <returns></returns>
    ArraySegment<byte> BuildReply(ArraySegment<byte> buffer)
    {
      Uri replyId = null;
      string ns = null;
      string operation = null;
      
      string reply = System.Text.Encoding.UTF8.GetString(buffer.Array,
        buffer.Offset,buffer.Count);

      Message message = 
        Message.CreateMessage(this.MessageVersion, string.Empty);

      replyId = new Uri(lastMessage);
      operation = replyId.Segments[replyId.Segments.Length - 1];

      ns = replyId.Scheme + "://" + replyId.Host + "/";//TODO:Handle custom ns

      XElement body = 
        new XElement(XName.Get(operation + "Response", ns),
              new XElement(XName.Get(operation + "Result", ns), reply));

      return new ArraySegment<byte>(
        System.Text.Encoding.UTF8.GetBytes(body.ToString()));
    }
  }
}
