using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TimeMeasurement_Backend.Entities;
using TimeMeasurement_Backend.Logic;
using TimeMeasurement_Backend.Networking.MessageData;

namespace TimeMeasurement_Backend.Networking.Handlers
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
            RaceManager.Instance.RunnerFinished += BroadcastRunnerFinished;
        }

        /// <summary>
        /// Connect with viewer and listen for its messages
        /// </summary>
        /// <param name="viewer">The Websocket corrresponding to a viewer</param>
        /// <returns></returns>
        public async Task AddViewerAsync(WebSocket viewer)
        {
            _viewers.Add(viewer);
            SendCurrentStateTo(viewer);
            if (RaceManager.Instance.CurrentState == RaceManager.State.InProgress)
            {
                SendRaceStartTo(viewer);
            }

            SendRacesTo(viewer);
            await ListenAsync(viewer);
        }

        protected override void HandleMessage(WebSocket sender, Message<ViewerCommands> received)
        {
            if (received.Command == ViewerCommands.GetRunners)
            {
                SendRunnersTo(sender, (int)(long)received.Data);
            }
        }

        protected override void OnDisconnect(WebSocket disconnected)
        {
            //Remove from active viewers
            _viewers.Remove(disconnected);
        }

        /// <summary>
        /// Broadcasts the current race state
        /// </summary>
        private void BroadcastCurrentState()
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.State,
                Data = RaceManager.Instance.CurrentState
            };
            BroadcastMessage(_viewers, toSend);
        }

        /// <summary>
        /// Broadcasts the end of a race
        /// </summary>
        private void BroadcastRaceEnd()
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RaceEnd,
                Data = null
            };
            BroadcastMessage(_viewers, toSend);
        }

        /// <summary>
        /// Broadcasts all races
        /// </summary>
        private void BroadcastRaces()
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Races,
                Data = RaceManager.Instance.Races
            };
            BroadcastMessage(_viewers, toSend);
        }

        /// <summary>
        /// Broadcasts the start of a race
        /// </summary>
        private void BroadcastRaceStart()
        {
            var message = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RaceStart,
                Data = new RaceStartDTO
                {
                    StartTime = RaceManager.Instance.TimeMeter.StartTime,
                    CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                    Runners = RaceManager.Instance.CurrentRunners
                }
            };
            BroadcastMessage(_viewers, message);
        }

        /// <summary>
        /// Broadcasts that a runner has finished the race
        /// </summary>
        /// <param name="runner">the runner who finished the race</param>
        private void BroadcastRunnerFinished(Runner runner)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RunnerFinished,
                Data = new AssignmentDTO
                {
                    Time = runner.Time,
                    Starter = runner.Starter
                }
            };
            BroadcastMessage(_viewers, toSend);
        }

        private void OnRaceManagerStateChanged(RaceManager.State prev, RaceManager.State current)
        {
            BroadcastCurrentState();

            if (current == RaceManager.State.InProgress)
            {
                BroadcastRaceStart();
            }

            if (prev == RaceManager.State.InProgress)
            {
                BroadcastRaceEnd();
                BroadcastRaces();
            }
        }

        /// <summary>
        /// Send the current state to a viewer
        /// <param name="receiver">the viewer to notify</param>
        /// </summary>
        private void SendCurrentStateTo(WebSocket receiver)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.State,
                Data = RaceManager.Instance.CurrentState
            };
            SendMessage(receiver, toSend);
        }

        /// <summary>
        /// Send current race start data to a viewer
        /// </summary>
        /// <param name="receiver">the viewer to send them to</param>
        private void SendRaceStartTo(WebSocket receiver)
        {
            var message = new Message<ViewerCommands>
            {
                Command = ViewerCommands.RaceStart,
                Data = new RaceStartDTO
                {
                    StartTime = RaceManager.Instance.TimeMeter.StartTime,
                    CurrentTime = RaceManager.Instance.TimeMeter.ApproximatedCurrentTime,
                    Runners = RaceManager.Instance.CurrentRunners
                }
            };
            SendMessage(receiver, message);
        }

        /// <summary>
        /// Send all races to a viewer
        /// </summary>
        /// <param name="receiver">the viewer to send them to</param>
        private void SendRacesTo(WebSocket receiver)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Races,
                Data = RaceManager.Instance.Races
            };
            SendMessage(receiver, toSend);
        }

        /// <summary>
        /// Send all runners for a race to a viewer
        /// </summary>
        /// <param name="receiver">the viewer who requested the runners</param>
        /// <param name="raceId">the id of the race</param>
        private void SendRunnersTo(WebSocket receiver, int raceId)
        {
            var toSend = new Message<ViewerCommands>
            {
                Command = ViewerCommands.Runners,
                Data = RaceManager.Instance.GetRunners(raceId)
            };
            SendMessage(receiver, toSend);
        }
    }
}