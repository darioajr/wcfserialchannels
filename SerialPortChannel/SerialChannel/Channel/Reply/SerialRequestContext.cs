using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace SerialChannel.Channel.Reply
{
    partial class SerialReplyChannel
    {
        partial class SerialRequestContext : RequestContext
        {
            bool aborted;
            readonly Message message;
            readonly SerialReplyChannel parent;
            CommunicationState state;
            readonly object thisLock;
            readonly object writeLock;

            public SerialRequestContext(Message message, SerialReplyChannel parent)
            {
                this.aborted = false;
                this.message = message;
                this.parent = parent;
                this.state = CommunicationState.Opened;
                this.thisLock = new object();
                this.writeLock = new object();
            }


            public override void Abort()
            {
                lock (thisLock)
                {
                    if (this.aborted)
                    {
                        return;
                    }
                    this.aborted = true;
                    this.state = CommunicationState.Faulted;
                }
            }

            public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (timeout == null)
                    return BeginReply(message, this.parent.DefaultSendTimeout, callback, state);
                else
                    return BeginReply(message, timeout, callback, state);
            }

            public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
            {
                return BeginReply(message, this.parent.DefaultSendTimeout, callback, state);
            }

            public override void Close(TimeSpan timeout)
            {
                lock (thisLock)
                {
                    this.state = CommunicationState.Closed;
                }
            }

            public override void Close()
            {
                Close(this.parent.DefaultCloseTimeout);
            }

            public override void EndReply(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public override void Reply(Message message, TimeSpan timeout)
            {
                lock (thisLock)
                {
                    if (this.aborted)
                    {
                        throw new CommunicationObjectAbortedException();
                    }
                    if (this.state == CommunicationState.Faulted)
                    {
                        throw new CommunicationObjectFaultedException();
                    }
                    if (this.state == CommunicationState.Closed)
                    {
                        throw new ObjectDisposedException("this");
                    }
                }
                this.parent.ThrowIfDisposedOrNotOpen();
                lock (writeLock)
                {
                    this.parent.WriteMessage(message);
                }
            }

            public override void Reply(Message message)
            {
                Reply(message, this.parent.DefaultSendTimeout);
            }

            public override Message RequestMessage
            {
                get { return message; }
            }

            public void Dispose()
            {
                Close();
            }

        }
    }
}
