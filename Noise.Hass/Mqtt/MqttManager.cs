using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using Newtonsoft.Json;
using Noise.Hass.Context;
using OneOf;
using OneOf.Types;

// ReSharper disable IdentifierTypo

namespace Noise.Hass.Mqtt {
    public interface IJsonPayload { }

    public enum MqttStatus {
        Uninitialized,
        Connected,
        Connecting,
        Disconnected,
        Error
    }

    public interface IMqttManager : IDisposable {
        MqttStatus                      Status { get; }
        string                          StatusMessage { get; }
        bool                            IsConnected { get; }
        IObservable<MqttStatus>         OnStatusChanged { get; }

        IObservable<MqttMessage>        OnMessageReceived { get; }
        IObservable<MqttMessage>        OnMessageProcessed {  get; }

        Task<OneOf<None, Exception>>    PublishAsync( string topic, string payload );
        Task<OneOf<None, Exception>>    PublishAsync( string topic, IJsonPayload payload );
        Task<OneOf<None, Exception>>    PublishAsync( MqttApplicationMessage message );

        Task<OneOf<None, Exception>>    SubscribeAsync( string topic );
        Task<OneOf<None, Exception>>    SubscribeAsync( IList<string> topics );
        Task<OneOf<None, Exception>>    UnsubscribeAsync( string topic );
    }

    public class MqttManager : IMqttManager {
        private readonly MqttFactory                    mClientFactory;
        private readonly Subject<MqttMessage>           mReceivedMessageSubject;
        private readonly Subject<MqttMessage>           mProcessedMessageSubject;
        private readonly BehaviorSubject<MqttStatus>    mStatusSubject;
        private readonly IDisposable                    mContextSubscription;
        private IManagedMqttClient                      mClient;

        public  bool                                    IsConnected => mClient?.IsConnected == true;

        public  MqttStatus                              Status { get; private set; }
        public  string                                  StatusMessage { get; private set; }

        public  IObservable<MqttMessage>    OnMessageReceived => mReceivedMessageSubject.AsObservable();
        public  IObservable<MqttMessage>    OnMessageProcessed => mProcessedMessageSubject.AsObservable();
        public  IObservable<MqttStatus>     OnStatusChanged => mStatusSubject.AsObservable();

        public MqttManager( MqttFactory clientFactory, IHassContextProvider contextProvider ) {
            mClientFactory = clientFactory;

            mReceivedMessageSubject = new Subject<MqttMessage>();
            mProcessedMessageSubject = new Subject<MqttMessage>();

            Status = MqttStatus.Uninitialized;
            StatusMessage = "Uninitialized";
            mStatusSubject = new BehaviorSubject<MqttStatus>( Status );

            mContextSubscription = contextProvider.OnContextChanged.Subscribe( OnContextChanged );
        }

        private void CreateClient() {
            DisposeExistingClient();

            mClient = mClientFactory.CreateManagedMqttClient();

            mClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync; 
            mClient.ApplicationMessageProcessedAsync += OnApplicationMessageProcessedAsync;
            mClient.ConnectedAsync += OnConnectedAsync;
            mClient.ConnectingFailedAsync += OnConnectingFailedAsync;
            mClient.DisconnectedAsync += OnDisconnectedAsync;
        }

        private void UpdateStatus( MqttStatus status, string statusMessage = "" ) {
            Status = status;
            StatusMessage = statusMessage;

            mStatusSubject.OnNext( status );
        }

        private async void OnContextChanged( IHassClientContext context ) {
            await ConnectAsync( context );
        }

