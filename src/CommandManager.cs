using System;
using System.Net;
using System.Windows;

namespace Connection.src
{
    static class CommandManager
    {
        public delegate void EventStringContainer(string msg);
        static public event EventStringContainer AddToMsgEvent;
        static public event EventStringContainer AddToCmndsEvent;
        static public void ProcessTheCommand(string cmdStr)
        {
            AddToCmndsEvent?.Invoke($"-=-=-=-=-=--=--=-=-=-=\nReceived message:{cmdStr}\n");
            Command cmd= ParseTheCommand(cmdStr);
            if (cmd.sender.ToString() == NetworkManager.myIP.ToString())
            {
                AddToCmndsEvent?.Invoke($"Message from myself");
            }
            else
            {
                AddToCmndsEvent?.Invoke(cmd.ToString());
            }


        }
        static private Command ParseTheCommand(string cmdStr)
        {
            Command cmd = null ;
            string[] args;
            if (cmdStr.StartsWith(Command.commandFlag))
            {
               
                args = cmdStr.Split(Command.divider);
                Enum.TryParse(args[1], out CommandType type);
                IPAddress reciever = IPAddress.Parse(args[2]);
                IPAddress sender = IPAddress.Parse(args[3]);
                cmd = new Command(reciever, sender,type, args[4]);
                return cmd;
            }
            
            return null;
        }
        
    }
}
