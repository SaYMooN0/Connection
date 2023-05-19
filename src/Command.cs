using System.Net;

namespace Connection.src
{
    class Command
    {
        public readonly IPAddress reciever;
        public readonly IPAddress sender;
        public readonly CommandType commandType;
        public readonly string otherInfo;
        const char divider = '\n';
        const string commandFlag = "\tCommand\t";
        public Command(IPAddress reciever, IPAddress sender, CommandType type, string otherInfo)
        {
            this.reciever = reciever;
            this.sender = sender;
            this.commandType = type;
            this.otherInfo = otherInfo;
        }
        public Command(IPAddress reciever, IPAddress sender, CommandType type)
        {
            this.reciever = reciever;
            this.sender = sender;
            this.commandType = type;
            this.otherInfo = "";
        }
        public string getStringToSend()
        {
            string str = commandFlag;
            str += divider;
            str += commandType.ToString() + divider;
            str += reciever.ToString() + divider + sender.ToString() + divider;
            str+= otherInfo;
            return str;
        }
        public override string ToString()
        {
            return $"Command\nType: <<{commandType}>>\nSender <<{sender}>>\nReciever <<{reciever}>>\nOther info \"{otherInfo}\"";
        }
    }
    enum CommandType
    {
        NotCommand,
        ToEveryOneLoading,

    }
}
