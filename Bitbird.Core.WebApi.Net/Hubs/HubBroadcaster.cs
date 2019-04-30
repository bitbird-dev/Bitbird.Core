using System;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Bitbird.Core.WebApi.Net.Hubs
{
    /// <summary>
    /// Stores data that can be updated at any time and executes a broadcast action every given interval.
    /// This class supports SignalR-hubs that are in danger of sending updates to frequently.
    ///
    /// Does not provide locking mechanisms for the data.
    /// </summary>
    /// <typeparam name="THub">The hub-type that this broadcaster is for.</typeparam>
    /// <typeparam name="THubClient">The client-interface for the hub that this broadcaster is for.</typeparam>
    /// <typeparam name="TData">The data this broadcaster should store (and is then used for broadcasting).</typeparam>
    public class HubBroadcaster<THub, THubClient, TData> : IDisposable
        where THub : IHub
        where THubClient : class
        where TData: class
    {
        private readonly TData data;
        private readonly Action<IHubContext<THubClient>, TData> broadcastAction;
        private readonly IHubContext<THubClient> hubContext;
        private readonly Timer broadcastLoopTimer;

        /// <summary>
        /// Constructs a new <see cref="HubBroadcaster{THub,THubClient,TData}"/>.
        /// </summary>
        /// <param name="interval">The interval in which to broadcast.</param>
        /// <param name="initData">The initial data that this broadcaster stores.</param>
        /// <param name="broadcastAction">The action which to execute on every broadcast.</param>
        public HubBroadcaster(TimeSpan interval, TData initData, Action<IHubContext<THubClient>, TData> broadcastAction)
        {
            data = initData;
            this.broadcastAction = broadcastAction;
            hubContext = GlobalHost.ConnectionManager.GetHubContext<THub, THubClient>();
            broadcastLoopTimer = new Timer(BroadcastCallback, null, interval, interval);
        }
        /// <inheritdoc />
        public void Dispose()
        {
            broadcastLoopTimer?.Dispose();
        }
        /// <summary>
        /// Called every interval.
        /// Calls the broadcast action that was passed to the constructor.
        /// </summary>
        /// <param name="state">Used by the timer class. Ignored.</param>
        private void BroadcastCallback(object state)
        {
            broadcastAction(hubContext, data);
        }
        /// <summary>
        /// Updates the data.
        /// Does not mark the data as dirty.
        /// The broadcast action is executed whether this method is called or not.
        /// </summary>
        /// <param name="updateAction">An action that updates the data object.</param>
        public void UpdateData(Action<TData> updateAction)
        {
            updateAction(data);
        }
    }
}