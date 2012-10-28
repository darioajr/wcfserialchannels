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
      lastMessage = message.ToString();

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
      string action = null;
      UniqueId messageId = null;
      Uri to = null;
      string ns = null;
      string responseId = null;
      string resultId = null;
      Uri replyId = null;
      string reply = System.Text.Encoding.UTF8.GetString(buffer.Array,
        buffer.Offset,buffer.Count);

      MemoryStream ms = new MemoryStream(
        System.Text.Encoding.UTF8.GetBytes(lastMessage));

      XmlReader reader = XmlReader.Create(ms);

      Message message = Message.CreateMessage
        (this.MessageVersion, null, reader);

      while (reader.Read())
      {
        if (reader.Name == "a:Action")
          action = reader.ReadElementContentAsString("Action",
            "http://www.w3.org/2005/08/addressing");
        if (reader.Name == "a:MessageID")
          messageId = new UniqueId(
            reader.ReadElementContentAsString("MessageID",
            "http://www.w3.org/2005/08/addressing"));
        if (reader.Name == "a:To")
          to = new Uri(reader.ReadElementContentAsString("To",
            "http://www.w3.org/2005/08/addressing"));
      }
      message.Headers.Clear();

      message.Headers.Action = action + "Response";
      message.Headers.RelatesTo = messageId;
      message.Headers.To = to;

      replyId = new Uri(action);
      ns = replyId.Scheme + "://" + replyId.Host + "/";
      responseId =
        replyId.Segments[replyId.Segments.Length - 1] + "Response";
      resultId =
        replyId.Segments[replyId.Segments.Length - 1] + "Result";

      XElement bodyContent = new XElement(XName.Get(responseId, ns),
              new XElement(XName.Get(resultId, ns), reply));

      lastMessage = message.ToString();

      XElement el = XElement.Parse(lastMessage);

      el.Element(XName.Get("Body",
        "http://www.w3.org/2003/05/soap-envelope")).RemoveAll();
      lastMessage = el.ToString();

      string body = "<s:Body>\n" + bodyContent.ToString() + "\n</s:Body>";
      lastMessage = lastMessage.Replace("<s:Body />", body);

      return new ArraySegment<byte>(
        System.Text.Encoding.UTF8.GetBytes(lastMessage));
    }
  }
}
