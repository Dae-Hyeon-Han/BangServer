using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeNet;

namespace BangServer
{
    public class GameUser : IPeer
    {
        CUserToken token;

        public GameRoom battleRoom { get; private set; }

        Player player;

        public GameUser(CUserToken token)
        {
            this.token = token;
            this.token.set_peer(this);
        }

        void IPeer.on_message(Const<byte[]> buffer)
        {
            // ex)
            byte[] clone = new byte[1024];
            Array.Copy(buffer.Value, clone, buffer.Value.Length);
            CPacket msg = new CPacket(clone, this);
            Program.game_main.enqueue_packet(msg, this);
        }

        void IPeer.on_removed()
        {
            Console.WriteLine("클라이언트 연결 종료");
            Program.remove_user(this);
        }

        public void send(CPacket msg)
        {
            this.token.send(msg);
        }

        void IPeer.disconnect()
        {
            this.token.socket.Disconnect(false);
        }

        // 유저 측에서 서버에 요청한 메시지
        void IPeer.process_user_operation(CPacket msg)
        {
            BangProtocol protocol = (BangProtocol)msg.pop_protocol_id();
            Console.WriteLine("요청한 콘솔 아이디: " + protocol);

            switch(protocol)
            {
                // 입장 요청
                case BangProtocol.ENTER_GAME_ROOM_REQ:
                    Program.game_main.matching_req(this);
                    break;

                // 로딩(매칭) 완료
                case BangProtocol.LOADING_COMPLETED:
                    
                    break;

                // 플레이어의 메인 턴 이전의 행위(감옥, 다이너마이트 등) 요청
                case BangProtocol.PLAYER_FIRST_ACT:
                    {
                        //this.battleRoom.
                    }
                    break;

                // 플레이어의 메인 턴 행위 요청
                case BangProtocol.PLAYER_NORMAL_ACT:
                    break;

                // 플레이어 턴 완료
                case BangProtocol.TURN_FINISHED_REQ:
                    break;

                // 플레이어가 메시지를 보냄
                case BangProtocol.PLAYER_CHAT_SEND:
                    {
                        //Console.WriteLine("채팅 침");
                        string text = msg.pop_string();
                        Console.WriteLine(text);
                        CPacket message = CPacket.create((short)BangProtocol.PLAYER_CHAT_RECV);
                        message.push(text);
                        //battleRoom.BroadCast(msg);
                        battleRoom.BroadCast(message);
                    }
                    break;

                // 플레이어가 메시지를 받음
                case BangProtocol.PLAYER_CHAT_RECV:
                    break;
            }
        }

        public void EnterRoom(Player player, GameRoom room)
        {
            this.player = player;
            this.battleRoom = room;
        }
    }
}
