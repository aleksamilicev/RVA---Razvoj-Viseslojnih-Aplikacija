using System;

namespace RVA.Client.Commands
{
    public interface IUndoableCommand
    {
        bool Execute();

        bool Undo();

        string Description { get; }

        DateTime ExecutedAt { get; }
    }
}