        private async Task ConnectAsync( IHassClientContext context ) {
            try {
                if( mClient?.IsConnected == true ) {
                    await Disconnect();

                    // wait while connecting
                    while( mClient.IsConnected ) {
                        await Task.Delay( 250 );
                    } 
                }

                if( context.MqttEnabled ) {
                    UpdateStatus( MqttStatus.Connecting );

                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId( context.DeviceConfiguration.Identifiers )
                        .WithTcpServer( context.ServerAddress )
                        .WithCleanSession()
                        .WithKeepAlivePeriod( TimeSpan.FromSeconds( 15 ));

                    if(!String.IsNullOrWhiteSpace( context.UserName )) {
                        mqttClientOptions
                            .WithCredentials( context.UserName, context.Password );
                    }

                    if(!String.IsNullOrWhiteSpace( context.LastWillTopic )) {
                        mqttClientOptions
                            .WithWillTopic( context.LastWillTopic )
                            .WithWillPayload( context.LastWillPayload )
                            .WithWillRetain( context.UseMqttRetainFlag );
                    }

                    var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                        .WithClientOptions( mqttClientOptions.Build())
                        .Build();

                    CreateClient();

                    if( mClient != null ) {
                        await mClient.StartAsync( managedMqttClientOptions );
                    }
                }
                else {
                    UpdateStatus( MqttStatus.Disconnected );
                }
            }
            catch( Exception ex ) {
                UpdateStatus( MqttStatus.Error, ex.Message );
            }
        }

        private Task OnConnectingFailedAsync( ConnectingFailedEventArgs arg ) {
            UpdateStatus( MqttStatus.Error, arg.Exception?.Message ?? String.Empty );

            return Task.CompletedTask;
        }

        private Task OnConnectedAsync( MqttClientConnectedEventArgs arg ) {
            UpdateStatus( MqttStatus.Connected, arg.ConnectResult.ReasonString ?? String.Empty );

            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync( MqttClientDisconnectedEventArgs arg ) {
            UpdateStatus( MqttStatus.Error, arg.Exception?.Message ?? String.Empty );

            return Task.CompletedTask;
        }

        private async Task OnApplicationMessageReceivedAsync( MqttApplicationMessageReceivedEventArgs arg ) {
            mReceivedMessageSubject.OnNext( new MqttMessage( arg.ApplicationMessage ));

            await arg.AcknowledgeAsync( CancellationToken.None );
        }

        private Task OnApplicationMessageProcessedAsync( ApplicationMessageProcessedEventArgs arg ) {
            mProcessedMessageSubject.OnNext( new MqttMessage( arg.ApplicationMessage ));

            return Task.CompletedTask;
        }

        public Task<OneOf<None, Exception>> PublishAsync( string topic, IJsonPayload payload ) =>
            PublishAsync( topic, JsonConvert.SerializeObject( payload ));

        public async Task<OneOf<None, Exception>> PublishAsync( MqttApplicationMessage message ) {
            if( mClient != null ) {
                await mClient.EnqueueAsync( message );
            }

            return new None();
        }

        public async Task<OneOf<None, Exception>> PublishAsync( string topic, string payload ) {
            if( mClient != null ) {
                var messageFactory = mClientFactory.CreateApplicationMessageBuilder();
                var message = messageFactory
                    .WithTopic( topic )
                    .WithPayload( payload )
                    .Build();

                await mClient.EnqueueAsync( message );
            }

            return new None();
        }

        public async Task<OneOf<None, Exception>> SubscribeAsync( string topic ) =>
            await SubscribeAsync( new List<string>{ topic });

        public async Task<OneOf<None, Exception>> SubscribeAsync( IList<string> topics ) {
            if( mClient != null ) {
                var topicFilters = topics.Select( t => new MqttTopicFilter{ Topic = t }).ToList();

                await mClient.SubscribeAsync( topicFilters );
            }

            return new None();
        }

        public async Task<OneOf<None, Exception>> UnsubscribeAsync( string topic ) {
            if( mClient != null ) {
                await mClient.UnsubscribeAsync( topic );
            }

            return new None();
        }

        private async Task Disconnect() {
            if( mClient != null ) {
                await mClient.StopAsync();
            }

            UpdateStatus( MqttStatus.Disconnected );
        }

        private void DisposeExistingClient() {
            if( mClient != null ) {
                mClient.ApplicationMessageReceivedAsync -= OnApplicationMessageReceivedAsync; 
                mClient.ApplicationMessageProcessedAsync -= OnApplicationMessageProcessedAsync;
                mClient.ConnectedAsync -= OnConnectedAsync;
                mClient.ConnectingFailedAsync -= OnConnectingFailedAsync;
                mClient.DisconnectedAsync -= OnDisconnectedAsync;

                mClient.Dispose();
            }
        }

        public async void Dispose() {
            mContextSubscription.Dispose();

            await Disconnect();

            DisposeExistingClient();
        }
    }
}
