using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Reversi
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ReversiSilnikAI silnik = new ReversiSilnikAI(1, 8, 8);
        private SolidColorBrush[] kolory = { Brushes.Beige, Brushes.Green, Brushes.Brown };
        string[] nazwyGraczy = { "", "Zielony", "Brązowy" };
        private Button[,] plansza;
        private bool graPrzeciwKoputerowi = true;
        private DispatcherTimer timer;
        private struct WspółrzędnePola
        {
            public int Poziomo, Pionowo;
        }
        private void uzgodnijZawartośćPlanszy()
        {
            for (int i = 0; i < silnik.SzerokośćPlanszy; i++)
                for (int j = 0; j < silnik.WysokośćPlanszy; j++)
                {
                    plansza[i, j].Background = kolory[silnik.PobierzStanPola(i, j)];
                    plansza[i, j].Content = silnik.PobierzStanPola(i, j).ToString();
                }
            przyciskKolorGracza.Background = kolory[silnik.NumerGraczaWykonującegoNastępnyRuch];
            liczbaPólZielony.Text = silnik.liczbaPólGracz1.ToString();
            liczbaPólBrązowy.Text = silnik.liczbaPólGracz2.ToString();
        }
        private static string symbolPola(int poziomo, int pionowo)
        {
            if (poziomo > 25 || pionowo > 8)
                return "(" + (poziomo + 1).ToString() + "," + (pionowo + 1).ToString() + ")";
            return "" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[poziomo] + "123456789"[pionowo];
        }
        void kliknięciePolaPlanszy(object sender, RoutedEventArgs e)
        {
            Button klikniętyPrzycisk = sender as Button;
            WspółrzędnePola współrzędne = (WspółrzędnePola)klikniętyPrzycisk.Tag;
            int zapamiętanyNumerGracza = silnik.NumerGraczaWykonującegoNastępnyRuch;
            if (silnik.PołóżKamień(współrzędne.Poziomo, współrzędne.Pionowo))
            {
                uzgodnijZawartośćPlanszy();
                if (zapamiętanyNumerGracza == 1) listaRuchówZielony.Items.Add(symbolPola(współrzędne.Poziomo, współrzędne.Pionowo));
                if (zapamiętanyNumerGracza == 2) listaRuchówBrązowy.Items.Add(symbolPola(współrzędne.Poziomo, współrzędne.Pionowo));
            }
            bool koniecGry = false;
            ReversiSilnik.SytuacjaNaPlanszy sytuacjaNaPlanszy = silnik.ZbadajSytuacjęNaPlanszy();
            switch (sytuacjaNaPlanszy)
            {
                case ReversiSilnik.SytuacjaNaPlanszy.BieżącyGraczNieMożeWykonaćRuchu:
                    MessageBox.Show("Gracz" + nazwyGraczy[silnik.NumerGraczaWykonującegoNastępnyRuch] + " nie może wykonać ruchu");
                    silnik.Pasuj();
                    uzgodnijZawartośćPlanszy();
                    break;
                case ReversiSilnik.SytuacjaNaPlanszy.WszystkiePolaPlanszySąZajęte:
                    koniecGry = true;
                    break;
                case ReversiSilnik.SytuacjaNaPlanszy.ObajGraczeNieMogąWykonaćRuchu:
                    koniecGry = true;
                    MessageBox.Show("Żaden z graczy nie ma możliwości wykonania ruchu");
                    break;
                case ReversiSilnik.SytuacjaNaPlanszy.RuchJestMożliwy:
                    break;
            }
            if (koniecGry)
            {
                int numerZwycięzcy = silnik.NumerGraczaMającegoPrzewagę;
                if (numerZwycięzcy == 0)
                    MessageBox.Show("Gra zakończyła się remisem");
                if (numerZwycięzcy != 0)
                    MessageBox.Show("Wygrał gracz" + nazwyGraczy[silnik.NumerGraczaMającegoPrzewagę], Title, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                if (MessageBox.Show("Czy rozpocząć grę od nowa?", Title, MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    przygotowaniePlanszyDoNowejGry(1, silnik.SzerokośćPlanszy, silnik.WysokośćPlanszy);
                }
                else
                {
                    planszaSiatka.IsEnabled = false;
                    przyciskKolorGracza.IsEnabled = false;
                }

            }
            if (graPrzeciwKoputerowi && silnik.NumerGraczaWykonującegoNastępnyRuch == 2)
            //wykonajNajlepszyRuch();
            {
                if (timer == null)
                {
                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    timer.Tick +=
                        (_sender, _e) => { timer.IsEnabled = false; wykonajNajlepszyRuch(); };
                }
                timer.Start();
            }
        }
        private void przygotowaniePlanszyDoNowejGry(int numerGraczaZaczynającego,
            int szerokośćPlanszy, int wysokośćPlanszy)
        {
            silnik = new ReversiSilnikAI(numerGraczaZaczynającego, szerokośćPlanszy, wysokośćPlanszy);
            listaRuchówZielony.Items.Clear();
            listaRuchówBrązowy.Items.Clear();
            uzgodnijZawartośćPlanszy();
            planszaSiatka.IsEnabled = true;
            przyciskKolorGracza.IsEnabled = true;
        }
        private void zaznaczNajlepszyRuch()
        {
            WspółrzędnePola? współżędnaPola = ustalNajlepszyRuch();

            if (współżędnaPola.HasValue)
            {
                SolidColorBrush kolorPodpowiedzi =
                    kolory[silnik.NumerGraczaWykonującegoNastępnyRuch].Lerp(kolory[0], 0.5);
                plansza[współżędnaPola.Value.Poziomo, współżędnaPola.Value.Pionowo].Background = kolorPodpowiedzi;
            }
        }
        private void wykonajNajlepszyRuch()
        {
            WspółrzędnePola? współżędnaPola = ustalNajlepszyRuch();
            if (współżędnaPola.HasValue)
            {
                Button przycisk = plansza[współżędnaPola.Value.Poziomo, współżędnaPola.Value.Pionowo];
                kliknięciePolaPlanszy(przycisk, null);
            }
        }
        private WspółrzędnePola? ustalNajlepszyRuch()
        {
            if (planszaSiatka.IsEnabled) return null;
            if (silnik.liczbaPustychPól == 0)
            {
                MessageBox.Show("Nie ma wolnych pól", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            int poziomo, pionowo;
            try
            {
                silnik.ProponujNajlepszyRuch(out poziomo, out pionowo);
                return new WspółrzędnePola() { Poziomo = poziomo, Pionowo = pionowo };
            }
            catch
            {
                MessageBox.Show("Aktualny gracz nie może wykonać ruchu", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < silnik.SzerokośćPlanszy; i++)
                planszaSiatka.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < silnik.WysokośćPlanszy; i++)
                planszaSiatka.RowDefinitions.Add(new RowDefinition());
            plansza = new Button[silnik.SzerokośćPlanszy, silnik.WysokośćPlanszy];
            for (int i = 0; i < silnik.SzerokośćPlanszy; i++)
                for (int j = 0; j < silnik.WysokośćPlanszy; j++)
                {
                    Button przycisk = new Button();
                    przycisk.Margin = new Thickness(0);
                    planszaSiatka.Children.Add(przycisk);
                    Grid.SetColumn(przycisk, i);
                    Grid.SetRow(przycisk, j);
                    przycisk.Tag = new WspółrzędnePola { Poziomo = i, Pionowo = j };
                    przycisk.Click += new RoutedEventHandler(kliknięciePolaPlanszy);
                    plansza[i, j] = przycisk;
                }
            uzgodnijZawartośćPlanszy();
        }

        #region Metody zdarzeniowe menu głównego
        private void MenuItem_NowaGraDla1Gracza_Rozpoczyna_Komputer_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwKoputerowi = true;
            przygotowaniePlanszyDoNowejGry(2, 8, 8);
            wykonajNajlepszyRuch();
        }

        private void MenuItem_NowaGraDla1Gracza_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwKoputerowi = true;
            przygotowaniePlanszyDoNowejGry(1, 8, 8);
        }

        private void MenuItem_NowaDraDla2Graczy_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwKoputerowi = false;
            przygotowaniePlanszyDoNowejGry(1,8,8);
        }

        private void MenuItem_Zamknij_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_PodpowiedźRuchu_Click(object sender, RoutedEventArgs e)
        {
            zaznaczNajlepszyRuch();
        }

        private void MenuItem_RuchWykonywanyPrzezKomputer_Click(object sender, RoutedEventArgs e)
        {
            wykonajNajlepszyRuch();
        }

        private void MenuItem_ZasadyGry_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "W grze Reversi gracze zajmują na przemian pola planszy, przejmując przy tym wszystkie pola przeciwnika znajdujące się między nowo zajętym polem a innymi polami gracza wykonującego ruch. Celem gry jest zdobycie większej liczy pól niż przeciwnik. \n"
                + "Gracz może zająć jedynie takie pole, które pozwoli mu przejąć przynajmniej jedno pole przeciwnika. Jeżeli takiego pola nie ma, musi oddać ruch. \n"
                + "Gra kończy się w momencie zajęcia wszystkich pól lub gdy żaden z graczy nie może wykonać ruchu. \n",
                "Reversi - Zasady gry");
        }

        private void MenuItem_StrategiaKomputera_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Komputer kieruje się następującymi priorytetami (od najważniejszego): \n"+
                "1. Ustawić pionek w rogu. \n"+
                "2. Unikać ustawienia pionka tuż przy rogu. \n"+
                "3. Ustawić pionek przy krawędzi planszy. \n"+
                "4. Unikać ustawienia pionka w wierszu lub kolumnie oddalonej o jedno pole od krawędzi planszy. \n"+
                "5. Wybierać pole, w wyniku którego zdobyta zostanie największa liczba pól przeciwnika. \n",
                "Reversi - Strategia komputera");
        }

        private void MenuItem_OProgramie_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Przygotował Marcin Rybka \n",
                "Informacje o programie");
        }

        private void przyciskKolorGracza_Click(object sender, RoutedEventArgs e)
        {
            if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                wykonajNajlepszyRuch();
            else
                zaznaczNajlepszyRuch();
        }
        #endregion
    }
}
