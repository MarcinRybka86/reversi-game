using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    internal class ReversiSilnikAI : ReversiSilnik
    {
        public ReversiSilnikAI(int numerGraczaRozpoczynającego,
            int szerokośćPlanszy = 8, int wysokośćPlanszy = 8)
            : base(numerGraczaRozpoczynającego, szerokośćPlanszy, wysokośćPlanszy)
        {

        }
        private struct MożliwyRuch
        {
            public int poziomo;
            public int pionowo;
            public int priorytet;
            public MożliwyRuch(int poziomo, int pionowo, int priorytet)
            {
                this.poziomo = poziomo;
                this.pionowo = pionowo;
                this.priorytet = priorytet;
            }
            public int CompareTo(MożliwyRuch innyRuch)
            {
                return innyRuch.priorytet - this.priorytet;

            }
        }

        public void ProponujNajlepszyRuch(out int najlepszyRuchPoziomo, out int najlepszyRuchPionowo)
        {
            List<MożliwyRuch> możliweRuchy = new List<MożliwyRuch>();
            int skokPriorytetu = SzerokośćPlanszy + WysokośćPlanszy;

            for (int poziomo = 0; poziomo < SzerokośćPlanszy; poziomo++)
                for (int pionowo = 0; pionowo < WysokośćPlanszy; pionowo++)
                {
                    if (PobierzStanPola(poziomo, pionowo) == 0)
                    {
                        int priorytet = PołóżKamień(poziomo, pionowo, true);
                        if (priorytet > 0)
                        {
                            MożliwyRuch mr = new MożliwyRuch(poziomo, pionowo, priorytet);

                            if ((poziomo == 0 || poziomo == SzerokośćPlanszy - 1) &&
                                (pionowo == 0 || pionowo == WysokośćPlanszy - 1))
                                priorytet = priorytet + skokPriorytetu * skokPriorytetu;
                            if ((poziomo == 1 || poziomo == SzerokośćPlanszy - 2) &&
                                (pionowo == 1 || pionowo == WysokośćPlanszy - 2))
                                priorytet = priorytet - skokPriorytetu * skokPriorytetu;
                            if ((poziomo == 1 || poziomo == SzerokośćPlanszy - 2) &&
                                (pionowo == 0 || pionowo == WysokośćPlanszy - 1))
                                priorytet = priorytet - skokPriorytetu * skokPriorytetu;
                            if ((poziomo == 0 || poziomo == SzerokośćPlanszy - 1) &&
                                (pionowo == 1 || pionowo == WysokośćPlanszy - 2))
                                priorytet = priorytet - skokPriorytetu * skokPriorytetu;
                            if ((poziomo == 0 || poziomo == SzerokośćPlanszy - 1) ||
                                (pionowo == 0 || pionowo == WysokośćPlanszy - 1))
                                priorytet = priorytet + skokPriorytetu;
                            if ((poziomo == 1 || poziomo == SzerokośćPlanszy - 2) ||
                                (pionowo == 1 || pionowo == WysokośćPlanszy - 2))
                                priorytet = priorytet - skokPriorytetu;

                            mr.priorytet = priorytet;
                            możliweRuchy.Add(mr);

                        }
                    }
                }
            if (możliweRuchy.Count > 0)
            {
                możliweRuchy.Sort();
                najlepszyRuchPoziomo = możliweRuchy[0].poziomo;
                najlepszyRuchPionowo = możliweRuchy[0].pionowo;
            }
            else
            {
                throw new Exception("Brak ruchu");
            }
        }
    }
}
