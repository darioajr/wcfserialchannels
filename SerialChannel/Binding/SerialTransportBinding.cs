using System.ServiceModel.Channels;
using SerialChannel.Binding.Encoding;
using SerialChannel.Binding.Transport;

namespace SerialChannel.Binding
{
    /// <summary>
    /// Binding object used by service model.
    /// </summary>
    /// <remarks>Required for service model programming.</remarks>
    public class SerialTransportBinding : System.ServiceModel.Channels.Binding
    {
        readonly MessageEncodingBindingElement messageElement;
        readonly SerialTransportBindingElement transportElement;

        /// <summary>
        /// Default constructor. 
        /// Creates encoding and transport binding elements.
        /// </summary>
        public SerialTransportBinding(string port)
        {
          //this.messageElement = new TextMessageEncodingBindingElement();
          this.messageElement =
            new SerialEncoderBindingElement();
          this.transportElement = new SerialTransportBindingElement();
          this.transportElement.FactoryPort = port;
        }

        /// <summary>
        /// COM port to be used on client side.
        /// </summary>
        public override BindingElementCollection CreateBindingElements()
        {
            BindingElementCollection elements = new BindingElementCollection();
            elements.Add(this.messageElement);
            elements.Add(this.transportElement);
            return elements.Clone();
        }

        /// <summary>
        /// We use a scheme serial
        /// </summary>
        public override string Scheme
        {
            get { return this.transportElement.Scheme; }
        }
    }
}
