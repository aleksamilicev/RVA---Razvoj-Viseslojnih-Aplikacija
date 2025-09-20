using RVA.Client.Services;
using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using System;
using System.Collections.ObjectModel;

namespace RVA.Client.Commands
{
    public class AddRaftingCommand : IUndoableCommand
    {
        private readonly WcfServiceClient _serviceClient;
        private readonly ObservableCollection<RaftingDto> _collection;
        private readonly RaftingDto _rafting;
        private int _assignedId;

        public string Description { get; }
        public DateTime ExecutedAt { get; private set; }

        public AddRaftingCommand(WcfServiceClient serviceClient, ObservableCollection<RaftingDto> collection, RaftingDto rafting)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _rafting = rafting ?? throw new ArgumentNullException(nameof(rafting));
            Description = $"Add rafting '{rafting.Name}'";
        }

        public bool Execute()
        {
            try
            {
                ExecutedAt = DateTime.Now;

                var newId = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.Create(_rafting),
                    "Create rafting");

                if (newId > 0)
                {
                    _assignedId = newId;
                    _rafting.Id = newId;
                    _collection.Add(_rafting);
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
                if (_assignedId > 0)
                {
                    var success = _serviceClient.Execute(() =>
                        _serviceClient.RaftingService.Delete(_assignedId),
                        "Delete rafting");

                    if (success)
                    {
                        _collection.Remove(_rafting);
                        return true;
                    }
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