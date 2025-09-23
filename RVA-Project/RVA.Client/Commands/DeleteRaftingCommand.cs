using RVA.Client.Services;
using RVA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Client.Commands
{
    public class DeleteRaftingCommand : IUndoableCommand
    {
        private readonly WcfServiceClient _serviceClient;
        private readonly ObservableCollection<RaftingDto> _collection;
        private readonly RaftingDto _rafting;
        private int _originalIndex;

        public string Description { get; }
        public DateTime ExecutedAt { get; private set; }

        public DeleteRaftingCommand(WcfServiceClient serviceClient, ObservableCollection<RaftingDto> collection, RaftingDto rafting)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _rafting = rafting ?? throw new ArgumentNullException(nameof(rafting));
            Description = $"Delete rafting '{rafting.Name}'";
        }

        public bool Execute()
        {
            try
            {
                ClientLogger.Info($"Executing delete rafting command: {Description}");
                ExecutedAt = DateTime.Now;
                _originalIndex = _collection.IndexOf(_rafting);

                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.Delete(_rafting.Id),
                    "Delete rafting");

                if (success)
                {
                    ClientLogger.Info($"Rafting deleted successfully");

                    _collection.Remove(_rafting);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Error($"Error executing delete rafting command: {ex.Message}", ex);
            }
            return false;
        }

        public bool Undo()
        {
            try
            {
                var newId = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.Create(_rafting),
                    "Create rafting");

                if (newId > 0)
                {
                    _rafting.Id = newId;

                    // Insert at original position if possible
                    if (_originalIndex >= 0 && _originalIndex < _collection.Count)
                    {
                        _collection.Insert(_originalIndex, _rafting);
                    }
                    else
                    {
                        _collection.Add(_rafting);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Error($"Error executing undo rafting command: {ex.Message}", ex);
            }
            return false;
        }
    }
}
