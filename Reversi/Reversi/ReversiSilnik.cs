using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversi
{
    internal class ReversiSilnik
    {
        public int SzerokośćPlanszy { get; private set; }
        public int WysokośćPlanszy { get; private set; }
        public int NumerGraczaWykonującegoNastępnyRuch { get; private set; } = 1;
        private int[,] plansza;
        private int[] liczbaPól = new int[3];
        public int liczbaPustychPól { get { return liczbaPól[0]; } }
        public int liczbaPólGracz1 { get { return liczbaPól[1]; } }
        public int liczbaPólGracz2 { get { return liczbaPól[2]; } }
        public int NumerGraczaMającegoPrzewagę
        {
            get
            {
                if (liczbaPólGracz1 == liczbaPólGracz2) return 0;
                if (liczbaPólGracz1 > liczbaPólGracz2) return 1;
                return 2;
            }
        }
        public enum SytuacjaNaPlanszy
        {
            RuchJestMożliwy,
            BieżącyGraczNieMożeWykonaćRuchu,
            ObajGraczeNieMogąWykonaćRuchu,
            WszystkiePolaPlanszySąZajęte
        }
        public ReversiSilnik(int numerGraczaRozpoczynającego, int szerokośćPlanszy = 8, int wysokośćPlanszy = 8)
        {
            if (numerGraczaRozpoczynającego != 1 && numerGraczaRozpoczynającego != 2)
                throw new Exception("Nieprawidłowy numer gracza rozpoczynającego grę");
            SzerokośćPlanszy = szerokośćPlanszy;
            WysokośćPlanszy = wysokośćPlanszy;
            NumerGraczaWykonującegoNastępnyRuch = numerGraczaRozpoczynającego;
            plansza = new int[szerokośćPlanszy, wysokośćPlanszy];
            czyśćPlanszę();
            obliczLiczbyPól();
        }
        private void czyśćPlanszę()
        {
            for (int i = 0; i < SzerokośćPlanszy; i++)
                for (int j = 0; j < WysokośćPlanszy; j++)
                    plansza[i, j] = 0;
            int środekSzerokości = SzerokośćPlanszy / 2;
            int środekWysokości = WysokośćPlanszy / 2;
            plansza[środekSzerokości, środekWysokości] = 1;
            plansza[środekSzerokości - 1, środekWysokości - 1] = 1;
            plansza[środekSzerokości - 1, środekWysokości] = 2;
            plansza[środekSzerokości, środekWysokości - 1] = 2;

        }
        public int PobierzStanPola(int poziomo, int pionowo)
        {
            if (!czyWspółżędnePolaPrawidłowe(poziomo, pionowo))
                throw new Exception("Nieprawidłowe Współżędne Pola");
            return plansza[poziomo, pionowo];
        }
        private bool czyWspółżędnePolaPrawidłowe(int poziomo, int pionowo)
        {
            if (poziomo >= 0 && pionowo >= 0 && poziomo < SzerokośćPlanszy && pionowo < WysokośćPlanszy)
                return true;
            return false;
        }
        private static int numerPrzeciwnika(int numerGracza)
        {
            if (numerGracza == 1) return 2;
            return 1;
        }
        protected int PołóżKamień(int poziomo, int pionowo, bool tylkoTest)
        {
            int ilePólPrzejętych = 0;
            if (plansza[poziomo, pionowo] != 0) return -1;
            for (int kierunekPoziomo = -1; kierunekPoziomo <= 1; kierunekPoziomo++)
                for (int kierunekPionowo = -1; kierunekPionowo <= 1; kierunekPionowo++)
                {
                    if (kierunekPoziomo == 0 && kierunekPionowo == 0) continue;
                    bool osiągniętoKrawędźPlanszy = false;
                    bool znalezionoPustePole = false;
                    bool znalezionoKamieńGraczaWykonującegoRych = false;
                    bool znalezionoKamieńPrzeciwnika = false;
                    int i = poziomo;
                    int j = pionowo;
                    do
                    {
                        i += kierunekPoziomo;
                        j += kierunekPionowo;
                        if (!czyWspółżędnePolaPrawidłowe(i, j))
                            osiągniętoKrawędźPlanszy = true;
                        if (!osiągniętoKrawędźPlanszy)
                        {
                            if (plansza[i, j] == NumerGraczaWykonującegoNastępnyRuch)
                                znalezionoKamieńGraczaWykonującegoRych = true;
                            else if (plansza[i, j] == numerPrzeciwnika(NumerGraczaWykonującegoNastępnyRuch))
                                znalezionoKamieńPrzeciwnika = true;
                            else if (plansza[i, j] == 0) znalezionoPustePole = true;
                        }
                    }
                    while (!(osiągniętoKrawędźPlanszy || znalezionoPustePole
                    || znalezionoKamieńGraczaWykonującegoRych));
                    bool położenieKamieniaJestMożliwe = znalezionoKamieńPrzeciwnika && znalezionoKamieńGraczaWykonującegoRych && !znalezionoPustePole;
                    if (położenieKamieniaJestMożliwe)
                    {
                        int maxIndeks = Math.Max(Math.Abs(i - poziomo),
                            Math.Abs(j - pionowo));
                        if (!tylkoTest)
                        {
                            for (int indeks = 0; indeks < maxIndeks; indeks++)
                                plansza[poziomo + indeks * kierunekPoziomo, pionowo + indeks * kierunekPionowo] = NumerGraczaWykonującegoNastępnyRuch;
                        }
                        ilePólPrzejętych = ilePólPrzejętych + maxIndeks - 1;
                    }
                }

            if (ilePólPrzejętych > 0 && !tylkoTest)
                zmieńBieżącegoGracza();
            obliczLiczbyPól();
            return ilePólPrzejętych;
        }
        public bool PołóżKamień(int poziomo, int pionowo)
        {
            return PołóżKamień(poziomo, pionowo, false) > 0;
        }
        private void zmieńBieżącegoGracza()
        {
            NumerGraczaWykonującegoNastępnyRuch = numerPrzeciwnika(NumerGraczaWykonującegoNastępnyRuch);
        }
        private void obliczLiczbyPól()
        {
            liczbaPól[0] = liczbaPól[1] = liczbaPól[2] = 0;
            for (int i = 0; i < SzerokośćPlanszy; i++)
                for (int j = 0; j < WysokośćPlanszy; j++)
                    liczbaPól[plansza[i, j]]++;
        }
        private bool czyBieżącyGraczMożeWykonaćRych()
        {
            int liczbaPoprawnychPól = 0;
            for (int i = 0; i < SzerokośćPlanszy; i++)
                for (int j = 0; j < WysokośćPlanszy; j++)
                {
                    if (PołóżKamień(i, j, true) > 0) liczbaPoprawnychPól++;

                }
            return liczbaPoprawnychPól > 0;
        }
        public void Pasuj()
        {
            if (czyBieżącyGraczMożeWykonaćRych())
                throw new Exception("Gracz nie może wykonać ruchu, jeżeli wykonanie ruchu jest możliwe");
            zmieńBieżącegoGracza();
        }
        public SytuacjaNaPlanszy ZbadajSytuacjęNaPlanszy()
        {
            if (liczbaPustychPól == 0) return SytuacjaNaPlanszy.WszystkiePolaPlanszySąZajęte;
            bool czyMożliwyRuch = czyBieżącyGraczMożeWykonaćRych();
            if (czyMożliwyRuch) return SytuacjaNaPlanszy.RuchJestMożliwy;

            zmieńBieżącegoGracza();
            bool czyMożliwyRuchPrzeciwnika = czyBieżącyGraczMożeWykonaćRych();
            zmieńBieżącegoGracza();
            if (czyMożliwyRuchPrzeciwnika) return SytuacjaNaPlanszy.BieżącyGraczNieMożeWykonaćRuchu;

            return SytuacjaNaPlanszy.ObajGraczeNieMogąWykonaćRuchu;
        }
    }
}
