using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.Messaging;

namespace TimeMeasurement_Backend.Networking
{
    /// <summary>
    /// Handles websocket connections with multiple viewers
    /// </summary>
    public class ViewerHandler : Handler<ViewerCommands>
    {
        /// <summary>
        /// All connected Viewers
        /// </summary>
        private readonly List<WebSocket> _viewers;

        public ViewerHandler()
        {
            _viewers = new List<WebSocket>();
            RaceManager.Instance.StateChanged += OnRaceManagerStateChanged;
        }

        /// <summary>
        /// Connect with viewer and listen for its messages
        /// </summary>
        /// <param name="viewer">The Websocket corrresponding to a viewer</param>
        /// <returns></returns>
        public async Task AddViewerAsync(WebSocket viewer)
        {
            _viewers.Add(viewer);
            await SendCurrentStateTo(viewer);
            await ListenAsync(viewer);
        }

        protected override void HandleMessage(WebSocket sender, Message<ViewerCommands> received)
        {
            //No messages yet
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Remove from active viewers
            _viewers.Remove(disconnected);
        }

        private void OnRaceManagerStateChanged(RaceManager.State prev, RaceManager.State current)
        {
            //Broadcast current state
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Status,
                Data = RaceManager.Instance.CurrentState
            };
            Task.Run(async () => await BroadcastMessageAsync(_viewers, toSend));

            //Broadcast additional data on certain states
            switch (current)
            {
                case RaceManager.State.InProgress:
                {
                    //Send start
                    var message = new Message<ViewerCommands>
                    {
                        Command = ViewerCommands.RunStart,
                        Data = new RunStart
                        {
                            StartTime = RaceManager.Instance.TimeMeter.StartTime,
                            CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                            Runners = RaceManager.Instance.Runners
                        }
                    };
                    Task.Run(async () => await BroadcastMessageAsync(_viewers, message));
                    break;
                }
                case RaceManager.State.Ready when prev == RaceManager.State.InProgress:
                {
                    //The race has ended
                    //Notfiy all viewers
                    var message = new Message<ViewerCommands>
                    {
                        Command = ViewerCommands.RunEnd,
                        Data = null
                    };
                    Task.Run(async () => await BroadcastMessageAsync(_viewers, message));
                    break;
                }
            }
        }

        /// <summary>
        /// Tell the viewer if time is being measured or not and why
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private async Task SendCurrentStateTo(WebSocket receiver)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Status,
                Data = RaceManager.Instance.CurrentState
            };
            await SendMessageAsync(receiver, toSend);

            if (RaceManager.Instance.CurrentState == RaceManager.State.InProgress)
            {
                //Send start
                var message = new Message<ViewerCommands>
                {
                    Command = ViewerCommands.RunStart,
                    Data = new RunStart
                    {
                        StartTime = RaceManager.Instance.TimeMeter.StartTime,
                        CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                        Runners = RaceManager.Instance.Runners
                    }
                };
                await SendMessageAsync(receiver, message);
            }
        }
    }
}