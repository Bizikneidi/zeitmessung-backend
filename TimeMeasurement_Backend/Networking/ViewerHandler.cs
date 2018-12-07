using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Entities;
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
            RaceManager.Instance.RunnerFinished += OnRaceManagerRunnerFinished;
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
            await SendRaces(viewer);
            await ListenAsync(viewer);
        }

        protected override void HandleMessage(WebSocket sender, Message<ViewerCommands> received)
        {
            if (received.Command == ViewerCommands.GetRunners)
            {
                SendRacersTo(sender, (int)(long)received.Data);
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Remove from active viewers
            _viewers.Remove(disconnected);
        }

        private void OnRaceManagerRunnerFinished(Runner runner)
        {
            //Broadcast finished runner
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RunnerFinished,
                Data = new AssignmentDTO
                {
                    Time = runner.Time,
                    Starter = runner.Starter
                }
            };
            Task.Run(async () => await BroadcastMessageAsync(_viewers, toSend));
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

            switch (current)
            {
                //Broadcast additional data if in progress
                case RaceManager.State.InProgress:
                {
                    //Send start
                    var message = new Message<ViewerCommands>
                    {
                        Command = ViewerCommands.RunStart,
                        Data = new RunStartDTO
                        {
                            StartTime = RaceManager.Instance.TimeMeter.StartTime,
                            CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                            Runners = RaceManager.Instance.CurrentRunners
                        }
                    };
                    Task.Run(async () => await BroadcastMessageAsync(_viewers, message));
                    break;
                }
                case RaceManager.State.Ready when prev == RaceManager.State.InProgress:
                case RaceManager.State.Disabled when prev == RaceManager.State.InProgress:
                    {
                    //Send end
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
        /// Tell the viewer the current state of the race
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        private async Task SendCurrentStateTo(WebSocket receiver)
        {
            //Send status
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
                    Data = new RunStartDTO
                    {
                        StartTime = RaceManager.Instance.TimeMeter.StartTime,
                        CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                        Runners = RaceManager.Instance.CurrentRunners
                    }
                };
                await SendMessageAsync(receiver, message);
            }
        }

        private void SendRacersTo(WebSocket receiver, int raceId)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Runners,
                Data = RaceManager.Instance.GetRunners(raceId)
            };
            Task.Run(async () => await SendMessageAsync(receiver, toSend));
        }

        private async Task SendRaces(WebSocket receiver)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Races,
                Data = RaceManager.Instance.Races
            };
            await SendMessageAsync(receiver, toSend);
        }
    }
}