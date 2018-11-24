using System;
using IQFeed.CSharpApiClient.Common.Interfaces;
using IQFeed.CSharpApiClient.Lookup.Chains;
using IQFeed.CSharpApiClient.Lookup.Historical;
using IQFeed.CSharpApiClient.Lookup.News;
using IQFeed.CSharpApiClient.Lookup.Symbol;

namespace IQFeed.CSharpApiClient.Lookup
{
    public class LookupClient: IClient
    {
        public event Action Connected
        {
            add => _lookupDispatcher.Connected += value;
            remove => _lookupDispatcher.Connected -= value;
        }
        public event Action Disconnected
        {
            add => _lookupDispatcher.Disconnected += value;
            remove => _lookupDispatcher.Disconnected -= value;
        }

        private readonly LookupDispatcher _lookupDispatcher;

        public LookupClient(
            LookupDispatcher lookupDispatcher,
            HistoricalFacade historical, 
            NewsFacade news, 
            SymbolFacade symbol,
            ChainsFacade chains)
        {
            _lookupDispatcher = lookupDispatcher;
            Historical = historical;
            News = news;
            Symbol = symbol;
            Chains = chains;
        }

        public HistoricalFacade Historical { get; }
        public NewsFacade News { get; }
        public ISymbolFacade Symbol { get; }
        public ChainsFacade Chains { get; }

        public void Connect()
        {
            _lookupDispatcher.ConnectAll();
        }

        public void Disconnect()
        {
            _lookupDispatcher.DisconnectAll();
        }
    }
}