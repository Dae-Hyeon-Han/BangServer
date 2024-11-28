using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeNet;

namespace BangServer
{
    /// <summary>
    /// 하나의 session객체를 나타낸다.
    /// </summary>
    public class CGameUser : IPeer
    {
        CUserToken token;

        public CGameRoom battle_room { get; private set; }
        public string playerId { get; set; }                    // 플레이어 아이디를 저장하는 변수

        CPlayer player;

        public CGameUser(CUserToken token)
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
            Program.game_main.enqueue_packet(msg, this);        // 롤백 시 주석 해제
        }

        void IPeer.on_removed()
        {
            Console.WriteLine("The client disconnected.");

            Program.remove_user(this);                      // 롤백 시 주석 해제
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
            PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
            Console.WriteLine("protocol id " + protocol);
            switch (protocol)
            {
                case PROTOCOL.ENTER_GAME_ROOM_REQ:
                    //player.playerId = msg.pop_string();             // 플레이어가 왜 null이지?
                    //Console.WriteLine("플레이어 아이디: " + player.playerId);
                    #region 아이디 인풋 기능 활성화시 사용
                    //playerId = msg.pop_string();
                    #endregion
                    Program.game_main.matching_req(this);
                    break;

                case PROTOCOL.LOADING_COMPLETED:
                    this.battle_room.loading_complete(player);
                    break;

                case PROTOCOL.MOVING_REQ:
                    {
                        short begin_pos = msg.pop_int16();
                        short target_pos = msg.pop_int16();
                    }
                    break;
                case PROTOCOL.CHARACTERCHOICE:
                    {
                        // 캐릭터 선택 기능 활성화 시 사용할 것
                    }
                    break;
                case PROTOCOL.DRAWCARD:
                    {
                        battle_room.DrawCard(msg.pop_byte(), msg.pop_int32());
                    }
                    break;
                case PROTOCOL.DROPCARD:         // 카드 사용(USECARD) 후 쓴 카드 덱에 넣는 용도
                    {
                        battle_room.DropCard(msg.pop_byte(), msg.pop_string(), msg.pop_string(), msg.pop_string());
                    }
                    break;
                case PROTOCOL.USECARD:
                    {
                        // 첫번째 pop_string 값은 무조건 사용한 카드 이름일 것
                        string cardName = msg.pop_string();
                        //byte targetIndex = msg.pop_byte();

                        // 뱅
                        if(cardName == "BANG") 
                        {
                            battle_room.UseBang(msg.pop_byte());
                        }
                        // 빗나감. 안 쓸 듯?
                        else if (cardName == "MANCATO") {}
                        // 맥주
                        else if (cardName == "BIRRA") 
                        {
                            battle_room.UseBirra();
                        }
                        // 기관총
                        else if (cardName == "GATLING")
                        {
                            battle_room.UseGatling();
                        }
                        // 결투
                        else if (cardName == "DUELLO") 
                        {
                            battle_room.UseDuello(msg.pop_byte());
                        }
                        // 인디언
                        else if (cardName == "INDIANI")
                        {
                            battle_room.UseIndiani();
                        }
                        // 주점
                        else if (cardName == "SALOON") 
                        {
                            battle_room.UseSaloon();
                        }
                        // 강탈
                        else if (cardName == "PANICO") { }
                        // 캣 벌로우
                        else if (cardName == "CAT BALOU") 
                        {
                            battle_room.UseCatBalou();
                        }
                        // 잡화점
                        else if (cardName == "EMPORIO") 
                        {
                            battle_room.UseEmporio();
                        }
                        // 역마차
                        else if (cardName == "DILIGENZA") 
                        {
                            battle_room.UseGetCards(2);
                        }
                        // 웰스파고 은행
                        else if (cardName == "WELLS FARGO") 
                        {
                            battle_room.UseGetCards(3);
                        }
                        // 스코필드, 레밍턴, 카빈, 윈체스터, 볼캐닉, 조준경, 야생마, 술통
                        else if (cardName == "SCHOFIELD" || cardName == "REMINGTON" || 
                                 cardName == "CARABINE" || cardName == "WINCHESTER" || 
                                 cardName == "VOLCANIC" || cardName == "MIRONO" || 
                                 cardName == "MUSTANG" || cardName == "BARILE")
                        {
                            battle_room.EquipGun(msg.pop_byte(), cardName);
                        }
                        // 감옥
                        else if (cardName == "PRIGIONE") { }
                        // 다이너마이트
                        else if (cardName == "DINAMITE") { }
                    }
                    break;
                case PROTOCOL.REACTION:
                    {
                        string tag = msg.pop_string();
                        if(tag == "MINCATO")
                        {

                        }
                        if (tag == "BANG")
                        {

                        }
                    }
                    break;
                case PROTOCOL.REQUESTFAIL:
                    {
                        battle_room.RequestFail(msg.pop_byte());
                    }
                    break;
                case PROTOCOL.CHAT:
                    {
                        Console.WriteLine("chat 받음");
                        this.battle_room.Chat(msg);
                    }
                    break;

                case PROTOCOL.TURN_FINISHED_REQ:
                    //this.battle_room.turn_finished(this.player);
                    byte index = msg.pop_byte();
                    this.battle_room.TurnEnd(index);
                    break;
            }
        }

        

        public void enter_room(CPlayer player, CGameRoom room)
        {
            this.player = player;
            this.battle_room = room;
        }
    }
}
