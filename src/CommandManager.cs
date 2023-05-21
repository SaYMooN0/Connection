using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Windows;

namespace Connection.src
{
    static class CommandManager
    {
        public delegate void EventStringContainer(string msg);
        static public event EventStringContainer AddToMsgEvent;
        static public event EventStringContainer AddToCmndsEvent;
        static public event EventStringContainer AddToUsersList;
        static public event EventStringContainer ConnectionRequest;
        static public event EventStringContainer GotConnectionApproved;

        static public void ProcessTheCommand(string cmdStr)
        {
            Command cmd = ParseTheCommand(cmdStr);
            if (cmd.sender.ToString() == NetworkManager.myIP.ToString())
                AddToCmndsEvent?.Invoke($"Message from myself");
            else
            {
                switch (cmd.commandType)
                {
                    case CommandType.NotCommand:
                        {
                            AddToMsgEvent?.Invoke(cmd.sender.ToString() + ": <<" + cmd.otherInfo + ">>\n");
                            break;
                        }
                    case CommandType.ToEveryOneLoading:
                        {
                            if (NetworkManager.receivingOffers)
                            {
                                NetworkManager networkManager = NetworkManager.Instance;
                                Command cmdToSend = new Command(cmd.sender, NetworkManager.myIP, CommandType.ResponseToOnLoaded);
                                networkManager.SendCommandByUPD(cmdToSend);
                                AddToUsersList?.Invoke(cmd.sender.ToString());
                            }
                            break;
                        }
                    case CommandType.ResponseToOnLoaded:
                        {
                            AddToCmndsEvent?.Invoke($"response got by:<< " + cmd.sender.ToString() + " >>");
                            AddToUsersList?.Invoke(cmd.sender.ToString());
                            break;
                        }
                    case CommandType.ConectionRequest:
                        {
                            ConnectionRequest?.Invoke(cmd.sender.ToString());
                            AddToCmndsEvent?.Invoke($"Connection requested by<< " + cmd.sender.ToString() + " >>");
                            break;
                        }
                    case CommandType.RequestRejected:
                        {
                            AddToCmndsEvent?.Invoke($"Your request was rejected by << " + cmd.sender.ToString() + " >>");
                            break;
                        }
                    case CommandType.RequestApproved:
                        {
                            GotConnectionApproved?.Invoke(cmd.sender.ToString());
                            break;
                        }
                    default: { break; }

                }
            }
        }
        static private Command ParseTheCommand(string cmdStr)
        {
            Command cmd = null;
            string[] args;
            if (cmdStr.StartsWith(Command.commandFlag))
            {

                args = cmdStr.Split(Command.divider);
                Enum.TryParse(args[1], out CommandType type);
                IPAddress reciever = IPAddress.Parse(args[2]);
                IPAddress sender = IPAddress.Parse(args[3]);
                cmd = new Command(reciever, sender, type, args[4]);
                return cmd;
            }

            return null;
        }

    }
}
