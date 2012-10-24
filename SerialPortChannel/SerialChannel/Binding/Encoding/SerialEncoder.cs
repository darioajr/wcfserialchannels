using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.IO;

namespace SerialChannel.Binding.Encoding
{
  class SerialEncoder : MessageEncoder
  {
    readonly SerialEncoderBindingElement bindingElement;
    readonly MessageEncoder innerEncoder;

    public SerialEncoder(SerialEncoderBindingElement bindingElement,
      MessageEncoder innerEncoder)
    {
      this.bindingElement = bindingElement;
      this.innerEncoder = innerEncoder;
    }

    public override string ContentType
    {
      get { return this.innerEncoder.ContentType; }
    }

    public override string MediaType
    {
      get { return this.innerEncoder.MediaType; }
    }

    public override MessageVersion MessageVersion
    {
      get { return this.innerEncoder.MessageVersion; }
    }


    public override Message ReadMessage(ArraySegment<byte> buffer,
      BufferManager bufferManager, string contentType)
    {
      Message message = this.innerEncoder.ReadMessage(buffer, bufferManager);
      return message;
    }

    public override Message ReadMessage(Stream stream,
  int maxSizeOfHeaders, string contentType)
    {
      throw new NotImplementedException("Streamed mode not implemented");
    }

    public override ArraySegment<byte> WriteMessage(Message message,
      int maxMessageSize, BufferManager bufferManager, int messageOffset)
    {
      ArraySegment<byte> buffer =
        innerEncoder.WriteMessage(message, maxMessageSize,
        bufferManager, messageOffset);
      return buffer;
    }

    public override void WriteMessage(Message message, Stream stream)
    {
      throw new NotImplementedException("Streamed mode not implemented");
    }
  }
}
