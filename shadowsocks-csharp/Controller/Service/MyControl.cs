using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.CompilerServices;
using Shadowsocks.Controller.Strategy;
using Shadowsocks.Encryption;
using Shadowsocks.Model;


enum CMD_TYPE
{
    GET_SERVER,
    UP_INDEX,
    DOWN_INDEX,
    SET_INDEX,
};
namespace Shadowsocks.Controller
{
    public class MyControl : Listener.Service
    {
        private ShadowsocksController _controller;

        public MyControl(ShadowsocksController controller)
        {
            this._controller = controller;
        }

        public override bool Handle(byte[] firstPacket, int length, Socket socket, object state)
        {
            switch ((CMD_TYPE)firstPacket[0]){
                case CMD_TYPE.GET_SERVER:
                    sendServerInfo();
                    break;
                case CMD_TYPE.UP_INDEX:
                    moveIndex(CMD_TYPE.UP_INDEX);
                    break;
                case CMD_TYPE.DOWN_INDEX:
                    moveIndex(CMD_TYPE.DOWN_INDEX);
                    break;
                case CMD_TYPE.SET_INDEX:
                    setIndex(firstPacket[1]);
                    break;
            }
            return true;
        }

        private void setIndex(int index)
        {
            Configuration config = _controller.GetCurrentConfiguration();

            if (index <= 0)
            {
                index = 0;
            }
            else if (index >= config.configs.Count)
            {
                index = config.configs.Count - 1;
            }

            _controller.SelectServerIndex(index);
            sendMsg("" + index);
        }

        private void moveIndex(CMD_TYPE opr)
        {
            Configuration config = _controller.GetCurrentConfiguration();
            int curIndex = config.index;

            if (opr == CMD_TYPE.UP_INDEX)
            {
                curIndex -= 1;
            }
            else if (opr == CMD_TYPE.DOWN_INDEX)
            {
                curIndex += 1;
            }

            if (curIndex <= 0)
            {
                curIndex = 0;
            }
            else if (curIndex >= config.configs.Count)
            {
                curIndex = config.configs.Count - 1;
            }

            _controller.SelectServerIndex(curIndex);
            sendMsg("" + curIndex);
        }

        private void sendServerInfo ()
        {
            Server server = _controller.GetCurrentServer();

            string msg = "";
            msg += server.server + "&"
                + server.server_port + "&"
                + server.password + "&"
                + server.method + "&"
                + server.remarks;

            sendMsg(msg);
        }

        private void sendMsg(string msg)
        {

            EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9527);
            Socket remote = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            remote.SendTo(Encoding.UTF8.GetBytes(msg), msg.Length, SocketFlags.None, point);

        }
    }
}
