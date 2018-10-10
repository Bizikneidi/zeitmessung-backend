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
            TimeMeter.Instance.StateChanged += OnTimeMeterStateChanged;
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
            //Simply listen for messages
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

        private void OnTimeMeterStateChanged(TimeMeter.State prev, TimeMeter.State current)
        {
            switch (current)
            {
                case TimeMeter.State.Measuring:
                {
                    //The time meter has started a measurement
                    //Notfiy all viewers
                    var message = new Message<ViewerCommands>
                    {
                        Command = ViewerCommands.MeasuredStart,
                        Data = new MeasurementStart
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            StartTime = (long)TimeMeter.Instance.Measurement.Start,
                            CurrentTime = TimeMeter.Instance.ApproximatedCurrentTime
                        }
                    };
                    Task.Run(async () => await BroadcastMessageAsync(_viewers, message));
                    break;
                }
                case TimeMeter.State.Ready when prev == TimeMeter.State.Measuring:
                {
                    //The time meter has completed a measurement
                    //Notfiy all viewers
                    var message = new Message<ViewerCommands>
                    {
                        Command = ViewerCommands.MeasuredStop,
                        Data = TimeMeter.Instance.Measurement.End
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
                Data = TimeMeter.Instance.CurrentState
            };
            await SendMessageAsync(receiver, toSend);

            //If time is being measured
            if (TimeMeter.Instance.CurrentState == TimeMeter.State.Measuring)
            {
                //Send start time and current station time to allow client to calculate time differences and run local timer
                var message = new Message<ViewerCommands>
                {
                    Command = ViewerCommands.MeasuredStart,
                    Data = new MeasurementStart
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        StartTime = (long)TimeMeter.Instance.Measurement.Start,
                        CurrentTime = TimeMeter.Instance.ApproximatedCurrentTime
                    }
                };
                await SendMessageAsync(receiver, message);
            }
        }

        /// <summary>
        /// entity to store the start and current time for state - and measurment start messages
        /// Can be used by viewers to calculate time differences and run local timer
        /// </summary>
        private class MeasurementStart
        {
            /// <summary>
            /// The current time of the station
            /// </summary>
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public long CurrentTime { get; set; }

            /// <summary>
            /// The start time of the station
            /// </summary>
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public long StartTime { get; set; }
        }
    }
}