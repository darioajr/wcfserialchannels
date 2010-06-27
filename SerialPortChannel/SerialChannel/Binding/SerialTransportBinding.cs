using System.ServiceModel.Channels;
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
        /// Default constructor. Creates encoding and transport binding elements.
        /// </summary>
        public SerialTransportBinding()
        {
            this.messageElement = new TextMessageEncodingBindingElement();
            this.transportElement = new SerialTransportBindingElement();
        }

        /// <summary>
        /// COM port to be used on client side.
        /// </summary>
        public string FactoryPort
        {
            set { this.transportElement.FactoryPort = value; }
        }

        /// <summary>
        /// COM port to be used on server side.
        /// </summary>
        public string ListenerPort
        {
            set { this.transportElement.ListenerPort = value; }
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
        /// We use a scheme like serial://localhost/???
        /// </summary>
        public override string Scheme
        {
            get { return this.transportElement.Scheme; }
        }
    }
}
