using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IQFeed.CSharpApiClient.Common.Interfaces;
using IQFeed.CSharpApiClient.Socket;

namespace IQFeed.CSharpApiClient.Lookup
{
    public class LookupDispatcher
    {
        public event Action Connected;
        public event Action Disconnected;

        private readonly SemaphoreSlim _semaphoreSlim;
        private readonly List<SocketClient> _socketClients;
        private readonly HashSet<SocketClient> _connectedSocketClients;
        private readonly Queue<SocketClient> _availableSocketClients;
        private readonly string _protocol;
        private readonly IRequestFormatter _requestFormatter;

        public LookupDispatcher(string host, int port, int bufferSize, string protocol, int numberOfClients, IRequestFormatter requestFormatter)
        {
            _protocol = protocol;
            _semaphoreSlim = new SemaphoreSlim(0, numberOfClients);
            _socketClients = new List<SocketClient>(CreateSocketClients(host, port, bufferSize, numberOfClients));
            _availableSocketClients = new Queue<SocketClient>();
            _connectedSocketClients = new HashSet<SocketClient>();
            _requestFormatter = requestFormatter;
        }

        private IEnumerable<SocketClient> CreateSocketClients(string host, int port, int bufferSize, int numberOfClients)
        {
            for (var i = 0; i < numberOfClients; i++)
            {
                var socketClient = new SocketClient(host, port, bufferSize);
                socketClient.MessageReceived += OnMessageReceived;
                socketClient.Connected += OnConnected;
                socketClient.Disconnected += OnDisconnected;
                yield return socketClient;
            }
        }

        public void ConnectAll()
        {
            foreach (var socketClient in _socketClients)
            {
                socketClient.Connect();
            }
        }

        public void DisconnectAll()
        {
            foreach (var socketClient in _socketClients)
            {
                socketClient.Disconnect();
            }
        }

        public async Task<SocketClient> TakeAsync()
        {
            await _semaphoreSlim.WaitAsync();
            lock (_availableSocketClients)
            {
                return _availableSocketClients.Dequeue();
            }
        }

        public void Add(SocketClient socketClient)
        {
            lock (_availableSocketClients)
            {
                _availableSocketClients.Enqueue(socketClient);
            }
            _semaphoreSlim.Release();
        }

        private void OnConnected(object sender, EventArgs eventArgs)
        {
            var socketClient = (SocketClient)sender;
            socketClient.Send(_requestFormatter.SetProtocol(_protocol));
            _connectedSocketClients.Add(socketClient);

            if (_connectedSocketClients.Count == _socketClients.Count)
                Connected?.Invoke();
        }

        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            var socketClient = (SocketClient)sender;
            _connectedSocketClients.Remove(socketClient);

            if (_connectedSocketClients.Count == _socketClients.Count - 1)
                Disconnected?.Invoke();
        }

        private void OnMessageReceived(object sender, SocketMessageEventArgs socketMessageEventArgs)
        {
            var socketClient = (SocketClient)sender;
            socketClient.MessageReceived -= OnMessageReceived;  // TODO: validate the protocol confirmation
            Add(socketClient);
        }
    }
}