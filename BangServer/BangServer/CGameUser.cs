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
			Program.game_main.enqueue_packet(msg, this);		// 롤백 시 주석 해제
		}

		void IPeer.on_removed()
		{
			Console.WriteLine("The client disconnected.");

			Program.remove_user(this);						// 롤백 시 주석 해제
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
                    Program.game_main.matching_req(this);
					break;

				case PROTOCOL.LOADING_COMPLETED:
					this.battle_room.loading_complete(player);
					break;

				case PROTOCOL.MOVING_REQ:
					{
						short begin_pos = msg.pop_int16();
						short target_pos = msg.pop_int16();
						this.battle_room.moving_req(this.player, begin_pos, target_pos);
					}
					break;
				case PROTOCOL.CHARACTERCHOICE:
					{
						// 캐릭터 선택 기능 활성화 시 사용할 것
					}
					break;
				case PROTOCOL.USECARD:
                    {
						// 플레이어가 카드 소비, 장착할 경우 프로토콜
						// 사용 카드는 string 형태로 구분할 것
						//battle_room.UseBang
                    }
					break;

				case PROTOCOL.TURN_FINISHED_REQ:
					this.battle_room.turn_finished(this.player);
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
