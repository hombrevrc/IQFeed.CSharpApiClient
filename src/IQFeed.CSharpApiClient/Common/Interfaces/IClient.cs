using System;

namespace IQFeed.CSharpApiClient.Common.Interfaces
{
    public interface IClient
    {
        event Action Connected;
        event Action Disconnected;
        void Connect();
        void Disconnect();
    }
}