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
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Steganografia
{
    public partial class MainWindow : Window
    {
        private static Random random = new Random();
        private String helpText = "Autor: Damian Szkudlarek\n\n1. Algorytm najmniej znaczącego bitu\nWiadomość, którą chcemy umieścić na obrazie, konwertujemy na postać binarną.\nKażdy z bitów wiadomości umieszczamy na najmniej znaczącym bicie kanałów każdego piksela.\nPrzykładowo, piksele obrazu bitmapowego reprezentowane są przez 4 kanały: Red Green Blue Alpha.\nOznacza to, że do zakodowania 8bitowego kodu ASCII użyć należy dwóch pikseli obrazu.\n\n2. Dane wejściowe\nWprowadzone z klawiatury, wklejone lub wczytane z pliku tekstowego.\n\n3. Rozmiar danych\nWczytany obraz jest skalowany, aby można go było wyświetlić w oknie programu. Skalowanie nie wpływa na rozmiar obrazu wyjściowego.\n\n4. Format danych\nProgram obsługuje formaty obrazów .bmp, .png i .jpg. Pliki z wiadomością mogą mieć rozszerzenie .txt\n\n5. Ograniczenia\nWprowadzona wiadomość jest filtrowana przez wyrażenie regularne i dopuszcza wszystkie litery alfabetu angielskiego,\ncyfry, znaki białe i wszystkie znaki specjalne możliwe do znalezienia na klawiaturze, poza tyldą(~) - tylda używana jest jako wartownik. \n\n6. Umieszczenie wiadomości\nWiadomość umieszczana jest od lewego górnego piksela w naturalnym kierunku, czyli w prawo, przy końcu ekranu, zaczynamy \nod lewego piksela w wierszu poniżej.\n\n7. Zabezpiecznie przed umieszczeniem zbyt dużej wiadomości\nW zależności od rozdzielczości obrazu i ilości kanałów możemy wprowadzić ograniczoną wiadomość.\nJeśli mamy obraz PNG o rozdzielczości 1920x1080 to możemy umieścić na obrazie 1 036 800=(1920*1080*4/8) znaków 8bitowych.\n\n8. Środowisko programistyczne \nAplikacja WPF wykonana w środowisku Visual Studio 2017 w języku C# z wykorzystaniem elementów XAML";
        private BitmapImage image = new BitmapImage();
        private WriteableBitmap imageC;
        private int howLongMessage = 0;

        public MainWindow()
        {
            InitializeComponent();
            help.Text = helpText;
        }
        #region Losowanie
        public static List<int> GenerateRandom(int count, int min, int max)
        {
            if (max <= min || count < 0 ||

                    (count > max - min && max - min > 0))
            {

                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                        " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }


            HashSet<int> candidates = new HashSet<int>();

            for (int top = max - count; top < max; top++)
            {
                if (!candidates.Add(random.Next(min, top + 1)))
                {
                    candidates.Add(top);
                }
            }


            List<int> result = candidates.ToList();
            for (int i = result.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                int tmp = result[k];
                result[k] = result[i];
                result[i] = tmp;
            }
            return result;
        }
        public static bool GenerateRandom()
        {
            int count = 1, min = 0, max = 2;
            if (max <= min || count < 0 ||

                    (count > max - min && max - min > 0))
            {

                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                        " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }


            HashSet<int> candidates = new HashSet<int>();

            for (int top = max - count; top < max; top++)
            {
                if (!candidates.Add(random.Next(min, top + 1)))
                {
                    candidates.Add(top);
                }
            }


            List<int> result = candidates.ToList();
            for (int i = result.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                int tmp = result[k];
                result[k] = result[i];
                result[i] = tmp;
            }
            if (result[0] == 1)
                return true;
            else
                return false;
        }
        #endregion

        #region Okienka informacyjne
        private void showAlert(String text)
        {
            string caption = "Uwaga";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(text, caption, button, icon);
        }
        #endregion

        private void loadFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".bmp",
                Filter = "Pliki bitmapy (.bmp)|*.bmp|Pliki PNG (.png)|*.png|Pliki JPEG (.jpg)|*.jpg"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                BitmapImage b = new BitmapImage();
                b.BeginInit();
                b.UriSource = new Uri(dlg.FileName);
                b.EndInit();

                byte[] pixels = new byte[
                 b.PixelHeight * b.PixelWidth *
                 b.Format.BitsPerPixel / 8];
                b.CopyPixels(pixels, b.PixelWidth * 4, 0);
                fileInfo.Text = "";
                var splitted = dlg.FileName.Split('\\');
                fileInfo.Text += "Ścieżka: " +splitted[splitted.Length-1]+Environment.NewLine;
                fileInfo.Text += "Wymiary: " + b.PixelWidth.ToString() + "x" +b.PixelHeight.ToString() +" - "+ b.Format.BitsPerPixel +"b"+Environment.NewLine;
                fileInfo.Text += "Pierwsze piksele: " + Environment.NewLine;
                for (int i = 0,j=0; i < 32; i+=4,j++)
                {
                    fileInfo.Text += j.ToString()+".[R(" + pixels[i] + "), G(" + pixels[i+1] + "), B(" + pixels[i+2] + "), A(" + pixels[i+3] + ")]" + Environment.NewLine;
                }

              
                howLongMessage = (b.PixelHeight * b.PixelWidth * b.Format.BitsPerPixel / 8)/8 - 1;
                showAlert("Możesz wprowadzić wiadomość o długości: " + howLongMessage.ToString() + " B");

                imageNormal.Source = b;
                image = b;
            }
            else
                showAlert("Nie załadowano obrazu.");
        }

        private bool checkInputMessage(string[] lines)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\\"" !@#$%^&*()_\-\+=\}\{\:\;\'\[\]\,\.\\\/\|<>]*$");
            foreach (var line in lines)
            {
                if (!_regex.IsMatch(line))
                {
                    return false;
                }
            }
            return true;
        }

        private void loadTextToBeCiphered_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\\"" !@#$%^&*()_\-\+=\}\{\:\'\[\]\,\.\\\/\|<>]*$");

                var lines = File.ReadAllLines(dlg.FileName, Encoding.GetEncoding("Windows-1250"));
                var match = checkInputMessage(lines);
                String fileText = "";
                if (match)
                {
                    foreach (var line in lines)
                    {
                        if (line != lines.Last())
                            fileText += line + Environment.NewLine;
                        else
                            fileText += line;

                    }
                    loadedTextToBeCiphered.Text = fileText;
                }
                else
                {
                    showAlert("Niedozowolna treść pliku!");
                }

            }
        }
        private bool[] ToBoolArray(string str)
        {
            bool[] ret = new bool[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '1')
                    ret[i] = true;
                else ret[i] = false;
            }
            return ret;
        }
        private bool[] GetBinaryStringFromString(String data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return ToBoolArray(sb.ToString());
        }

        private void cipherButton_Click(object sender, RoutedEventArgs e)
        {
            if (image.Width < 1)
            {
                showAlert("Nie wprowadzono zdjęcia.");
                return;
            }
            if (loadedTextToBeCiphered.Text.Length > howLongMessage)
            {
                showAlert("Za długa wiadomość.");
                return;
            }
            if (loadedTextToBeCiphered.Text.Length < 1)
            {
                showAlert("Nie wprowadzono wiadomości.");
                return;
            }
            var lines = loadedTextToBeCiphered.Text.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
            if (!checkInputMessage(lines))
            {
                showAlert("Niepoprawna treść wiadomości");
                return;
            }
            WriteableBitmap bitmap = new WriteableBitmap(image);

            String mess = loadedTextToBeCiphered.Text+'~';
            bool[] messB = GetBinaryStringFromString(mess);

            byte[] pixels = new byte[
                 bitmap.PixelHeight * bitmap.PixelWidth *
                 bitmap.Format.BitsPerPixel / 8];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth*4, 0);
            for(int i = 0; i< pixels.Count(); i++)
            {
                //zakończ jeśli nie ma więcej wiadomości
                if (i == messB.Count())
                    break;
                if (pixels[i] % 2 == 0 && messB[i] == true)
                    pixels[i]++;
                else if (pixels[i] % 2 != 0 && messB[i] != true)
                    pixels[i]--;

            }

            bitmap.WritePixels(
              new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight),
              pixels,
              bitmap.PixelWidth * bitmap.Format.BitsPerPixel / 8, 0
            );
            fileCInfo.Text = "";
            fileCInfo.Text += "Wymiary: " + bitmap.PixelWidth.ToString() + "x" + bitmap.PixelHeight.ToString() + " - " + bitmap.Format.BitsPerPixel + "b" + Environment.NewLine;
            fileCInfo.Text += "Pierwsze piksele: " + Environment.NewLine;
            for (int i = 0, j = 0; i < 32; i += 4, j++)
            {
                fileCInfo.Text += j.ToString() + ".[R(" + pixels[i] + "), G(" + pixels[i + 1] + "), B(" + pixels[i + 2] + "), A(" + pixels[i + 3] + ")]" + Environment.NewLine;
            }

            imageCiphered.Source = bitmap;
            imageC = bitmap;
        }

        private byte binToDec(ref bool[] bin)
        {
            byte dec = 0;
            if (bin.Count() < 9)
            {
                if (bin[0] == true) dec += 128;
                if (bin[1] == true) dec += 64;
                if (bin[2] == true) dec += 32;
                if (bin[3] == true) dec += 16;
                if (bin[4] == true) dec += 8;
                if (bin[5] == true) dec += 4;
                if (bin[6] == true) dec += 2;
                if (bin[7] == true) dec += 1;
            }
            return dec;
        }
           
        private bool areEqual(bool[] a, bool[] b)
        {
            if (a.Count() != b.Count())
                return false;
            for (int i = 0; i < a.Count(); i++)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private void decipherButton_Click(object sender, RoutedEventArgs e)
        {
            WriteableBitmap bitmap = new WriteableBitmap(image);
            

            String tmp = "";

            byte[] pixels = new byte[
                 bitmap.PixelHeight * bitmap.PixelWidth *
                 bitmap.Format.BitsPerPixel / 8];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
            bool[] previous = new bool[8];
            var stopWord = GetBinaryStringFromString("~");

            for (int i = 0; i < pixels.Count(); i += 8)
            {
                //zakończ jeśli nie ma więcej wiadomości
                if (areEqual(previous,stopWord))
                    break;
                for (int j = 0; j < 8; j++)
                {
                    previous[j] = Convert.ToBoolean(pixels[i+j] % 2);
                }
                tmp += Encoding.ASCII.GetString(new byte[1] { binToDec(ref previous) });

            }

            message.Text = tmp.Substring(0,tmp.Length-1);
        }

        private void saveCipheredImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                DefaultExt = ".bmp",
                Filter = "Pliki bitmapy (.bmp)|*.bmp|Pliki PNG (.png)|*.png|Pliki JPEG (.jpg)|*.jpg"
            };

            if (dialog.ShowDialog() == true)
            {
                var uri = image.UriSource.Segments;
                var ext = (uri[uri.Length-1].Substring(uri[uri.Length-1].Length-4));
                BitmapEncoder encoder = new BmpBitmapEncoder();
                switch (ext) {
                    case ".bmp": encoder = new BmpBitmapEncoder();
                        break;
                    case ".png": encoder = new PngBitmapEncoder();
                        break;
                    case "jpeg": encoder = new PngBitmapEncoder();
                        break;
                }
                encoder.Frames.Add(BitmapFrame.Create(imageC));
                using (var fileStream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

            }
        }

        private void saveMessage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {  
               File.WriteAllText(dlg.FileName, message.Text, Encoding.GetEncoding("Windows-1250"));
            }
            else
                showAlert("Błąd przy zapisie do pliku.");
        }


    }
}