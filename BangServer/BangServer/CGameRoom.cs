using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
	using FreeNet;

	/// <summary>
	/// 게임의 로직이 처리되는 핵심 클래스이다.
	/// </summary>
	public class CGameRoom
	{
		enum PLAYER_STATE : byte
		{
			// 방에 막 입장한 상태.
			ENTERED_ROOM,

			// 로딩을 완료한 상태.
			LOADING_COMPLETE,

			// 턴 진행 준비 상태.
			READY_TO_TURN,

			// 턴 연출을 모두 완료한 상태.
			CLIENT_TURN_FINISHED
		}

		// 게임을 진행하는 플레이어. 1P, 2P가 존재한다.
		List<CPlayer> players;
		List<CPlayer> livingPlayers;

		// 플레잉 카드
		//List<CCard> deck;

		// 캐릭터 카드
		List<string> Characters;

		// 플레이어들의 상태를 관리하는 변수.
		Dictionary<byte, PLAYER_STATE> player_state;

		// 현재 턴을 진행하고 있는 플레이어의 인덱스.
		byte current_turn_player;

		// 게임 보드판.
		List<short> gameboard;

		// 0~49까지의 인덱스를 갖고 있는 보드판 데이터.
		List<short> table_board;

		static byte COLUMN_COUNT = 7;

		readonly short EMPTY_SLOT = short.MaxValue;

        #region 뱅 용
        List<CCard> deck = new List<CCard>();

        #endregion

        public CGameRoom()
		{
			this.players = new List<CPlayer>();
			this.livingPlayers = players;
			this.player_state = new Dictionary<byte, PLAYER_STATE>();
			this.current_turn_player = 0;

			// 7*7(총 49칸)모양의 보드판을 구성한다.
			// 초기에는 모두 빈공간이므로 EMPTY_SLOT으로 채운다.
			this.gameboard = new List<short>();
			this.table_board = new List<short>();
			for (byte i = 0; i < COLUMN_COUNT * COLUMN_COUNT; ++i)
			{
				this.gameboard.Add(EMPTY_SLOT);
				this.table_board.Add(i);
			}
		}


		/// <summary>
		/// 모든 유저들에게 메시지를 전송한다.
		/// </summary>
		/// <param name="msg"></param>
		void broadcast(CPacket msg)
		{
			this.players.ForEach(player => player.send_for_broadcast(msg));
			CPacket.destroy(msg);
		}


		/// <summary>
		/// 플레이어의 상태를 변경한다.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="state"></param>
		void change_playerstate(CPlayer player, PLAYER_STATE state)
		{
			if (this.player_state.ContainsKey(player.player_index))
			{
				this.player_state[player.player_index] = state;
			}
			else
			{
				this.player_state.Add(player.player_index, state);
			}
		}


		/// <summary>
		/// 모든 플레이어가 특정 상태가 되었는지를 판단한다.
		/// 모든 플레이어가 같은 상태에 있다면 true, 한명이라도 다른 상태에 있다면 false를 리턴한다.
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		bool allplayers_ready(PLAYER_STATE state)
		{
			foreach (KeyValuePair<byte, PLAYER_STATE> kvp in this.player_state)
			{
				if (kvp.Value != state)
				{
					return false;
				}
			}

			return true;
		}


		/// <summary>
		/// 매칭이 성사된 플레이어들이 게임에 입장한다.
		/// </summary>
		/// <param name="player1"></param>
		/// <param name="player2"></param>
		public void enter_gameroom(CGameUser user1, CGameUser user2)
		{
			// 플레이어들을 생성하고 각각 0번, 1번 인덱스를 부여해 준다.
			CPlayer player1 = new CPlayer(user1, 0);        // 1P
			CPlayer player2 = new CPlayer(user2, 1);        // 2P
			this.players.Clear();
			this.players.Add(player1);
			this.players.Add(player2);

			// 플레이어들의 초기 상태를 지정해 준다.
			this.player_state.Clear();
			change_playerstate(player1, PLAYER_STATE.ENTERED_ROOM);
			change_playerstate(player2, PLAYER_STATE.ENTERED_ROOM);

			// 로딩 시작메시지 전송.
			this.players.ForEach(player =>
			{
				CPacket msg = CPacket.create((Int16)PROTOCOL.START_LOADING);
				msg.push(player.player_index);  // 본인의 플레이어 인덱스를 알려준다.
				player.send(msg);
			});

			user1.enter_room(player1, this);
			user2.enter_room(player2, this);
		}


		/// <summary>
		/// 클라이언트에서 로딩을 완료한 후 요청함.
		/// 이 요청이 들어오면 게임을 시작해도 좋다는 뜻이다.
		/// </summary>
		/// <param name="sender">요청한 유저</param>
		public void loading_complete(CPlayer player)
		{
			// 해당 플레이어를 로딩완료 상태로 변경한다.
			change_playerstate(player, PLAYER_STATE.LOADING_COMPLETE);

			// 모든 유저가 준비 상태인지 체크한다.
			if (!allplayers_ready(PLAYER_STATE.LOADING_COMPLETE))
			{
				// 아직 준비가 안된 유저가 있다면 대기한다.
				return;
			}

			// 모두 준비 되었다면 게임을 시작한다.
			battle_start();
		}


		/// <summary>
		/// 게임을 시작한다.
		/// </summary>
		void battle_start()
		{
			Console.WriteLine("게임 시작!");

            #region 구버전
            //        // 게임을 새로 시작할 때 마다 초기화해줘야 할 것들.
            //        reset_gamedata();

            //        // 게임 시작 메시지 전송.
            //        CPacket msg = CPacket.create((short)PROTOCOL.GAME_START);
            //        // 플레이어들의 세균 위치 전송.
            //        msg.push((byte)this.players.Count);
            //        this.players.ForEach(player =>
            //        {
            //            msg.push(player.player_index);      // 누구인지 구분하기 위한 플레이어 인덱스.

            //Console.WriteLine("인덱스: " + player.player_index);

            //// 플레이어가 소지한 세균들의 전체 개수.
            //byte cell_count = (byte)player.viruses.Count;
            //            msg.push(cell_count);
            //            // 플레이어의 세균들의 위치정보.
            //            player.viruses.ForEach(position => msg.push_int16(position));
            //        });
            //        // 첫 턴을 진행할 플레이어 인덱스.
            //        msg.push(this.current_turn_player);
            //        broadcast(msg);
            #endregion

            #region 뱅 버전
            // 게임을 새로 시작할 때 마다 초기화해줘야 할 것들.
            ResetGameData();

            // 게임 시작 메시지 전송.
            CPacket msg = CPacket.create((short)PROTOCOL.GAME_START);
            msg.push((byte)this.players.Count);
            //CPacket charNameMsg = CPacket.create((short)PROTOCOL.CHARACTERCHOICE);

            // 플레이어들에게 선택창(캐릭터 픽) 전송.(코드 x)
            // 일단 랜덤으로 배치함
            for (int i = 0; i < players.Count; i++)
            {
                players[i].charName = Characters[i];
            }

            // 덱 셋팅 DeckSet
            
            deck = DeckSet();


            // 플레이어들이 선택한 캐릭터 전송
            this.players.ForEach(player =>
            {
                msg.push(player.player_index);      // 플레이어 구분을 위한 플레이어 인덱스
                msg.push(player.charName);
                //Console.WriteLine("인덱스: " + player.player_index);
            });
            // 동기화는...?

            // 첫턴을 시작할 플레이어 인덱스
            msg.push(this.current_turn_player);
            broadcast(msg);
            #endregion
        }


        /// <summary>
        /// 턴을 시작하라고 클라이언트들에게 알려 준다.
        /// </summary>
        void start_turn()
		{
			// 턴을 진행할 수 있도록 준비 상태로 만든다.
			this.players.ForEach(player => change_playerstate(player, PLAYER_STATE.READY_TO_TURN));

			CPacket msg = CPacket.create((short)PROTOCOL.START_PLAYER_TURN);
			msg.push(this.current_turn_player);
			broadcast(msg);
		}


		/// <summary>
		/// 게임 데이터를 초기화 한다.
		/// 게임을 새로 시작할 때 마다 초기화 해줘야 할 것들을 넣는다.
		/// </summary>
		void reset_gamedata()
		{
			//// 보드판 데이터 초기화.
			//for (int i = 0; i < this.gameboard.Count; ++i)
			//{
			//	this.gameboard[i] = EMPTY_SLOT;
			//}
			//// 1번 플레이어의 세균은 왼쪽위(0,0), 오른쪽위(0,6) 두군데에 배치한다.
			//put_virus(0, 0, 0);
			//put_virus(0, 0, 6);
			//// 2번 플레이어는 세균은 왼쪽아래(6,0), 오른쪽아래(6,6) 두군데에 배치한다.
			//put_virus(1, 6, 0);
			//put_virus(1, 6, 6);

			//// 턴 초기화.
			//this.current_turn_player = 0;   // 1P부터 시작.
		}


		/// <summary>
		/// 보드판에 플레이어의 세균을 배치한다.
		/// </summary>
		/// <param name="player_index"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>
		void put_virus(byte player_index, byte row, byte col)
		{
			short position = CHelper.get_position(row, col);
			put_virus(player_index, position);
		}


		/// <summary>
		/// 보드판에 플레이어의 세균을 배치한다.
		/// </summary>
		/// <param name="player_index"></param>
		/// <param name="position"></param>
		void put_virus(byte player_index, short position)
		{
			this.gameboard[position] = player_index;
			get_player(player_index).add_cell(position);
		}


		/// <summary>
		/// 배치된 세균을 삭제한다.
		/// </summary>
		/// <param name="player_index"></param>
		/// <param name="position"></param>
		void remove_virus(byte player_index, short position)
		{
			this.gameboard[position] = EMPTY_SLOT;
			get_player(player_index).remove_cell(position);
		}


		/// <summary>
		/// 플레이어 인덱스에 해당하는 플레이어를 구한다.
		/// </summary>
		/// <param name="player_index"></param>
		/// <returns></returns>
		CPlayer get_player(byte player_index)
		{
			return this.players.Find(obj => obj.player_index == player_index);
		}


		/// <summary>
		/// 상대방의 세균을 감염 시킨다.
		/// </summary>
		/// <param name="basis_cell"></param>
		/// <param name="attacker"></param>
		/// <param name="victim"></param>
		public void infect(short basis_cell, CPlayer attacker, CPlayer victim)
		{
			// 방어자의 세균중에 기준위치로 부터 1칸 반경에 있는 세균들이 감염 대상이다.
			List<short> neighbors = CHelper.find_neighbor_cells(basis_cell, victim.viruses, 1);
			foreach (short position in neighbors)
			{
				// 방어자의 세균을 삭제한다.
				remove_virus(victim.player_index, position);

				// 공격자의 세균을 추가하고,
				put_virus(attacker.player_index, position);
			}
		}


		/// <summary>
		/// 현재 턴인 플레이어의 상대 플레이어를 구한다.
		/// </summary>
		/// <returns></returns>
		CPlayer get_opponent_player()
		{
			return this.players.Find(player => player.player_index != this.current_turn_player);
		}


		/// <summary>
		/// 현재 턴을 진행중인 플레이어를 구한다.
		/// </summary>
		/// <returns></returns>
		CPlayer get_current_player()
		{
			return this.players.Find(player => player.player_index == this.current_turn_player);
		}


		/// <summary>
		/// 클라이언트의 이동 요청.
		/// </summary>
		/// <param name="sender">요청한 유저</param>
		/// <param name="begin_pos">시작 위치</param>
		/// <param name="target_pos">이동하고자 하는 위치</param>
		public void moving_req(CPlayer sender, short begin_pos, short target_pos)
		{
			// sender차례인지 체크.
			if (this.current_turn_player != sender.player_index)
			{
				// 현재 턴이 아닌 플레이어가 보낸 요청이라면 무시한다.
				// 이런 비정상적인 상황에서는 화면이나 파일로 로그를 남겨두는것이 좋다.
				return;
			}

			// begin_pos에 sender의 세균이 존재하는지 체크.
			if (this.gameboard[begin_pos] != sender.player_index)
			{
				// 시작 위치에 해당 플레이어의 세균이 존재하지 않는다.
				return;
			}

			// 목적지는 EMPTY_SLOT으로 설정된 빈 공간이어야 한다.
			// 다른 세균이 자리하고 있는 곳으로는 이동할 수 없다.
			if (this.gameboard[target_pos] != EMPTY_SLOT)
			{
				// 목적지에 다른 세균이 존재한다.
				return;
			}

			// target_pos가 이동 또는 복제 가능한 범위인지 체크.
			short distance = CHelper.get_distance(begin_pos, target_pos);
			if (distance > 2)
			{
				// 2칸을 초과하는 거리는 이동할 수 없다.
				return;
			}

			if (distance <= 0)
			{
				// 자기 자신의 위치로는 이동할 수 없다.
				return;
			}

			// 모든 체크가 정상이라면 이동을 처리한다.
			if (distance == 1)      // 이동 거리가 한칸일 경우에는 복제를 수행한다.
			{
				put_virus(sender.player_index, target_pos);
			}
			else if (distance == 2)     // 이동 거리가 두칸일 경우에는 이동을 수행한다.
			{
				// 이전 위치에 있는 세균은 삭제한다.
				remove_virus(sender.player_index, begin_pos);

				// 새로운 위치에 세균을 놓는다.
				put_virus(sender.player_index, target_pos);
			}

			// 목적지를 기준으로 주위에 존재하는 상대방 세균을 감염시켜 같은 편으로 만든다.
			CPlayer opponent = get_opponent_player();
			infect(target_pos, sender, opponent);

			// 최종 결과를 broadcast한다.
			CPacket msg = CPacket.create((short)PROTOCOL.PLAYER_MOVED);
			msg.push(sender.player_index);      // 누가
			msg.push(begin_pos);                // 어디서
			msg.push(target_pos);               // 어디로 이동 했는지
			broadcast(msg);
		}


		/// <summary>
		/// 클라이언트에서 턴 연출이 모두 완료 되었을 때 호출된다.
		/// </summary>
		/// <param name="sender"></param>
		public void turn_finished(CPlayer sender)
		{
			change_playerstate(sender, PLAYER_STATE.CLIENT_TURN_FINISHED);

			if (!allplayers_ready(PLAYER_STATE.CLIENT_TURN_FINISHED))
			{
				return;
			}

			// 턴을 넘긴다.
			turn_end();
		}


		/// <summary>
		/// 턴을 종료한다. 게임이 끝났는지 확인하는 과정을 수행한다.
		/// </summary>
		void turn_end()
		{
			// 보드판 상태를 확인하여 게임이 끝났는지 검사한다.
			if (!CHelper.can_play_more(this.table_board, get_opponent_player(), this.players))
			{
				game_over();
				return;
			}

			// 아직 게임이 끝나지 않았다면 다음 플레이어로 턴을 넘긴다.
			if (this.current_turn_player < this.players.Count - 1)
			{
				++this.current_turn_player;
			}
			else
			{
				// 다시 첫번째 플레이어의 턴으로 만들어 준다.
				this.current_turn_player = this.players[0].player_index;
			}

			// 턴을 시작한다.
			start_turn();
		}

		// 승패를 가리는 부분
		void game_over()
		{
			// 우승자 가리기.
			byte win_player_index = byte.MaxValue;
			int count_1p = this.players[0].get_virus_count();
			int count_2p = this.players[1].get_virus_count();

			if (count_1p == count_2p)
			{
				// 동점인 경우.
				win_player_index = byte.MaxValue;
			}
			else
			{
				if (count_1p > count_2p)
				{
					win_player_index = this.players[0].player_index;
				}
				else
				{
					win_player_index = this.players[1].player_index;
				}
			}


			CPacket msg = CPacket.create((short)PROTOCOL.GAME_OVER);
			msg.push(win_player_index);
			msg.push(count_1p);
			msg.push(count_2p);
			broadcast(msg);

			//방 제거.
			//Program.game_main.room_manager.remove_room(this);
		}


		public void destroy()
		{
			CPacket msg = CPacket.create((short)PROTOCOL.ROOM_REMOVED);
			broadcast(msg);

			this.players.Clear();
		}

        #region 뱅 용 메서드
		// 게임 시작시 덱 초기화
        public List<CCard> DeckSet()
        {
            #region 진짜 구버전
            //         // 카드 생성. 숫자와 무늬는 랜덤 생성
            //         for (int i=0; i<80; i++)
            //         {
            //	CCard card = new CCard();

            //	#region 덱 셋팅(구버전)
            //	//if(i<25)
            //	//            {
            //	//	card.name = "Bang!";		// 뱅 * 25

            //	//            }
            //	//else if(i<37)
            //	//            {
            //	//	card.name = "Missed!";		// 빗나감 * 12
            //	//            }
            //	//else if(i<43)
            //	//            {
            //	//	card.name = "Beer!";		// 맥주 * 6
            //	//            }
            //	//else if(i<44)
            //	//            {
            //	//	card.name = "Gatling!";		// 기관총 * 1
            //	//            }
            //	//else if(i<47)
            //	//            {
            //	//	card.name = "Duel!";		// 결투 * 3
            //	//            }
            //	//else if(i<49)
            //	//            {
            //	//	card.name = "Indian!";		// 인디언 * 2
            //	//            }
            //	//else if(i<50)
            //	//            {
            //	//	card.name = "Saloon!";		// 주점 * 1
            //	//            }
            //	//else if (i < 54)
            //	//{
            //	//	card.name = "Panic!";		// 강탈 * 4
            //	//}
            //	//else if (i < 58)
            //	//{
            //	//	card.name = "Cat Balou!";	// 캣벌로우 * 4
            //	//}
            //	//else if (i < 60)
            //	//{
            //	//	card.name = "General Store";	// 잡화점 * 2
            //	//}
            //	//else if (i < 62)
            //	//{
            //	//	card.name = "Stagecoach";		// 역마차 * 2
            //	//}
            //	//else if (i < 63)
            //	//{
            //	//	card.name = "Wells Fargo";       // 웰스파고 은행 * 1
            //	//}
            //	//else if (i < 66)
            //	//{
            //	//	card.name = "Schofield";       // 스코필드(총) * 3
            //	//}
            //	//else if (i < 67)
            //	//{
            //	//	card.name = "Remington";       // 레밍턴(총) * 1
            //	//}
            //	//else if (i < 68)
            //	//{
            //	//	card.name = "Carabine";			// 카빈(총) * 1
            //	//}
            //	//else if (i < 69)
            //	//{
            //	//	card.name = "Winchester";       // 윈체스터(총) * 1
            //	//}
            //	//else if (i < 71)
            //	//{
            //	//	card.name = "Volcanic";			// 볼캐닉(총) * 2
            //	//}
            //	//else if (i < 72)
            //	//{
            //	//	card.name = "Scope";			 // 조준경 * 1
            //	//}
            //	//else if (i < 74)
            //	//{
            //	//	card.name = "Mustang";			// 야생마 * 2
            //	//}
            //	//else if (i < 76)
            //	//{
            //	//	card.name = "Barrel";			 // 술통 * 2
            //	//}
            //	//else if (i < 79)
            //	//{
            //	//	card.name = "Jail";				// 감옥 * 3
            //	//}
            //	//else if (i < 80)
            //	//{
            //	//	card.name = "Dynamite";       // 다이너마이트 * 1
            //	//}
            //	#endregion

            //	//deck.Add()
            //}
            #endregion

            #region 카드 객체 생성 0 ~79
            CCard card0 = new CCard();
			CCard card1 = new CCard();
			CCard card2 = new CCard();
			CCard card3 = new CCard();
			CCard card4 = new CCard();
			CCard card5 = new CCard();
			CCard card6 = new CCard();
			CCard card7 = new CCard();
			CCard card8 = new CCard();
			CCard card9 = new CCard();
			CCard card10 = new CCard();
			CCard card11 = new CCard();
			CCard card12 = new CCard();
			CCard card13 = new CCard();
			CCard card14 = new CCard();
			CCard card15 = new CCard();
			CCard card16 = new CCard();
			CCard card17 = new CCard();
			CCard card18 = new CCard();
			CCard card19 = new CCard();
			CCard card20 = new CCard();
			CCard card21 = new CCard();
			CCard card22 = new CCard();
			CCard card23 = new CCard();
			CCard card24 = new CCard();
			CCard card25 = new CCard();
			CCard card26 = new CCard();
			CCard card27 = new CCard();
			CCard card28 = new CCard();
			CCard card29 = new CCard();
			CCard card30 = new CCard();
			CCard card31 = new CCard();
			CCard card32 = new CCard();
			CCard card33 = new CCard();
			CCard card34 = new CCard();
			CCard card35 = new CCard();
			CCard card36 = new CCard();
			CCard card37 = new CCard();
			CCard card38 = new CCard();
			CCard card39 = new CCard();
			CCard card40 = new CCard();
			CCard card41 = new CCard();
			CCard card42 = new CCard();
			CCard card43 = new CCard();
			CCard card44 = new CCard();
			CCard card45 = new CCard();
			CCard card46 = new CCard();
			CCard card47 = new CCard();
			CCard card48 = new CCard();
			CCard card49 = new CCard();
			CCard card50 = new CCard();
			CCard card51 = new CCard();
			CCard card52 = new CCard();
			CCard card53 = new CCard();
			CCard card54 = new CCard();
			CCard card55 = new CCard();
			CCard card56 = new CCard();
			CCard card57 = new CCard();
			CCard card58 = new CCard();
			CCard card59 = new CCard();
			CCard card60 = new CCard();
			CCard card61 = new CCard();
			CCard card62 = new CCard();
			CCard card63 = new CCard();
			CCard card64 = new CCard();
			CCard card65 = new CCard();
			CCard card66 = new CCard();
			CCard card67 = new CCard();
			CCard card68 = new CCard();
			CCard card69 = new CCard();
			CCard card70 = new CCard();
			CCard card71 = new CCard();
			CCard card72 = new CCard();
			CCard card73 = new CCard();
			CCard card74 = new CCard();
			CCard card75 = new CCard();
			CCard card76 = new CCard();
			CCard card77 = new CCard();
			CCard card78 = new CCard();
			CCard card79 = new CCard();
            #endregion

            #region 뱅 카드 목록(0~24/ 총 25)
            card0.name = "BANG";
			card0.shape = "HEART";
			card0.number = "Q";

			card1.name = "BANG";
			card1.shape = "HEART";
			card1.number = "K";

			card2.name = "BANG";
			card2.shape = "HEART";
			card2.number = "A";

			card3.name = "BANG";
			card3.shape = "DIAMOND";
			card3.number = "2";

			card4.name = "BANG";
			card4.shape = "DIAMOND";
			card4.number = "3";

			card5.name = "BANG";
			card5.shape = "DIAMOND";
			card5.number = "4";

			card6.name = "BANG";
			card6.shape = "DIAMOND";
			card6.number = "5";

			card7.name = "BANG";
			card7.shape = "DIAMOND";
			card7.number = "6";

			card8.name = "BANG";
			card8.shape = "DIAMOND";
			card8.number = "7";

			card9.name = "BANG";
			card9.shape = "DIAMOND";
			card9.number = "8";

			card10.name = "BANG";
			card10.shape = "DIAMOND";
			card10.number = "9";

			card11.name = "BANG";
			card11.shape = "DIAMOND";
			card11.number = "10";

			card12.name = "BANG";
			card12.shape = "DIAMOND";
			card12.number = "J";

			card13.name = "BANG";
			card13.shape = "DIAMOND";
			card13.number = "Q";

			card14.name = "BANG";
			card14.shape = "DIAMOND";
			card14.number = "K";

			card15.name = "BANG";
			card15.shape = "DIAMOND";
			card15.number = "A";

			card16.name = "BANG";
			card16.shape = "CLOVER";
			card16.number = "2";

			card17.name = "BANG";
			card17.shape = "CLOVER";
			card17.number = "3";

			card18.name = "BANG";
			card18.shape = "CLOVER";
			card18.number = "4";

			card19.name = "BANG";
			card19.shape = "CLOVER";
			card19.number = "5";

			card20.name = "BANG";
			card20.shape = "CLOVER";
			card20.number = "6";

			card21.name = "BANG";
			card21.shape = "CLOVER";
			card21.number = "7";

			card22.name = "BANG";
			card22.shape = "CLOVER";
			card22.number = "8";

			card23.name = "BANG";
			card23.shape = "CLOVER";
			card23.number = "9";

			card24.name = "BANG";
			card24.shape = "SPADE";
			card24.number = "A";
			#endregion

			#region 빗나감 카드 목록(25~36/ 총 12)
			card25.name = "MISSED";
			card25.shape = "SPADE";
			card25.number = "2";

			card26.name = "MISSED";
			card26.shape = "SPADE";
			card26.number = "3";

			card27.name = "MISSED";
			card27.shape = "SPADE";
			card27.number = "4";

			card28.name = "MISSED";
			card28.shape = "SPADE";
			card28.number = "5";

			card29.name = "MISSED";
			card29.shape = "SPADE";
			card29.number = "6";

			card30.name = "MISSED";
			card30.shape = "SPADE";
			card30.number = "7";

			card31.name = "MISSED";
			card31.shape = "SPADE";
			card31.number = "8";

			card32.name = "MISSED";
			card32.shape = "CLOVER";
			card32.number = "10";

			card33.name = "MISSED";
			card33.shape = "CLOVER";
			card33.number = "J";

			card34.name = "MISSED";
			card34.shape = "CLOVER";
			card34.number = "Q";

			card35.name = "MISSED";
			card35.shape = "CLOVER";
			card35.number = "K";

			card36.name = "MISSED";
			card36.shape = "CLOVER";
			card36.number = "A";
			#endregion

			#region 맥주 카드 목록(37~42 / 총 6)
			card37.name = "BEER";
			card37.shape = "HEART";
			card37.number = "6";

			card38.name = "BEER";
			card38.shape = "HEART";
			card38.number = "7";

			card39.name = "BEER";
			card39.shape = "HEART";
			card39.number = "8";

			card40.name = "BEER";
			card40.shape = "HEART";
			card40.number = "9";

			card41.name = "BEER";
			card41.shape = "HEART";
			card41.number = "10";

			card42.name = "BEER";
			card42.shape = "HEART";
			card42.number = "J";
			#endregion

			#region 기관총 카드 목록(43 / 총 1)
			card43.name = "GATLING";
			card43.shape = "HEART";
			card43.number = "10";
			#endregion

			#region 결투 카드 목록(44~46 / 총 3)
			card44.name = "DUAL";
			card44.shape = "DIAMOND";
			card44.number = "Q";
			
			card45.name = "DUAL";
			card45.shape = "CLOVER";
			card45.number = "8";

			card46.name = "DUAL";
			card46.shape = "SPADE";
			card46.number = "J";
			#endregion

			#region 인디언 카드 목록(47~48 / 총 2)
			card47.name = "INDIANS";
			card47.shape = "DIAMOND";
			card47.number = "K";

			card48.name = "INDIANS";
			card48.shape = "DIAMOND";
			card48.number = "A";
			#endregion

			#region 주점 카드 목록 (49 / 총 1)
			card49.name = "SALOON";
			card49.shape = "HEART";
			card49.number = "5";
			#endregion

			#region 강탈 카드 목록 (50~53 / 총 4)
			card50.name = "PANIC";
			card50.shape = "DIAMOND";
			card50.number = "8";

			card51.name = "PANIC";
			card51.shape = "HEART";
			card51.number = "J";

			card52.name = "PANIC";
			card52.shape = "HEART";
			card52.number = "Q";

			card53.name = "PANIC";
			card53.shape = "HEART";
			card53.number = "A";
			#endregion

			#region 캣 벌로우 카드 목록(54~57 / 총 4)
			card54.name = "CAT BALOU";
			card54.shape = "DIAMOND";
			card54.number = "9";

			card55.name = "CAT BALOU";
			card55.shape = "DIAMOND";
			card55.number = "10";

			card56.name = "CAT BALOU";
			card56.shape = "DIAMOND";
			card56.number = "J";

			card57.name = "CAT BALOU";
			card57.shape = "HEART";
			card57.number = "K";
			#endregion

			#region 잡화점 카드 목록(58~59 / 총 2)
			card58.name = "GENERAL STORE";
			card58.shape = "CLOVER";
			card58.number = "9";

			card59.name = "GENERAL STORE";
			card59.shape = "SPADE";
			card59.number = "Q";
			#endregion

			#region 역마차 카드 목록(60~61 / 총 2)
			card60.name = "STAGECOACH";
			card60.shape = "CLOVER";
			card60.number = "9";

			card61.name = "STAGECOACH";
			card61.shape = "SPADE";
			card61.number = "9";
			#endregion

			#region 웰스파고 은행 카드 목록(62 / 총 1)
			card62.name = "WELLS FARGO";
			card62.shape = "HEART";
			card62.number = "3";
			#endregion

			/*  여기서부터 장착 카드  */

			#region 스코필드 카드 목록(63~65 / 총 3)
			card63.name = "SCHOFIELD";
			card63.shape = "CLOVER";
			card63.number = "J";

			card64.name = "SCHOFIELD";
			card64.shape = "CLOVER";
			card64.number = "Q";

			card65.name = "SCHOFIELD";
			card65.shape = "SPADE";
			card65.number = "K";
			#endregion

			#region 레밍턴 카드 목록(66 / 총 1)
			card66.name = "REMINGTON";
			card66.shape = "CLOVER";
			card66.number = "K";
			#endregion

			#region 카빈 카드 목록(67 / 총 1)
			card67.name = "CARABINE";
			card67.shape = "CLOVER";
			card67.number = "A";
			#endregion

			#region 윈체스터 카드 목록(68 / 총 1)
			card68.name = "WINCHESTER";
			card68.shape = "SPADE";
			card68.number = "8";
			#endregion

			#region 볼캐닉 카드 목록(69~70 / 총 2)
			card69.name = "VOLCANIC";
			card69.shape = "SPADE";
			card69.number = "10";

			card70.name = "VOLCANIC";
			card70.shape = "CLOVER";
			card70.number = "10";
			#endregion

			#region 조준경 카드 목록(71 / 총 1)
			card71.name = "SCOPE";
			card71.shape = "SPADE";
			card71.number = "A";
			#endregion

			#region 야생마 카드 목록(72~73 / 총 2)
			card72.name = "MUSTANG";
			card72.shape = "HEART";
			card72.number = "8";

			card73.name = "MUSTANG";
			card73.shape = "HEART";
			card73.number = "9";
			#endregion

			#region 술통 카드 목록(74~75 / 총 2)
			card74.name = "BARREL";
			card74.shape = "HEART";
			card74.number = "Q";

			card75.name = "BARREL";
			card75.shape = "HEART";
			card75.number = "K";
			#endregion

			#region 감옥 카드 목록(76~78 / 총 3)
			card76.name = "JAIL";
			card76.shape = "HEART";
			card76.number = "4";

			card77.name = "JAIL";
			card77.shape = "SPADE";
			card77.number = "10";

			card78.name = "JAIL";
			card78.shape = "SPADE";
			card78.number = "J";
			#endregion

			#region 다이너마이트 카드 목록(79 / 총 1)
			card79.name = "DYNAMITE";
			card79.shape = "HEART";
			card79.number = "2";
            #endregion

            #region 덱에 카드 삽입
            deck.Add(card0);
			deck.Add(card1);
			deck.Add(card2);
			deck.Add(card3);
			deck.Add(card4);
			deck.Add(card5);
			deck.Add(card6);
			deck.Add(card7);
			deck.Add(card8);
			deck.Add(card9);

			deck.Add(card10);
			deck.Add(card11);
			deck.Add(card12);
			deck.Add(card13);
			deck.Add(card14);
			deck.Add(card15);
			deck.Add(card16);
			deck.Add(card17);
			deck.Add(card18);
			deck.Add(card19);

			deck.Add(card20);
			deck.Add(card21);
			deck.Add(card22);
			deck.Add(card23);
			deck.Add(card24);
			deck.Add(card25);
			deck.Add(card26);
			deck.Add(card27);
			deck.Add(card28);
			deck.Add(card29);

			deck.Add(card30);
			deck.Add(card31);
			deck.Add(card32);
			deck.Add(card33);
			deck.Add(card34);
			deck.Add(card35);
			deck.Add(card36);
			deck.Add(card37);
			deck.Add(card38);
			deck.Add(card39);

			deck.Add(card40);
			deck.Add(card41);
			deck.Add(card42);
			deck.Add(card43);
			deck.Add(card44);
			deck.Add(card45);
			deck.Add(card46);
			deck.Add(card47);
			deck.Add(card48);
			deck.Add(card49);

			deck.Add(card50);
			deck.Add(card51);
			deck.Add(card52);
			deck.Add(card53);
			deck.Add(card54);
			deck.Add(card55);
			deck.Add(card56);
			deck.Add(card57);
			deck.Add(card58);
			deck.Add(card59);

			deck.Add(card60);
			deck.Add(card61);
			deck.Add(card62);
			deck.Add(card63);
			deck.Add(card64);
			deck.Add(card65);
			deck.Add(card66);
			deck.Add(card67);
			deck.Add(card68);
			deck.Add(card69);

			deck.Add(card70);
			deck.Add(card71);
			deck.Add(card72);
			deck.Add(card73);
			deck.Add(card74);
			deck.Add(card75);
			deck.Add(card76);
			deck.Add(card77);
			deck.Add(card78);
			deck.Add(card79);
            #endregion

            return null;
        }

		public void ResetGameData()
        {
			CharacterChoice();
        }

		public void CharacterChoice()
        {
			List<string> TempCharList = new List<string>();

			TempCharList.Add("Willy_The_Kid");
			TempCharList.Add("Clamity_Janet");
			TempCharList.Add("Kit_Carlson");
			TempCharList.Add("Bart_Cassidy");
			TempCharList.Add("Sid_Ketchum");

			TempCharList.Add("Lucky_Duke");
			TempCharList.Add("Jourdonnais");
			TempCharList.Add("Black_Jack");
			TempCharList.Add("Vulture_Sam");
			TempCharList.Add("Jesse_Jones");

			TempCharList.Add("Suzy_Lafayette");
			TempCharList.Add("Pedro_Ramirez");
			TempCharList.Add("Slab_The_Killer");
			TempCharList.Add("Rose_Doolan");
			TempCharList.Add("Paul_Regret");

			TempCharList.Add("El_Gringo");

			Characters = CharacterShuffle(TempCharList);
		}

		public List<string> CharacterShuffle(List<string> values)
        {
			Random rand = new Random();
			var shuffled = values.OrderBy(_ => rand.Next()).ToList();

			return shuffled;
        }

		public List<CCard> CardShuffle(List<CCard> values)
        {
			Random rand = new Random();
			var shuffled = values.OrderBy(_ => rand.Next()).ToList();

			return shuffled;
        }

        // 유저 목록 및 배치 초기화
        public void UserReset()
        {

        }

		/// <summary>
		/// 어떤 플레이어가 BANG 사용했을 경우, 자신의 턴에.
		/// </summary>
		/// <param name="target_index">공격 대상 플레이어</param>
		public void UseBang(byte target_index)
        {
			// 플레이어 구분 어케함?
			CPacket msg = CPacket.create((short)PROTOCOL.USECARD);
			msg.push("BANG");
			//msg.push(target_index);
			// 
        }

		// BANG의 대상이 된 플레이어가 빗나감을 사용할 경우 호출
		public void UseMissed()
        {

        }

		// 모든 플레이어에게 적용되는 카드 사용시
		// 캣 벌로우, 인디언, 기관총, 잡화점, 주점 등
		public void UseBroadCastCard()
        {

        }

		// 웰스파고(count = 3), 역마차(count = 2) 사용시
		public void UseGetCards(int count)
		{

		}

		// 맥주 사용시
		public void UseBeer()
		{

		}

		// 체력 깎는 경우(즉, 인디언이나 기관총 등에서 임의로 깎지말고, 클라이언트와 인터렉션 확인 후 처리할 것.)
		public void MinusHp()
        {

        }

        #region 브로드캐스트 사용되는 메서드 예시
  //      // 인디언 사용한 경우
  //      public void UseIndian()
  //      {

  //      }

		//// 머신건 사용한 경우
		//public void UseMachineGun()
		//{ 
		//}
		

		//// 잡화점 사용한 경우
		//public void UseGeneralStore()
  //      {

  //      }


		//// 캣 벌로우 사용시
		//public void UseCatBalou()
  //      {

  //      }

		//// 주점 사용시
		//public void UseSaloon()
  //      {

  //      }
        #endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"> 턴 종료 버튼을 누른 플레이어의 인덱스 번호 </param>
		public void TurnEnd(byte index)
        {
			Console.WriteLine(index + "의 턴 종료");

            #region 구버전
            //// 보드판 상태를 확인하여 게임이 끝났는지 검사한다.
            //if (!CHelper.can_play_more(this.table_board, get_opponent_player(), this.players))
            //{
            //	game_over();
            //	return;
            //}

            //// 아직 게임이 끝나지 않았다면 다음 플레이어로 턴을 넘긴다.
            //if (this.current_turn_player < this.players.Count - 1)
            //{
            //	++this.current_turn_player;
            //}
            //else
            //{
            //	// 다시 첫번째 플레이어의 턴으로 만들어 준다.
            //	this.current_turn_player = this.players[0].player_index;
            //}
            #endregion

            while(true)
            {
				// 맨 끝 인덱스인지 먼저 확인
				if(index == livingPlayers.Count)
                {
					index++;
                }
				// 살아있는 플레이어 중 맨 끝 인덱스가 아니라면.
				else
                {
					index++;
					break;
                }
            }

            // 턴을 시작한다.
            TurnStart(index);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"> 곧 시작하게 될 플레이어 </param>
		public void TurnStart(byte index)
        {
			Console.WriteLine(index + "의 턴 시작");
			// 턴을 진행할 수 있도록 준비 상태로 만든다.
			this.livingPlayers.ForEach(player => change_playerstate(player, PLAYER_STATE.READY_TO_TURN));

			CPacket msg = CPacket.create((short)PROTOCOL.START_PLAYER_TURN);
			msg.push(this.current_turn_player);
			broadcast(msg);
		}
        #endregion
    }
}
