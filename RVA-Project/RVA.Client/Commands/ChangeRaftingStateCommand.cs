using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Client.Commands
{
    public class ChangeRaftingStateCommand : IUndoableCommand
    {
        private readonly WcfServiceClient _serviceClient;
        private readonly RaftingDto _rafting;
        private readonly RaftingState _newState;
        private readonly RaftingState _originalState;
        private readonly DateTime _originalModifiedDate;

        public string Description { get; }
        public DateTime ExecutedAt { get; private set; }

        public ChangeRaftingStateCommand(WcfServiceClient serviceClient, RaftingDto rafting, RaftingState newState)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _rafting = rafting ?? throw new ArgumentNullException(nameof(rafting));
            _newState = newState;
            _originalState = rafting.CurrentState;
            _originalModifiedDate = rafting.ModifiedDate;
            Description = $"Change '{rafting.Name}' state from {_originalState} to {newState}";
        }

        public bool Execute()
        {
            try
            {
                ExecutedAt = DateTime.Now;

                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.ChangeState(_rafting.Id, _newState),
                    "Change rafting state");

                if (success)
                {
                    _rafting.CurrentState = _newState;
                    _rafting.ModifiedDate = DateTime.Now;
                    return true;
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }
            return false;
        }

        public bool Undo()
        {
            try
            {
                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.ChangeState(_rafting.Id, _originalState),
                    "Change rafting state");

                if (success)
                {
                    _rafting.CurrentState = _originalState;
                    _rafting.ModifiedDate = _originalModifiedDate;
                    return true;
                }
            }
            catch (Exception)
            {
                // Log exception if needed
            }
            return false;
        }
    }
}
