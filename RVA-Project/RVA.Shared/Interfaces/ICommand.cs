using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    /// Osnovni Command interfejs - samo za izvršavanje komande
    public interface ICommand
    {
        void Execute();
        bool CanExecute();
        string Name { get; }
        DateTime ExecutedAt { get; }
    }

    /// Undoable command - dodaje Undo funkcionalnost
    public interface IUndoableCommand : ICommand
    {
        void Undo();
        bool CanUndo();
    }

    /// Command Manager za upravljanje istorijom komandi
    public interface ICommandManager
    {
        void ExecuteCommand(ICommand command);  // Može raditi i sa Undoable i sa običnim komandama
        void Undo();
        void Redo();

        bool CanUndo { get; }
        bool CanRedo { get; }

        void ClearHistory();
        int HistoryCount { get; }

        event EventHandler<CommandExecutedEventArgs> CommandExecuted;
        event EventHandler<CommandUndoneEventArgs> CommandUndone;
    }

    // Event argumenti za command events
    public class CommandExecutedEventArgs : EventArgs
    {
        public ICommand Command { get; set; }
        public CommandExecutedEventArgs(ICommand command) => Command = command;
    }

    public class CommandUndoneEventArgs : EventArgs
    {
        public IUndoableCommand Command { get; set; }
        public CommandUndoneEventArgs(IUndoableCommand command) => Command = command;
    }
}
