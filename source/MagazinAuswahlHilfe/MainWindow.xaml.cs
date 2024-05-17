#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable IDE0305 // Simplify collection initialization

using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace MagazinVorschläger
{
    /// <summary>
    /// Programm fürs Berechnen der 
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variablen
        private string druckOutputDes;
        private string druckOutputDes2;
        private string druckOutput;
        private string druckOutput2;
        private string druckInputDes;
        private string druckInput;

        private TextBox[] textBoxArray;
        private TextBox[] textBoxArrayFlach;

        private readonly List<List<string>> magazine = new();

        private List<List<string>> ausgabe = new();
        private List<string> ungeordnetAusgabe = new();

        private readonly string initialList = AppDomain.CurrentDomain.BaseDirectory + "Magazine.txt";
        private readonly string logFIle = AppDomain.CurrentDomain.BaseDirectory + "Log.txt";

        private readonly string[] textBoxNames = { "Box_Name", "Box_KA", "Box_KI", "Box_KH", "Box_WZ", "Box_WB", "Box_DA", "Box_RS", "Box_DDT","Box_FT", "Box_DD" };
        private readonly string[] textBoxNamesFlach = { "Box_Name", "Box_KA", "Box_KI", "Box_KH", "Box_WZ", "Box_WB", "Box_DA", "Box_RS", "Box_DDT", "Box_FT", "Box_DH","Box_DB"};

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            Visibility_Initial();
            File_ReadInitialData();
            TextBox_InitialFocus();
            Box_RS.Text = "1";
            Box_DA.Text = "1";
        }

        private void Visibility_Initial()
        {
            Button_Print.Visibility = Visibility.Collapsed;
            Button_Berechnen.Visibility = Visibility.Collapsed;
            View_Flach.Visibility = Visibility.Collapsed;
            View_Error.Visibility = Visibility.Collapsed;
        }

        private void TextBox_InitialFocus()
        {
            textBoxArray = new TextBox[textBoxNames.Length];
            textBoxArrayFlach = new TextBox[textBoxNamesFlach.Length];
            for (int i = 0; i < textBoxNames.Length; i++) textBoxArray[i] = FindName(textBoxNames[i]) as TextBox;
            for (int i = 0; i < textBoxNamesFlach.Length; i++) textBoxArrayFlach[i] = FindName(textBoxNamesFlach[i]) as TextBox;
            textBoxArray[0].Focus();
        }

        private void File_ReadInitialData()
        {
            if (File.Exists(initialList))
            {
                try
                {
                    string[] _allSplit = File.ReadAllText(initialList).Split("\n");

                    for (int i = 0; i < _allSplit.Length; i++)
                    {
                        string _currentLine = _allSplit[i];
                        if (IsEmpty(_currentLine)) continue;

                        List<string> _currentLineSplit = _currentLine.Split(';').ToList();
                        magazine.Add(_currentLineSplit);
                    }
                }
                catch
                {
                    DisplayError("Beim lesen der Datei Magazine.txt gab es einen Fehler. Stellen Sie sicher, dass die Datei nicht von einem andrem Programm geöffnet ist.", false);
                }
            }
            else
            {
                DisplayError("Achtung: Es scheint so als würde die Datei Magazine.txt fehlen, welche die Daten der Magazine enthält." +
                             "\nBitte schauen sie sich die Datei Anleitung.pdf an, um zu lernen wie man diese erstellt und formatiert." +
                             "\nWenn die Anleitung fehlt, versuchen Sie den Ersteller des Programms zu kontaktieren unter der Email: ibrahimoezhan@gmx.de", false);
            }
        }

        private void MagazinBerechnen()
        {
            ausgabe.Clear();
            while (ausgabe.Count < 4) ausgabe.Add(new());

            #region Variablen
            double _kernInnen = double.Parse(Box_KI.Text);
            double _kernAussen = double.Parse(Box_KA.Text);

            if (_kernInnen >= _kernAussen)
            {
                DisplayError("Kern-Innen muss kleiner sein als Kern-Aussen");
                return;
            }

            double _kernHöhe = double.Parse(Box_KH.Text);
            double _drahtBreite = double.Parse(IsFlachdraht() ? Box_DB.Text : Box_DD.Text);
            double _drahtHöhe = double.Parse(IsFlachdraht() ? Box_DH.Text : Box_DD.Text);
            double _drahtDurchmesser = DrahtDurchmesser(_drahtBreite, _drahtHöhe);
            double _windungen = double.Parse(Box_WZ.Text);
            double _wickelBereich = double.Parse(Box_WB.Text);
            double _anzahlDrähte = AnzahlDrähte();

            int _lage = 1;
            double _drahtLänge = 0;
            double _windungLänge = 0;
            double _windungenÜbrig = _windungen;
            double _kernQuerUmfang = (_kernAussen - _kernInnen) + (_kernHöhe * 2);

            string log = "";
            #endregion

            #region Input Display
            druckInputDes = "Input: \n\n";
            druckInputDes += "Kern Innendurchmesser:\n";
            druckInputDes += "Kern Aussendurchmesser:\n";
            druckInputDes += "Kern Höhe:\n";
            if (IsFlachdraht())
            {
                druckInputDes += "Draht Höhe:\n";
                druckInputDes += "Draht Breite:\n";
            }
            else druckInputDes += "Drahtdurchmesser:\n";

            druckInputDes += "Windungen:\n";
            druckInputDes += "Wickelbereich:\n";
            druckInputDes += "Drähte Anzahl:\n";

            druckInput = "\n\n";
            druckInput += _kernInnen.ToString("0.0") + " mm\n";
            druckInput += _kernAussen.ToString("0.0") + " mm\n";
            druckInput += _kernHöhe.ToString("0.0") + " mm\n";

            if (IsFlachdraht())
            {
                druckInput += _drahtHöhe + " mm\n";
                druckInput += _drahtBreite + " mm\n";
            }
            else druckInput += _drahtHöhe + " mm\n";
            druckInput += _windungen + "\n";
            druckInput += _wickelBereich + "°\n";
            druckInput += _anzahlDrähte + "\n";

            #endregion

            while (_windungenÜbrig > 0)
            {
                double _wickelUmfang = (_kernInnen - _drahtHöhe - (2 * _drahtHöhe * (_lage - 1))) * MathF.PI * (_wickelBereich / 360);
                if (_wickelUmfang <= 5)
                {
                    DisplayError("Zu Viele Windungen. Restloch zu klein");
                    return;
                }

                switch (_lage)
                {
                    case 1:
                        _windungLänge = _kernQuerUmfang + (4 * _drahtHöhe);
                        break;
                    case 2:
                        _windungLänge = _kernQuerUmfang + (4 * _drahtHöhe) + (8 * _drahtHöhe);
                        break;
                    default:
                        _windungLänge += (4 * _drahtHöhe);
                        break;
                }

                double _windungsZahl = (_wickelUmfang / _drahtBreite) / _anzahlDrähte;
                _windungsZahl = MathF.Floor((float)_windungsZahl);
                _windungsZahl = Math.Clamp(_windungsZahl, 0, _windungenÜbrig);

                _drahtLänge += _windungLänge * _windungsZahl / 1000;
                _windungenÜbrig -= _windungsZahl;

                log += "Lage: " + _lage + "\n";
                log += "Windungs Länge: " + _windungLänge.ToString() + "\n";
                log += "Windungs Zahl: " + _windungsZahl.ToString() + "\n";
                log += "Windungen Übrig: " + _windungenÜbrig.ToString() + "\n";

                if (_windungenÜbrig > 0) _lage++;
            }

            if (!File.Exists(logFIle)) File.Create(logFIle).Close();
            File.WriteAllText(logFIle, log);

            double _bewickelteRingBreite = (_kernAussen - _kernInnen) / 2 + 2 * _drahtHöhe * _lage;
            double _körperHöhe = _kernHöhe + _drahtHöhe * _lage * 2;
            double _restLoch = _kernInnen - (_drahtHöhe * _lage * 2);
            double _drahtLängeGesamt = _drahtLänge * _anzahlDrähte;
            double _füllraum = Füllraum(_drahtLängeGesamt, _drahtBreite, _drahtHöhe, _windungen);

            #region Magazin Berechnung
            Debug.Print(ausgabe.Count.ToString());
            ungeordnetAusgabe.Clear();

            int angaben = 1;
            for (int i = 1; i < magazine.Count; i++)
            {
                if (IsEmpty(magazine[i][0]))
                {
                    angaben = i;
                    continue;
                }

                if (double.TryParse(magazine[i][1], out double val) && _füllraum - float.Parse(Box_FT.Text) > val) continue;

                string magazinName = magazine[i][0].Trim();
                Debug.Print(magazinName);

                for (int restloch = 2; restloch < 26; restloch++)
                {
                    if (float.TryParse(magazine[angaben][restloch], out float _KörperHöheMax) && _KörperHöheMax >= _körperHöhe)
                    {
                        Debug.Print("KH");
                        if (float.TryParse(magazine[i][restloch], out float _restlochBraucht))
                        {

                            if (_restLoch >= _restlochBraucht + double.Parse(Box_RS.Text))
                            {
                                Debug.Print("RL");
                                float min = 0, max = 0;
                                for (int dd = 26; dd < magazine[i].Count; dd++)
                                {
                                    if (double.TryParse(magazine[i][dd], out _))
                                    {
                                        min = float.Parse(magazine[angaben][dd]);
                                        break;
                                    }
                                }

                                for (int dd = magazine[i].Count - 1; dd > 25; dd--)
                                {
                                    if (double.TryParse(magazine[i][dd], out _))
                                    {
                                        max = float.Parse(magazine[angaben][dd]);
                                        break;
                                    }
                                }

                                Debug.Print("Min: " + min + " Max: " + max + "DD: " + _drahtDurchmesser);
                                if ((float)_drahtDurchmesser < min - float.Parse(Box_DDT.Text) || (float)_drahtDurchmesser > max + float.Parse(Box_DDT.Text)) break;
                                Debug.Print("DD");
                                ungeordnetAusgabe.Add(magazinName);


                            }
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < ungeordnetAusgabe.Count; i++)
            {
                string magazinName = ungeordnetAusgabe[i];
                for (int f = 0; f < magazinName.Length; f++)
                {
                    if (int.TryParse(magazinName[f].ToString(), out int fn))
                    {
                        for (int db = 0; db < ausgabe.Count; db++)
                        {
                            switch (db)
                            {
                                case 0:
                                    switch (fn)
                                    {
                                        case 1:
                                        case 2:
                                            ausgabe[db].Add(magazinName);
                                            break;
                                    }
                                    break;
                                case 1:
                                    switch (fn)
                                    {
                                        case 2:
                                        case 3:
                                            ausgabe[db].Add(magazinName);
                                            break;
                                    }
                                    break;
                                case 2:
                                    switch (fn)
                                    {
                                        case 3:
                                        case 4:
                                            ausgabe[db].Add(magazinName);
                                            break;
                                    }
                                    break;
                                case 3:
                                    switch (fn)
                                    {
                                        case 4:
                                        case 5:
                                        case 6:
                                            ausgabe[db].Add(magazinName);
                                            break;
                                    }
                                    break;
                            }
                        }

                        break;
                    }
                }
            }
            #endregion

            #region OutputDisplay
            druckOutputDes = "Output: \n\n";
            druckOutputDes += "Lagen:\n";
            druckOutputDes += "Kernqueerschnittsumfang:\n";
            druckOutputDes += "Kernhöhe:\n";

            druckOutputDes2 = "\n\n\n\n\n\nRestloch:\n";
            druckOutputDes2 += "Drahtlänge:\n";
            druckOutputDes2 += "Füllraum:\n";

            druckOutput = "\n\n";
            druckOutput += _lage.ToString() + "\n";
            druckOutput += _kernQuerUmfang.ToString("0.0") + " mm\n";
            druckOutput += _körperHöhe.ToString("0.0") + " mm\n";

            druckOutput2 = "\n\n\n\n\n\n" + _restLoch.ToString("0.0") + " mm\n";
            druckOutput2 += _anzahlDrähte + " x " + _drahtLänge.ToString("0.00") + " m\n";
            druckOutput2 += _füllraum.ToString("0.00") + " cm³\n";
            #endregion

            #region Other
            for (int i = 0; i < ausgabe.Count; i++)
            {
                ausgabe[i] = ausgabe[i].OrderByDescending(s => s.Substring(0, Math.Min(2, s.Length))).ToList();
                File.AppendAllLines(logFIle, ausgabe[i].ToArray());
            }

            Button_Print.Visibility = Visibility.Visible;
            #endregion

            #region Vorschau
            FixedPage fixedPage = new();

            TextBlock _textBlockName = new()
            {
                Text = Box_Name.Text,
                FontSize = 18,
                Margin = new Thickness(20, 20, 0, 0)
            };
            fixedPage.Children.Add(_textBlockName);

            TextBlock _textBoxDatum = new()
            {
                Text = DateTime.Now.ToString("dd/MM/yyyy"),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(675, 30, 0, 0)
            };
            fixedPage.Children.Add(_textBoxDatum);

            TextBlock _textBlockInDes = new()
            {
                Text = druckInputDes,
                FontSize = 14,
                Margin = new Thickness(20, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockInDes);

            TextBlock _textBlockIn = new()
            {
                Text = druckInput,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(250, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockIn);

            TextBlock _textBlockOutDes = new()
            {
                Text = druckOutputDes,
                FontSize = 14,
                Margin = new Thickness(400, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockOutDes);

            TextBlock _textBlockOut = new()
            {
                Text = druckOutput,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(630, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockOut);

            TextBlock _textBlockOutDes2 = new()
            {
                Text = druckOutputDes2,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(400, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockOutDes2);

            TextBlock _textBlockOut2 = new()
            {
                Text = druckOutput2,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(630, 75, 0, 0)
            };
            fixedPage.Children.Add(_textBlockOut2);

            string content = "";

            for (int db = 0; db < ausgabe.Count; db++)
            {
                if (db > 0) content += "\n-----------------------------------------------------------------------------------------------------------------------------\n";
                content += "DB " + (db + 1) + "\n";
                if (ausgabe[db].Count == 0) content += "Keine passende Standard-Ausführung im Programm";
                else
                {
                    string sub = "WZ";
                    int lineCount = 0;
                    for (int i = 0; i < ausgabe[db].Count; i++)
                    {
                        lineCount++;
                        if (!ausgabe[db][i].Contains(sub))
                        {
                            content += (lineCount == 1 ? "\n" : "\n\n") + ausgabe[db][i] + ",    ";
                            sub = ausgabe[db][i].Substring(0, 2);
                            lineCount = 1;
                            continue;
                        }
                        else if (lineCount % 10 == 0)
                        {
                            content += ausgabe[db][i] + ",   \n";
                            sub = ausgabe[db][i].Substring(0, 2);
                            lineCount = 0;
                            continue;
                        }
                        content += ausgabe[db][i] + ",   ";
                    }
                }
            }

            TextBlock _textBlockMagazine = new()
            {
                Text = content,
                FontSize = 14,
                Margin = new Thickness(20, 275, 0, 0)
            };
            fixedPage.Children.Add(_textBlockMagazine);

            PageContent pageContent = new();
            ((IAddChild)pageContent).AddChild(fixedPage);

            FixedDocument fixedDoc = new();
            fixedDoc.Pages.Add(pageContent);

            Preview.Document = fixedDoc;
            Preview.FitToHeight();
            #endregion
        }

        private double DrahtDurchmesser(double _breite, double _höhe)
        {
            if (IsFlachdraht())
            {
                double rechteckFläsche = _breite * _höhe;
                return MathF.Sqrt((float)rechteckFläsche / MathF.PI) * 2;
            }
            else return _breite;
        }

        private static double Füllraum(double _drahtLänge, double _drahtBreite, double _drahtHöhe, double _windungen)
        {
            double _füllraum = (_drahtLänge * 100) * (_drahtHöhe / 10) * (_drahtBreite / 10);
            _füllraum += (_füllraum * (10 / _windungen));
            return _füllraum;
        }

        private float AnzahlDrähte()
        {
            return float.Parse(Box_DA.Text);
        }

        private bool IsFlachdraht()
        {
            return Check_Flachdraht.IsChecked != false;
        }

        private void DisplayError(string _message, bool allowOK = true)
        {
            Label_Error.Content = _message;
            Button_Error.Visibility = allowOK ? Visibility.Visible : Visibility.Collapsed;
            View_Error.Visibility = Visibility.Visible;
            View_Main.Visibility = Visibility.Collapsed;
            Button_Error.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            TextBox[] _textBoxArray = IsFlachdraht() ? textBoxArrayFlach : textBoxArray;
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Up:
                case Key.PageUp:
                    Button_MoveFocus(_textBoxArray.Length - 1, 1, 0, _textBoxArray);
                    break;
                case Key.Down:
                case Key.PageDown:
                    Button_MoveFocus(0, -1, _textBoxArray.Length - 1, _textBoxArray);
                    break;
            }
        }

        private void TextBox_InvalidValue(TextBox box)
        {
            //Nicht fortfahren wenn TextBox gleich der Namensbox da diese jeden Wert haben darf
            if (box == Box_Name) return;

            if (!float.TryParse(box.Text, out float value) || value > 100000 || value < 0)
            {
                box.Text = "0";
                if (box == Box_RS) box.Text = "1";
            }
            if (box == Box_WZ && value < 21)
            {
                box.Text = "21";
                DisplayError("Windungszahl zu niedrig. Einsatz der Maschiene wird unwirtschaftlich.");
            }
            if (box == Box_DA && (value < 1 || box.Text.Contains(',')))
            {
                box.Text = "1";
                DisplayError("Draht Anzahl kann 1 nicht unterschreiten.");
            }
            if (box == Box_WB && (value < 1 || value > 360))
            {
                box.Text = "360";
                DisplayError("Wickelbereich kann nur zwischen 1-360 liegen.");
            }
        }

        private static bool IsEmpty(string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) return true;
            return false;
        }

        #region UI
        private void Button_MagazinBerechnen(object sender, RoutedEventArgs e)
        {
            try
            {
                MagazinBerechnen();
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private void Button_PrintFile(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog pDialog = new()
                {
                    PageRangeSelection = PageRangeSelection.AllPages,
                    UserPageRangeEnabled = true
                };

                bool? print = pDialog.ShowDialog();
                if (print == true) pDialog.PrintDocument(Preview.Document.DocumentPaginator, "Print");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
            }
        }

        private void Button_ErrorOK(object sender, RoutedEventArgs e)
        {
            View_Error.Visibility = Visibility.Collapsed;
            View_Main.Visibility = Visibility.Visible;
            TextBox_InitialFocus();
        }

        private void Button_MoveFocus(int limit, int dir, int loopVal, TextBox[] _textBoxArray)
        {
            for (int i = 0; i < _textBoxArray.Length; i++)
            {
                if (_textBoxArray[i].IsFocused)
                {
                    try
                    {
                        TextBox_InvalidValue(_textBoxArray[i]);
                        Keyboard.ClearFocus();
                        int toFocus = (i != limit ? i + dir : loopVal);
                        _textBoxArray[toFocus].Focus();
                        break;
                    }
                    catch (Exception e)
                    {
                        _textBoxArray[0].Focus();
                        Debug.Print(e.Message);
                    }
                }
            }
        }

        private void Button_Condition()
        {
            TextBox[] _textBoxArray = IsFlachdraht() ? textBoxArrayFlach : textBoxArray;
            Button_Berechnen.IsEnabled = true;
            Button_Berechnen.Visibility = Visibility.Visible;
            for (int i = 1; i < _textBoxArray.Length; i++)
            {
                string textToCheck = _textBoxArray[i].Text;
                if (!float.TryParse(textToCheck, out _) || IsEmpty(textToCheck))
                {
                    Button_Berechnen.IsEnabled = false;
                    Button_Berechnen.Visibility = Visibility.Hidden;
                }
            }
        }

        private void TextBox_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            if (!float.TryParse(e.Text, out _) && (e.Text != ",")) e.Handled = true;
            Button_Condition();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Button_Condition();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Button_Condition();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Button_Condition();
            TextBox_InvalidValue(sender as TextBox);
        }
        private void CheckBox_Flachdraht(object sender, RoutedEventArgs e)
        {
            bool isFlach = Check_Flachdraht.IsChecked == true;
            View_Flach.Visibility = isFlach ? Visibility.Visible : Visibility.Collapsed;
            View_Rund.Visibility = !isFlach ? Visibility.Visible : Visibility.Collapsed;
            Button_Condition();
        }
        #endregion
    }
}

///
///Alte version der Formel
///
//float _windungLänge = _kernQueerschnittsUmfang + (8 * _drahtHöhe * (_lagen - 1) + (4 * _drahtHöhe));
//float _windungLänge = _kernQueerschnittsUmfang + (_drahtHöhe * (_lagen > 2 ? 4 : 8) * (_lagen - 1) + (4 * _drahtHöhe));
//float _windungLänge = _kernQueerschnittsUmfang + (8 * _drahtHöhe * (_lagen - 1) + (4 * _drahtHöhe)) - (_lagen > 2 ? 8 : 0);
//float _windungLänge = _kernQueerschnittsUmfang + (4 * _drahtHöhe) + (8 * _drahtHöhe * (_lage - 1));

#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore IDE0028 // Simplify collection initialization
#pragma warning restore IDE0305 // Simplify collection initialization
#pragma warning restore IDE0300 // Simplify collection initialization