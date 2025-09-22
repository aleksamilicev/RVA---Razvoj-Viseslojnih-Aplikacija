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
    public class UpdateRaftingCommand : IUndoableCommand
    {
        private readonly WcfServiceClient _serviceClient;
        private readonly ObservableCollection<RaftingDto> _collection;
        private readonly RaftingDto _originalRafting;
        private readonly RaftingDto _updatedRafting;

        public string Description { get; }
        public DateTime ExecutedAt { get; private set; }

        public UpdateRaftingCommand(WcfServiceClient serviceClient, ObservableCollection<RaftingDto> collection,
            RaftingDto originalRafting, RaftingDto updatedRafting)
        {
            _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _originalRafting = originalRafting ?? throw new ArgumentNullException(nameof(originalRafting));
            _updatedRafting = updatedRafting ?? throw new ArgumentNullException(nameof(updatedRafting));
            Description = $"Update rafting '{updatedRafting.Name}'";
        }

        public bool Execute()
        {   
            try
            {
                ExecutedAt = DateTime.Now;

                var success = _serviceClient.Execute(() =>
                    _serviceClient.RaftingService.Update(_updatedRafting),
                    "Update rafting");

                if (success)
                {
                    // Update the rafting in collection
                    var index = _collection.IndexOf(_originalRafting);
                    if (index >= 0)
                    {
                        _collection[index] = _updatedRafting;
                    }
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
                    _serviceClient.RaftingService.Update(_originalRafting),
                    "Update rafting");

                if (success)
                {
                    // Restore original rafting in collection
                    var index = _collection.IndexOf(_updatedRafting);
                    if (index >= 0)
                    {
                        _collection[index] = _originalRafting;
                    }
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
