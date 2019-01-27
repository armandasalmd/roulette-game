using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CSGO2x.com
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<int> sara = new List<int>();           // sarasas skaiciams saugoti
        private int laimejau = 0;                           // sarasas laimejimu skaiciui saugoti
        private int pralaimejau = 0;                        // sarasas pralaimejimu skaiciui saugoti
        private const int eilutes_ilgis = 10;               // issaugojimo faile eiluteje telpantys skaiciai
        private const int sk_nuo = 0;
        private const int sk_iki = 14;
        private const int rut_max = 10;
        private DispatcherTimer _taimeris = new DispatcherTimer();              // laikrodzio funkcijos sukurimas
        private int[] mas = new int[rut_max];                                   // laikomi paskutiniai rutuliukai
        private Border[] _border = new Border[rut_max];
        private TextBlock[] _history = new TextBlock[rut_max];
        private int[] traukiniai = new int[10];
        private int[] daugiau_maziau = new int[2];                              // [0] - daugiau, [1] - maziau
        private int pinigai;
        private int suma_pastatyta;

        public MainWindow()
        {
            InitializeComponent();

            _taimeris.Tick += Laikrodis;                                        // priskiriama funkcija kuria laikmatis vykdys
            _taimeris.Interval = new TimeSpan(0, 0, 2);                         // nustatomas laikmacio intervalas
            
            SkaitytiDuomenis();
            money.Content = pinigai.ToString(); 
            PaskutiniaiRutuliukai(mas);
            AtnaujintiProcentus();

            for (int i = 0; i < rut_max; i++)
            {
                rutuliukai.Children.Add(new Border());
                _border[i] = (Border)rutuliukai.Children[i];
                _history[i] = new TextBlock();
                _border[i].Child = _history[i];

                _border[i].BorderThickness = new Thickness(1);
                _border[i].Width = 32;
                _border[i].Height = 32;
                _border[i].Margin = new Thickness(1.5);
                _border[i].CornerRadius = new CornerRadius(14);
                _history[i].HorizontalAlignment = HorizontalAlignment.Center;
                _history[i].VerticalAlignment = VerticalAlignment.Center;
                _history[i].FontSize = 22;
                _history[i].Foreground = Brushes.Azure;
                _history[i].Opacity = 10;
            }
            AtnaujintiIstorija();
            TraukiniuStatistika();
            AtnaujintiMygtukus();
        }

        ~MainWindow()
        {
            IssaugotiDuomenis();
        }

        private void SpalvintiFona(Border ab, char sp)
        {
            BrushConverter bc = new BrushConverter();
            switch (sp)
            {
                case 'r':
                    ab.Background = (Brush)bc.ConvertFrom("#C9302C");
                    ab.BorderBrush = Brushes.DarkRed;
                    break;
                case 'g':
                    ab.Background = (Brush)bc.ConvertFrom("#4caf50");
                    ab.BorderBrush = Brushes.DarkGreen;
                    break;
                case 'b':
                    ab.Background = (Brush)bc.ConvertFrom("#383838");
                    ab.BorderBrush = Brushes.Black;
                    break;
            }
        }

        private void AtnaujintiIstorija()
        {
            int ilgis = mas.Length, iki = 0;
            if (ilgis < rut_max)
                iki = ilgis;
            else
                iki = rut_max;

            for(int i = 0; i < iki; i++)
            {
                _history[i].Text = mas[i].ToString();
                SpalvintiFona(_border[i], Conv_spalva(mas[i]));
            }
        }

        private char Conv_spalva(int num)
        {
            if (num == 0)
                return 'g';
            else if (num >= 1 && num <= 7)
                return 'r';
            else
                return 'b';
        }

        private int Gauti_total(char sp)
        {
            int total = 0;
            for (int i = 0; i < sara.Count; i++)
            {
                if (sp == Conv_spalva(sara[i]))
                    total++;
            }
            return total;
        }

        private void AtnaujintiProcentus()
        {
            int total_red = Gauti_total('r');
            int total_green = Gauti_total('g');
            int total_black = Gauti_total('b');
            int total = total_red + total_green + total_black;

            try
            {
                red_s.Text = (total_red * 100 / total).ToString() + "%";
                green_s.Text = (total_green * 100 / total).ToString() + "%";
                black_s.Text = (total_black * 100 / total).ToString() + "%";
            } catch
            {
                red_s.Text = "Klaida";
                green_s.Text = "Klaida";
                black_s.Text = "Klaida";
            }
        }

        private void PaskutiniaiRutuliukai(int[] rut)
        {
            int last = sara.Count, j = 0;
            if (last < rut_max)
                for (int i = last - 1; i >= 0; i--)
                {
                    mas[j] = sara[i];
                    j++;
                }
            else
                for (int i = last - 1; i >= last - rut_max; i--)
                {
                    mas[j] = sara[i];
                    j++;
                }
            j = 0;
        }

        private void IssaugotiDuomenis()
        {
            /// <summary>
            /// Issaugomi skaicia
            /// </summary>
            using (StreamWriter writer = new StreamWriter("skaiciai.txt"))
            {
                int pasikartojimas = 0;
                foreach (int number in sara)
                {
                    if(pasikartojimas >= eilutes_ilgis)
                    {
                        pasikartojimas = 0;
                        writer.WriteLine(" ");
                    }
                    pasikartojimas++;
                    writer.Write(number + " ");
                }
                writer.WriteLine("");
                writer.WriteLine("/pabaiga");
            }
            /// <summary>
            /// Issaugomi kiti duomenys
            /// </summary>
            using (StreamWriter writer2 = new StreamWriter("statistika.txt"))
            {
                writer2.WriteLine("Laimejau: " + laimejau.ToString() + " ");
                writer2.WriteLine("Pralaimejau: " + pralaimejau.ToString() + " ");
                writer2.WriteLine("Daugiau: " + daugiau_maziau[0].ToString() + " ");
                writer2.WriteLine("Maziau: " + daugiau_maziau[1].ToString() + " ");
                writer2.WriteLine("Pinigai: " + pinigai.ToString() + " ");
            }
            /// <summary>
            /// issaugomi raidiniai iskritimai
            /// </summary>
            using (StreamWriter writer3 = new StreamWriter("spalvos.txt"))
            {
                int pasikartojimas = 0;
                foreach (int number in sara)
                {
                    if (pasikartojimas >= eilutes_ilgis)
                    {
                        pasikartojimas = 0;
                        writer3.WriteLine(" ");
                    }
                    pasikartojimas++;
                    writer3.Write(Conv_spalva(number) + " ");
                }
                writer3.WriteLine("");
                writer3.WriteLine("/pabaiga");
            }
        }

        private void SkaitytiDuomenis()
        {
            /// <summary>
            /// Skaitomi skaiciai
            /// </summary>
            using (StreamReader reader = new StreamReader("skaiciai.txt"))
            {
                try { sara.Clear(); } catch { };
                string[] numbers = reader.ReadToEnd().Split(' ');
                foreach (string i in numbers)
                {
                    if(i != "/pabaiga")
                    {
                        try
                        {
                            sara.Add(int.Parse(i));
                        }catch
                        {
                        }
                    }
                }
            }
            /// <summary>
            /// Skaitomi skaiciai
            /// </summary>
            using (StreamReader reader2 = new StreamReader("statistika.txt"))
            {
                string[] numbers = reader2.ReadToEnd().Split(' ');
                laimejau = int.Parse(numbers[1]);
                pralaimejau = int.Parse(numbers[3]);
                daugiau_maziau[0] = int.Parse(numbers[5]);
                daugiau_maziau[1] = int.Parse(numbers[7]);
                pinigai = int.Parse(numbers[9]);
            }
        }

        private void RodytiDuomenis()
        {
            foreach (int number in sara)
            {
                Console.Write(number.ToString());
            }

        }

        private void NulintiTagus(int sk)
        {
            if (sk == 1)                // pastatytas skacius
            {
                bet_num.Text = "";
                bet_num.Tag = "0";
            }
            else
            {                           // laimetas skaicius
                win_num.Text = "";
                win_num.Tag = "0";
            }
        }

        private void PranestiKlaida(int sk)
        {
            if (sk == 1)
            {
                bet_num.Text = "";
                bet_num.Tag = "Tiek pastatyti negali! :D";
            }
            else
            {
                win_num.Text = "";
                win_num.Tag = "Rinkis skaičių nuo 0 iki 35! :)";
            }
        }

        private void TraukiniuStatistika()
        {
            char paskut_sp = ' ';
            int trainas = 1;
            // Skaiciavimai
            for (int i = 0; i < 10; i++)
                traukiniai[i] = 0;
            foreach (int o in sara)
            {
                if (paskut_sp == Conv_spalva(o))
                {
                    trainas++;
                }
                else
                {
                    for(int i = 0; i < 9; i++)
                    {
                        if (trainas == i + 2)
                            traukiniai[i]++;
                    }
                    if (trainas > 10)
                        traukiniai[9]++;

                    /*if (trainas == 2)
                        traukiniai[0]++;
                    else if (trainas == 3)
                        traukiniai[1]++;
                    else if (trainas == 4)
                        traukiniai[2]++;
                    else if (trainas == 5)
                        traukiniai[3]++;*/

                    paskut_sp = Conv_spalva(o);
                    trainas = 1;
                }
            }
            // Isvedimas
            train_1.Text = traukiniai[0].ToString();
            train_2.Text = traukiniai[1].ToString();
            train_3.Text = traukiniai[2].ToString();
            train_4.Text = traukiniai[3].ToString();
            train_5.Text = traukiniai[4].ToString();
            train_6.Text = traukiniai[5].ToString();
            train_7.Text = traukiniai[6].ToString();
            train_8.Text = traukiniai[7].ToString();
            train_9.Text = traukiniai[8].ToString();
            train_10.Text = traukiniai[9].ToString();
            // kartojimo skaciavimas
            paskut_sp = ' ';
            int k = 0, n = 0, a;
            foreach (int o in sara)
            {
                if (paskut_sp == Conv_spalva(o))
                    k++;
                else
                {
                    paskut_sp = Conv_spalva(o);
                    n++;
                }
            }
            // isvedimas kartojimu
            a = k + n;
            try
            {
                train_kart.Text = (n * 100 / a).ToString() + "%";
                train_nesikart.Text = (k * 100 / a).ToString() + "%";
            }catch
            {
                train_kart.Text = "Klaida";
                train_nesikart.Text = "Klaida";
            }
            
        }

        private void duomenys_click(object sender, RoutedEventArgs e)
        {
            // Blogos ivesties tikrinimas, skaiciu paimimas
            int laimetas = 0, klaida = 0;
            char pazymetas;
            bool nulinti = true;
            int pri, zero = 0;

            try
            {
                klaida = 1;
                if (bet_num.Tag.ToString() == "0" && bet_num.Text == "")
                {
                    laimetas = 0;
                    nulinti = true;
                }
                else
                {
                    laimetas = int.Parse(bet_num.Text);
                    nulinti = true;
                }
                if (laimetas >= 0 && laimetas <= 1000000)
                    nulinti = true;
                else
                    pri = 1 / zero;
            }
            catch
            {
                PranestiKlaida(klaida);
                nulinti = false;
            }
            try
            {
                klaida = 2;
                laimetas = int.Parse(win_num.Text);
                
                if (laimetas < sk_nuo || laimetas > sk_iki)
                    pri = 1 / zero;

                if (nulinti != false)
                    nulinti = true;
            }
            catch
            {
                PranestiKlaida(klaida);
                nulinti = false;
            }
            if(nulinti)
            {
                bool ar_pazymeta = true;
                // pazymeto isrinkimas
                if (cb_black.IsChecked == true)
                    pazymetas = 'b';
                else if (cb_red.IsChecked == true)
                    pazymetas = 'r';
                else if (cb_green.IsChecked == true)
                    pazymetas = 'g';
                else
                {
                    label1.Foreground = Brushes.Red;
                    ar_pazymeta = false;
                }
                if (ar_pazymeta)
                {

                    // skaiciaus pridejimas
                    sara.Add(laimetas);
                    TraukiniuStatistika();
                    // radio button atfiksavimas
                    cb_red.IsChecked = false;
                    cb_green.IsChecked = false;
                    cb_black.IsChecked = false;
                    // atstatymai
                    label1.Foreground = Brushes.LightYellow;
                    NulintiTagus(1);
                    NulintiTagus(2);
                    AtnaujintiProcentus();
                    PaskutiniaiRutuliukai(mas);
                    AtnaujintiIstorija();
                    // pagrazinimai
                    confirm_panel.Height = 90;
                    confirm_panel.BorderBrush = Brushes.GreenYellow;
                    sekme.Visibility = Visibility.Visible;
                    _taimeris.Start();
                }
            }
        }
        
        private void Laikrodis(object sender, EventArgs e)
        {
            confirm_panel.Height = 60;
            confirm_panel.BorderBrush = Brushes.OrangeRed;
            sekme.Visibility = Visibility.Hidden;
            _taimeris.Stop();
        }

        private void AtnaujintiMygtukus()
        {
            int viso = daugiau_maziau[0] + daugiau_maziau[1];
            btn_1.Content = "DAUGIAU (" + (daugiau_maziau[0] * 100 / viso).ToString() + "%)";
            btn_2.Content = "MAŽIAU (" + (daugiau_maziau[1] * 100 / viso).ToString() + "%)";
        }
        
        private void btn_1_click(object sender, RoutedEventArgs e)
        {

            daugiau_maziau[0]++;
            AtnaujintiMygtukus();

        }

        private void btn_2_click(object sender, RoutedEventArgs e)
        {
            daugiau_maziau[1]++;
            AtnaujintiMygtukus();
        }


        private static char pasirinkta_spalva = '-';
        private void pradeti_sukima(object sender, RoutedEventArgs e)
        {
            if (pasirinkta_spalva != '-')
            {
                sp1.Foreground = Brushes.AliceBlue;
                sp2.Foreground = Brushes.AliceBlue;
                sp3.Foreground = Brushes.AliceBlue;

                Random r = new Random();
                int atsitiktinis = 0;
                char color;

                try
                {
                    suma_pastatyta = int.Parse(bet_amount.Text);
                    if (pinigai - suma_pastatyta > 0)
                    {
                        pinigai -= suma_pastatyta;
                        money.Content = pinigai.ToString();
                        atsitiktinis = r.Next(sk_nuo, sk_iki + 1);
                        color = Conv_spalva(atsitiktinis);
                        SpalvintiFona(krenta_border, color);
                        krenta.Text = atsitiktinis.ToString();
                        // beto laimejimas ar pralaimejimas

                    } else
                    {
                        MessageBox.Show("Neužtenka pinigų statymui! Bandyk dar kartą");
                        bet_amount.Text = null;
                    }
                }
                catch
                {
                    MessageBox.Show("Nepasirinkote statymo sumos! :)");
                }
                int laime = 0;
                if (pasirinkta_spalva == 'g')
                    laime = 14 * suma_pastatyta;
                else
                    laime = 2 * suma_pastatyta;
                MessageBox.Show(Conv_spalva(atsitiktinis).ToString() + ":" + pasirinkta_spalva.ToString());
                if (Conv_spalva(atsitiktinis) == pasirinkta_spalva)
                {
                    pinigai += laime;
                    bet_amount2.Text = "+" + laime.ToString();
                }else
                {
                    bet_amount2.Text = "-" + laime.ToString();
                }

                sara.Add(atsitiktinis);
                PaskutiniaiRutuliukai(mas);
                AtnaujintiIstorija();
                TraukiniuStatistika();
                
                pasirinkta_spalva = '-';
                money.Content = pinigai.ToString();
            } else
                MessageBox.Show("Taigi spalvos nepasirinkai");
        }

        private void save_failas_click(object sender, RoutedEventArgs e)
        {
            using (StreamReader reader3 = new StreamReader("save.txt"))
            {
                List<string> sara_help = new List<string>();           // sarasas skaiciams saugoti
                List<string> sara2 = new List<string>();           // sarasas skaiciams saugoti

                if (failas_taip.IsChecked == true)
                    try { sara.Clear(); } catch { };
                
                while(true)
                {
                    string line = reader3.ReadLine();
                    if (line == "/pabaiga")
                        break;
                    sara_help.Add(line);
                }

                for(int i = 0; i < sara_help.Count; i++)
                {
                    string[] abc = sara_help[i].Split('_');
                    sara2.Add(abc[1]);
                }

                try
                {
                    sara_help.Clear();
                }
                catch { }

                char symbol = sara2[0][0];
                foreach (string i in sara2)
                {
                    string[] abc2 = i.Split(symbol);
                    for(int j = 1; j <= 10; j++)
                    {
                        try
                        {
                            sara.Add(int.Parse(abc2[j]));
                        }
                        catch { }
                    }
                }
            }
            PaskutiniaiRutuliukai(mas);
            AtnaujintiIstorija();
            TraukiniuStatistika();
            MessageBox.Show("Lošimo numeriai sukelti", "Perspėjimas");
        }

        private void sp_btn1(object sender, RoutedEventArgs e)
        {
            sp1.Foreground = Brushes.Blue;
            sp2.Foreground = Brushes.AliceBlue;
            sp3.Foreground = Brushes.AliceBlue;
            pasirinkta_spalva = 'r';
        }

        private void sp_btn2(object sender, RoutedEventArgs e)
        {
            sp1.Foreground = Brushes.AliceBlue;
            sp2.Foreground = Brushes.Blue;
            sp3.Foreground = Brushes.AliceBlue;
            pasirinkta_spalva = 'g';
        }

        private void sp_btn3(object sender, RoutedEventArgs e)
        {
            sp1.Foreground = Brushes.AliceBlue;
            sp2.Foreground = Brushes.AliceBlue;
            sp3.Foreground = Brushes.Blue;
            pasirinkta_spalva = 'b';
        }
    }
}
