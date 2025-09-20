using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RVA.Client.Commands
{
    /// <summary>
    /// Command for simulating rafting state transitions through all possible states
    /// </summary>
    public class SimulateRaftingCommand : IUndoableCommand
    {
        private readonly WcfServiceClient _serviceClient;
        private readonly RaftingDto _rafting;
        private readonly RaftingState _originalState;
        private readonly DateTime _originalModifiedDate;
        private readonly Action<string> _statusUpdateCallback;
        private DispatcherTimer _simulationTimer;
        private List<RaftingState> _stateSequence;
        private int _currentStateIndex;
        private bool _isSimulationRunning;

        public string Description { get; }
        public DateTime ExecutedAt { get; private set; }

        // Define the logical state progression for rafting
        private static readonly RaftingState[] StateProgression =
        {
            RaftingState.Planned,
            RaftingState.Boarding,
            RaftingState.Paddling,
            RaftingState.Resting,
            RaftingState.Paddling,
            RaftingState.Resting,
            RaftingState.Paddling,
            RaftingState.Finished
        };

        public SimulateRaftingCommand(WcfServiceClient serviceClient, RaftingDto rafting, Action<string> statusUpdateCallback)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _rafting = rafting ?? throw new ArgumentNullException(nameof(rafting));
            _statusUpdateCallback = statusUpdateCallback;
            _originalState = rafting.CurrentState;
            _originalModifiedDate = rafting.ModifiedDate;
            Description = $"Simulate states for rafting '{rafting.Name}'";
        }

        public bool Execute()
        {
            try
            {
                ExecutedAt = DateTime.Now;

                if (_isSimulationRunning)
                {
                    return false; // Already running
                }

                // Create state sequence starting from current state
                _stateSequence = CreateStateSequence(_rafting.CurrentState);
                _currentStateIndex = 0;
                _isSimulationRunning = true;

                // Initialize timer for state transitions
                _simulationTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2) // Change state every 2 seconds
                };
                _simulationTimer.Tick += SimulationTimer_Tick;
                _simulationTimer.Start();

                _statusUpdateCallback?.Invoke($"Starting simulation for '{_rafting.Name}' - {_stateSequence.Count} state transitions");
                return true;
            }
            catch (Exception)
            {
                _isSimulationRunning = false;
                return false;
            }
        }

        public bool Undo()
        {
            try
            {
                // Stop simulation if running
                StopSimulation();

                // Restore original state
                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.ChangeState(_rafting.Id, _originalState),
                    "Restore rafting state");

                if (success)
                {
                    _rafting.CurrentState = _originalState;
                    _rafting.ModifiedDate = _originalModifiedDate;
                    _statusUpdateCallback?.Invoke($"Simulation undone for '{_rafting.Name}' - restored to {_originalState}");
                    return true;
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }
            return false;
        }

        private List<RaftingState> CreateStateSequence(RaftingState startState)
        {
            var sequence = new List<RaftingState>();

            // Find starting position in progression
            int startIndex = Array.IndexOf(StateProgression, startState);
            if (startIndex == -1)
            {
                // If current state is not in progression, start from beginning
                startIndex = 0;
            }

            // Add states from current position to end
            for (int i = startIndex + 1; i < StateProgression.Length; i++)
            {
                sequence.Add(StateProgression[i]);
            }

            // If we didn't reach Finished, ensure we end there
            if (sequence.LastOrDefault() != RaftingState.Finished)
            {
                sequence.Add(RaftingState.Finished);
            }

            return sequence;
        }

        private async void SimulationTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_currentStateIndex >= _stateSequence.Count)
                {
                    // Simulation complete
                    StopSimulation();
                    _statusUpdateCallback?.Invoke($"Simulation completed for '{_rafting.Name}' - final state: {_rafting.CurrentState}");
                    return;
                }

                var nextState = _stateSequence[_currentStateIndex];

                // Update state on server
                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.ChangeState(_rafting.Id, nextState),
                    "Change rafting state");

                if (success)
                {
                    _rafting.CurrentState = nextState;
                    _rafting.ModifiedDate = DateTime.Now;

                    var remainingSteps = _stateSequence.Count - _currentStateIndex - 1;
                    _statusUpdateCallback?.Invoke($"Simulating '{_rafting.Name}': {nextState} ({remainingSteps} steps remaining)");

                    _currentStateIndex++;
                }
                else
                {
                    // Stop simulation on failure
                    StopSimulation();
                    _statusUpdateCallback?.Invoke($"Simulation failed for '{_rafting.Name}' at state: {nextState}");
                }
            }
            catch (Exception)
            {
                StopSimulation();
                _statusUpdateCallback?.Invoke($"Simulation error for '{_rafting.Name}'");
            }
        }

        private void StopSimulation()
        {
            _simulationTimer?.Stop();
            _simulationTimer = null;
            _isSimulationRunning = false;
        }

        public void Dispose()
        {
            StopSimulation();
        }
    }
}