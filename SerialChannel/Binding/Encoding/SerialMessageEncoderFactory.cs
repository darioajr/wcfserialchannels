﻿using System.ServiceModel.Channels;

namespace SerialChannel.Binding.Encoding
{

  class SerialMessageEncoderFactory : MessageEncoderFactory
  {
    readonly SerialMessageEncoder encoder;

    internal SerialMessageEncoderFactory()
    {
      this.encoder = new SerialMessageEncoder(this);
    }

    public override MessageEncoder Encoder
    {
      get { return this.encoder; }
    }

    public string ContentType
    {
      get { return string.Empty; }
    }

    public string MediaType
    {
      get { return string.Empty; }
    }

    public override MessageVersion MessageVersion
    {
      get { return MessageVersion.Default; }
    }
  }
}
