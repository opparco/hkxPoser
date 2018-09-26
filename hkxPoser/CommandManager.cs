using System.Collections.Generic;

namespace hkxPoser
{
    public class CommandManager
    {
        /// 操作リスト
        public List<ICommand> commands = new List<ICommand>();
        int command_id = 0;

        /// 指定操作を実行します。
        public void Execute(ICommand command)
        {
            if (command.Execute())
            {
                if (command_id == commands.Count)
                    commands.Add(command);
                else
                    commands[command_id] = command;
                command_id++;
            }
        }

        /// 操作を消去します。
        public void ClearCommands()
        {
            commands.Clear();
            command_id = 0;
        }

        /// ひとつ前の操作による変更を元に戻せるか。
        public bool CanUndo()
        {
            return (command_id > 0);
        }

        /// ひとつ前の操作による変更を元に戻します。
        public void Undo()
        {
            if (!CanUndo())
                return;

            command_id--;
            Undo(commands[command_id]);
        }

        /// 指定操作による変更を元に戻します。
        public void Undo(ICommand command)
        {
            command.Undo();
        }

        /// ひとつ前の操作による変更をやり直せるか。
        public bool CanRedo()
        {
            return (command_id < commands.Count);
        }

        /// ひとつ前の操作による変更をやり直します。
        public void Redo()
        {
            if (!CanRedo())
                return;

            Redo(commands[command_id]);
            command_id++;
        }

        /// 指定操作による変更をやり直します。
        public void Redo(ICommand command)
        {
            command.Redo();
        }
    }
}
