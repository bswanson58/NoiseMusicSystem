namespace Noise.RemoteClient.Dto {
    enum eClientState {
        Unknown,
        Starting,
        Sleeping,
    }
    class ClientStatus {
        public  eClientState    ClientState { get; }

        public ClientStatus( eClientState state ) {
            ClientState = state;
        }
    }
}
