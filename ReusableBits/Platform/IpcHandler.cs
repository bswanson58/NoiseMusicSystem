using System;
using System.Text;
using System.Web.Script.Serialization;
using TinyIpc.Messaging;

namespace ReusableBits.Platform {
    public class BaseIpcMessage {
        public  string  From { get; set; }
        public  string  To { get; set; }
        public  string  Subject {  get; set; }
        public  string  Body { get; set; }

        public BaseIpcMessage() {
            From = String.Empty;
            To = String.Empty;
            Subject = String.Empty;
            Body = String.Empty;
        }

        public BaseIpcMessage( string from, string to, string subject, string body ) {
            From = from;
            To = to;
            Subject = subject;
            Body = body;
        }
    }

    public interface IIpcHandler {
        void Initialize( string identity, string channelName, Action<BaseIpcMessage> onReceive );

        void BroadcastMessage( string subject, string body );
        void SendMessage( string to, string subject, string body );
    }

    public class IpcHandler : IIpcHandler {
        public  const string                    cBroadcastMessage = "*";

        private readonly JavaScriptSerializer   mSerializer;
        private ITinyMessageBus                 mMessageBus;
        private Action<BaseIpcMessage>          mReceiveAction;
        private string                          mIdentity;

        public IpcHandler() {
            mIdentity = String.Empty;
            mSerializer = new JavaScriptSerializer();
        }

        public void Initialize( string identity, string channelName, Action<BaseIpcMessage> onReceive ) {
            mIdentity = identity;
            mReceiveAction = onReceive;

            mMessageBus = new TinyMessageBus( channelName );
            mMessageBus.MessageReceived += OnMessageReceived;
        }

        public void BroadcastMessage( string subject, string body ) {
            SendMessage( cBroadcastMessage, subject, body );
        }

        public void SendMessage( string to, string subject, string body ) {
            var message = new BaseIpcMessage( mIdentity, to, subject, body );
            var json = mSerializer.Serialize( message );

            mMessageBus.PublishAsync( Encoding.UTF8.GetBytes( json ));
        }

        private void OnMessageReceived( object sender, TinyMessageReceivedEventArgs args ) {
            var json = Encoding.UTF8.GetString( args.Message );
            var message = mSerializer.Deserialize<BaseIpcMessage>( json );

            if(( message.To.Equals( mIdentity )) ||
               ( message.To.Equals( cBroadcastMessage ))) {
                mReceiveAction?.Invoke( message );
            }
        }
    }
}
