using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RVA.Client.Commands
{
    /// <summary>
    /// Manages the execution, undo, and redo of commands
    /// </summary>
    public class CommandManager : INotifyPropertyChanged
    {
        #region Private Fields
        private readonly Stack<IUndoableCommand> _undoStack;
        private readonly Stack<IUndoableCommand> _redoStack;
        private readonly int _maxHistorySize;
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Constructor
        public CommandManager(int maxHistorySize = 50)
        {
            _maxHistorySize = maxHistorySize;
            _undoStack = new Stack<IUndoableCommand>();
            _redoStack = new Stack<IUndoableCommand>();
        }
        #endregion

        #region Properties
        public bool CanUndo => _undoStack.Count > 0;

        public bool CanRedo => _redoStack.Count > 0;

        public string NextUndoDescription
        {
            get
            {
                if (!CanUndo) return string.Empty;
                return $"Undo: {_undoStack.Peek().Description}";
            }
        }

        public string NextRedoDescription
        {
            get
            {
                if (!CanRedo) return string.Empty;
                return $"Redo: {_redoStack.Peek().Description}";
            }
        }

        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes a command and adds it to the undo stack
        /// </summary>
        public bool ExecuteCommand(IUndoableCommand command)
        {
            if (command == null) return false;

            try
            {
                if (command.Execute())
                {
                    // Clear redo stack when new command is executed
                    _redoStack.Clear();

                    // Add to undo stack
                    _undoStack.Push(command);

                    // Limit history size
                    LimitHistorySize();

                    OnPropertyChanged();
                    return true;
                }
            }
            catch (Exception)
            {
                // Log exception if needed
                return false;
            }

            return false;
        }

        /// <summary>
        /// Undoes the last command
        /// </summary>
        public bool Undo()
        {
            if (!CanUndo) return false;

            try
            {
                var command = _undoStack.Pop();
                if (command.Undo())
                {
                    _redoStack.Push(command);
                    OnPropertyChanged();
                    return true;
                }
                else
                {
                    // If undo failed, put it back
                    _undoStack.Push(command);
                }
            }
            catch (Exception)
            {
                // Log exception if needed
                return false;
            }

            return false;
        }

        /// <summary>
        /// Redoes the last undone command
        /// </summary>
        public bool Redo()
        {
            if (!CanRedo) return false;

            try
            {
                var command = _redoStack.Pop();
                if (command.Execute())
                {
                    _undoStack.Push(command);
                    OnPropertyChanged();
                    return true;
                }
                else
                {
                    // If redo failed, put it back
                    _redoStack.Push(command);
                }
            }
            catch (Exception)
            {
                // Log exception if needed
                return false;
            }

            return false;
        }

        /// <summary>
        /// Clears all command history
        /// </summary>
        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnPropertyChanged();
        }

        /// <summary>
        /// Gets the undo history as a list of descriptions
        /// </summary>
        public List<string> GetUndoHistory()
        {
            var history = new List<string>();
            foreach (var command in _undoStack)
            {
                history.Add($"{command.Description} ({command.ExecutedAt:HH:mm:ss})");
            }
            return history;
        }

        /// <summary>
        /// Gets the redo history as a list of descriptions
        /// </summary>
        public List<string> GetRedoHistory()
        {
            var history = new List<string>();
            foreach (var command in _redoStack)
            {
                history.Add($"{command.Description} ({command.ExecutedAt:HH:mm:ss})");
            }
            return history;
        }
        #endregion

        #region Private Methods
        private void LimitHistorySize()
        {
            while (_undoStack.Count > _maxHistorySize)
            {
                // Remove the oldest command
                var temp = new Stack<IUndoableCommand>();
                for (int i = 0; i < _undoStack.Count - 1; i++)
                {
                    temp.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (temp.Count > 0)
                {
                    _undoStack.Push(temp.Pop());
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Notify all related properties
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanUndo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanRedo)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextUndoDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextRedoDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UndoCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RedoCount)));
        }
        #endregion
    }
}