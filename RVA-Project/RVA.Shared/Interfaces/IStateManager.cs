using RVA.Shared.Enums;
using RVA.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// State Pattern - Interfejs za upravljanje stanjima Rafting entiteta
    public interface IStateManager
    {
        // State transitions
        bool CanTransitionTo(RaftingState currentState, RaftingState newState);
        void TransitionTo(Rafting rafting, RaftingState newState);

        // Valid transitions
        IEnumerable<RaftingState> GetValidTransitions(RaftingState currentState);

        // State info
        string GetStateDescription(RaftingState state);
        int GetStateOrder(RaftingState state);

        // Events
        event EventHandler<StateChangedEventArgs> StateChanged;
    }

    // Event argumenti za state promene
    public class StateChangedEventArgs : EventArgs
    {
        public int RaftingId { get; set; }
        public RaftingState OldState { get; set; }
        public RaftingState NewState { get; set; }
        public DateTime ChangedAt { get; set; }

        public StateChangedEventArgs(int raftingId, RaftingState oldState, RaftingState newState)
        {
            RaftingId = raftingId;
            OldState = oldState;
            NewState = newState;
            ChangedAt = DateTime.Now;
        }
    }
}
