using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BangServer
{
    using FreeNet;

    public class CPlayer
    {
        CGameUser owner;
        public byte player_index { get; private set; }      // 플레이어를 분간하는 변수.
                                                            //public string playerId { get; set; }					// 플레이어 아이디를 저장하는 변수
        public string playerJob { get; set; }                   // 플레이어 직업. 보안관, 무법자 등
        public string charName { get; set; }                    // 플레이어 캐릭터. 근데 public 인데 get,set, 쓰는 의미가 있나?
        private int maxLife;
        public int cardCount { get; set; }              // 손 패수

        // 거리 측정용
        public int range { get; set; }                  // 플레이어가 볼 때
        private int defaultRange;                      // 기본 사거리
        public int depth { get; set; }                  // 플레이어를 볼 때
        // 장비
        private string gun;
        private string mirono;
        private string mustang;
        private string barile;


        public int MaxLife
        {
            get { return maxLife; }
            set 
            { 
                maxLife = value;
                life = MaxLife;
            }
        }
        
        private int life;
        public int Life
        {
            get { return life; }
            set
            {
                life = value;
                if(life <= 0)
                {
                    // 플레이어 사망 시
                    Console.WriteLine("여기에 사망시 쓰일 메서드 기술");
                }
                else if(life >= maxLife)
                {
                    // 최대치 이상으로 체력 회복 방지
                    life = maxLife;
                }
            }
        }

        // 게임 초기에 한번 셋팅 된 후 고정
        public int DefaultRange
        {
            get { return defaultRange; }
            set
            {
                defaultRange = value;

                range = defaultRange;
            }
        }

        public string Gun
        {
            get { return gun; }
            set 
            {
                gun = value;

                // 조준경 옵션 추가할 것
                if (gun == "COLT")
                    range = defaultRange;
                else if (gun == "SCHOFIELD")
                    range = defaultRange+1;
                else if (gun == "REMINGTON")
                    range = defaultRange+2;
                else if (gun == "CARABINE")
                    range = defaultRange+3;
                else if (gun == "WINCHESTER")
                    range = defaultRange+4;
                else if (gun == "VOLCANIC")
                    range = defaultRange;
            }
        }

        public string Mirono
        {
            get { return mirono; }
            set
            {
                mirono = value;

                if (mirono == "true")           // true면 장착 중, 아니면 장착 해제
                    range++;
                else
                    range--;
            }
        }

        public string Mustang
        {
            get { return mustang; }
            set
            {
                mustang = value;

                if (mustang == "true")
                    depth++;
                else
                    depth--;
            }
        }

        public string Barile
        {
            get { return barile; }
            set{ barile = value; }
        }

        public List<short> viruses { get; private set; }

        public CPlayer(CGameUser user, byte player_index)
        {
            this.owner = user;
            this.player_index = player_index;
            this.viruses = new List<short>();
        }

        public void reset()
        {
            this.viruses.Clear();
        }

        public void add_cell(short position)
        {
            this.viruses.Add(position);
        }

        public void remove_cell(short position)
        {
            this.viruses.Remove(position);
        }

        public void send(CPacket msg)
        {
            this.owner.send(msg);
            CPacket.destroy(msg);
        }

        public void send_for_broadcast(CPacket msg)
        {
            this.owner.send(msg);
        }

        public int get_virus_count()
        {
            return this.viruses.Count;
        }

        // 여기서부터 뱅 용

    }
}
