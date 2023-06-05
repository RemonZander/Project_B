using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Globalization;

namespace Project_B_V2._0
{
    internal class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static readonly IntPtr _thisCon = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // State of the application once loaded
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;


        private static int _currentScreen;
        private static readonly List<Screen> _screens = new List<Screen>();

        private static readonly DualConsoleOutput _dualOutput = new DualConsoleOutput(@"output.txt", Console.Out);

        static void Main(string[] args)
        {
            Console.SetOut(_dualOutput);
            ShowWindow(_thisCon, MAXIMIZE);

            if (args.Length > 0)
            {
                DateTime newSetDate = Convert.ToDateTime(args[0]);

                _screens.Add(new HomeScreen(newSetDate)); // 0
                _screens.Add(new TestDataGeneratorScreen(newSetDate)); // 1
                _screens.Add(new AfdelingshoofdScherm(newSetDate)); //2
                _screens.Add(new InlogGidsScherm(newSetDate)); //3
                _screens.Add(new GidsScherm(newSetDate)); //4
                _screens.Add(new WeekSchema(newSetDate)); //5
            }
            else
            {
                _screens.Add(new HomeScreen(DateTime.Now)); // 0
                _screens.Add(new TestDataGeneratorScreen(DateTime.Now)); // 1
                _screens.Add(new AfdelingshoofdScherm(DateTime.Now)); //2
                _screens.Add(new InlogGidsScherm(DateTime.Now)); //3
                _screens.Add(new GidsScherm(DateTime.Now)); //4
                _screens.Add(new WeekSchema(DateTime.Now)); //5
            }

            _currentScreen = 0;
            do
            {
                Display();
                if (!Console.IsInputRedirected) Refresh();
            } while (_currentScreen != -1);
        }

        static internal void Display()
        {
            _currentScreen = _screens[_currentScreen].DoWork(_dualOutput);
        }

        static internal void Refresh()
        {
            Console.Clear();
        }
    }

    internal abstract partial class Screen
    {
        protected const string ESCAPE_KEY = "ESCAPE";
        protected const string ENTER_KEY = "ENTER";
        protected const string BACKSPACE_KEY = "BACKSPACE";
        protected const string UP_ARROW = "UPARROW";
        protected const string DOWN_ARROW = "DOWNARROW";
        protected const string LEFT_ARROW = "LEFTARROW";
        protected const string RIGHT_ARROW = "RIGHTARROW";
        protected const string DATE_FORMAT = "d-M-yyyy";
        protected const string TIME_FORMAT = "HH:mm";
        protected const string DATE_TIME_FORMAT = "d-M-yyyy HH:mm";
        protected readonly DateTime newSetDate;

        public Screen(DateTime newSetDate)
        {
            this.newSetDate = newSetDate;
        }

        /// <summary>
        /// This is the main function of the current screen. Here is all the logic of that current screen
        /// </summary>
        /// <returns>This function returns the ID of the next screen to display</returns>
        internal abstract int DoWork(DualConsoleOutput dualOutput);

        protected static ConsoleKeyInfo ReadKey()
        {
            if (Console.IsInputRedirected)
            {
                string input = Console.ReadLine();
                if (input == "EXIT") Environment.Exit(0);
                if (input == "DOWNARROW") return new ConsoleKeyInfo('a' ,(ConsoleKey)40, false, false, false);
                if (input == "UPARROW") return new ConsoleKeyInfo('a', (ConsoleKey)38, false, false, false);
                if (input == "RIGHTARROW") return new ConsoleKeyInfo('a', (ConsoleKey)39, false, false, false);
                if (input == "LEFTARROW") return new ConsoleKeyInfo('a', (ConsoleKey)37, false, false, false);
                ConsoleKey ck = (ConsoleKey)input[0];
                return new ConsoleKeyInfo(Convert.ToChar(ck), ck, false, false, false);
            }
            return Console.ReadKey();
        }

        protected static string? ReadLine()
        {
            string input = Console.ReadLine();
            if (input == "EXIT") Environment.Exit(0);
            Console.Write(input);
            return input;
        }

        /// <summary>
        /// Returns true if the key with the specified keycode is pressed.
        /// </summary>
        /// <param name="key">This is the name of the key, use one of the key constants described in the BaseScreen.</param>
        /// <returns>True if the right key is pressed, false is not</returns>
        protected static bool IsKeyPressed(ConsoleKeyInfo cki, string key) => cki.Key.ToString().ToUpper() == key.ToUpper();

        protected static (string?, int) AskForInput(int screenIndex)
        {
            if (Console.IsInputRedirected)
            {
                return (ReadLine(), -1);
            }

            bool AskRepeat = true;
            List<char> output = new();

            while (AskRepeat)
            {
                ConsoleKeyInfo CKInfo = ReadKey();

                if (CKInfo.KeyChar == '\0') continue;

                if (IsKeyPressed(CKInfo, ENTER_KEY)) break;

                if (IsKeyPressed(CKInfo, ESCAPE_KEY)) return ("", screenIndex);

                if (IsKeyPressed(CKInfo, BACKSPACE_KEY))
                {
                    (int, int) curserPos = Console.GetCursorPosition();
                    if (curserPos.Item1 > 0)
                    {
                        Console.SetCursorPosition(curserPos.Item1 - 1, curserPos.Item2);
                        Console.Write(" ");
                        output.RemoveAt(output.Count - 1);
                    }
                }
                else
                {
                    output.Add(CKInfo.KeyChar);
                }

                if (CKInfo.KeyChar != '\0')
                {
                    Console.Write(CKInfo.KeyChar.ToString());
                }
            }

            // -1 means no interruptions has been found while asking for input
            return (string.Join(null, output), -1);
        }

        /// <summary>
        /// You use this function if you want to make 1 box
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">This is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a string with is a box of sym around it</returns>
        protected static string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        /// <summary>
        /// you use this function if you want to make a list of boxes
        /// </summary>
        /// <param name="blocks">This is a list of list string where this is a list of list of lines</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <returns>This function returns a list of boxes</returns>
        protected static List<string> BoxAroundText(List<List<string>> blocks, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom)
        {
            List<string> output = new List<string>();

            foreach (var input in blocks)
            {
                string block = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                foreach (var line in input)
                {
                    block += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
                }

                for (int a = 0; a < spacingtop; a++)
                {
                    block += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
                }

                if (!openbottom)
                {
                    block += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
                }

                output.Add(block);
            }
            return output;
        }

        /// <summary>
        /// You use this function if you want to make a  box with custom bottom text
        /// </summary>
        /// <param name="input">This is the list of string wich are the lines where the box needs to made around</param>
        /// <param name="sym">This is the symbole the box will be made from</param>
        /// <param name="spacingside">This is the amount of spaces between the sides of the box and the lines</param>
        /// <param name="spacingtop">This is the amount of lines that will be added before and after the lines</param>
        /// <param name="maxlength">his is the max length of a line</param>
        /// <param name="openbottom">If this is true the box will not have a bottom made from sym</param>
        /// <param name="bottomtext">This is a list of strings where this is an extra list of lines</param>
        /// <returns>This function returns a list of strings with consists of a list of boxes with sym around it and has custom bottom text</returns>
        protected static string BoxAroundText(List<string> input, string sym, int spacingside, int spacingtop, int maxlength, bool openbottom, List<string> bottomtext)
        {
            string output = new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            foreach (var line in input)
            {
                output += sym + new string(' ', spacingside) + line + new string(' ', spacingside) + sym + "\n";
            }

            for (int b = 0; b < bottomtext.Count; b++)
            {
                output += sym + new string(' ', spacingside) + bottomtext[b] + new string(' ', spacingside) + sym + "\n";
            }

            for (int a = 0; a < spacingtop; a++)
            {
                output += sym + new string(' ', maxlength + spacingside * 2) + sym + "\n";
            }

            if (openbottom)
            {
                return output;
            }
            return output += new string(Convert.ToChar(sym), maxlength + 2 + spacingside * 2) + "\n";
        }

        protected static List<string> MakeInfoBoxes(List<List<string>> DisplayInfo, int pos, string BottomText, bool posNoSelect, int totalBoxLength, int subBoxPadding) 
        {
            List<string> boxes = new List<string>();
            if (pos == 0)
            {
                boxes.Add(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(0, 2), "#")[0], "#", 2, 0, totalBoxLength, true,
                    new List<string> { $"{(!posNoSelect ? BottomText : "Niet mogelijk".PadRight(subBoxPadding))}##".PadRight(totalBoxLength), "##".PadLeft(subBoxPadding + 2) + "".PadRight(subBoxPadding + 2) }));

                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(2, DisplayInfo.Count - 2), "#")
                    , "#", 2, 0, totalBoxLength, true));
            }
            else if (pos == DisplayInfo.Count)
            {
                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(0, DisplayInfo.Count - 2), "#"), "#", 2, 0, totalBoxLength, true));

                boxes.Add(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(DisplayInfo.Count - 2, 2), "#")[0], "#", 2, 0, totalBoxLength, true,
                    new List<string> { $"{(!posNoSelect ? BottomText : "Niet mogelijk".PadRight(subBoxPadding))}##".PadRight(totalBoxLength), "##".PadLeft(subBoxPadding + 2) + "".PadRight(subBoxPadding + 2) }));
            }
            else if (pos % 2 == 0)
            {
                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(0, pos), "#"), "#", 2, 0, totalBoxLength, true));

                boxes.Add(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(pos, 2), "#")[0], "#", 2, 0, totalBoxLength, true,
                    new List<string> { $"{(!posNoSelect ? BottomText : "Niet mogelijk".PadRight(subBoxPadding))}##".PadRight(totalBoxLength), "##".PadLeft(subBoxPadding + 2) + "".PadRight(subBoxPadding + 2) }));

                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(pos + 2, DisplayInfo.Count - (pos + 2)), "#"), "#", 2, 0, totalBoxLength, true));
            }
            else if (pos % 2 == 1)
            {
                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(0, pos - 1), "#"), "#", 2, 0, totalBoxLength, true));

                boxes.Add(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(pos - 1, 2), "#")[0], "#", 2, 0, totalBoxLength, true,
                    new List<string> { "".PadRight(subBoxPadding) + $"##  {(!posNoSelect ? BottomText : "Niet mogelijk".PadRight(subBoxPadding))}", "##".PadLeft(subBoxPadding + 2) + "".PadRight(subBoxPadding + 2) }));

                boxes.AddRange(BoxAroundText(MakeDubbelBoxes(DisplayInfo.GetRange(pos + 1, DisplayInfo.Count - (pos + 1)), "#"), "#", 2, 0, totalBoxLength, true));
            }

            return boxes;
        }
        
        protected static int NavigateBoxes(int pos, List<List<string>> DisplayInfo, ConsoleKeyInfo key) 
        {
           
            if (IsKeyPressed(key, UP_ARROW))
            {
                if (pos > 1)
                {
                    pos -= 2;
                }
            }

            else if (IsKeyPressed(key, DOWN_ARROW))
            {
                if (pos < DisplayInfo.Count - 2)
                {
                    pos += 2;
                }
            }
            else if (IsKeyPressed(key, LEFT_ARROW))
            {
                if (pos % 2 == 1)
                {
                    pos -= 1;
                }

            }
            else if (IsKeyPressed(key, RIGHT_ARROW))
            {
                if (pos % 2 == 0)
                {
                    pos += 1;
                }
            }

            return pos;
        }

        /// <summary>
        /// you use this function if you want to put 2 lists of lines together to make one big box
        /// </summary>
        /// <param name="input">This is the list of list string where this is a list of list lines</param>
        /// <param name="sym">This is the symbole you want to use as a saperator for the 2 boxes. It is smart to use the same symbol here you also use for the boxaroundtext function</param>
        /// <returns>This returns a list of list lines</returns>
        protected static List<List<string>> MakeDubbelBoxes(List<List<string>> input, string sym)
        {
            List<List<string>> output = new List<List<string>>();
            for (int a = 0; a < input.Count; a += 2)
            {
                //When you have a uneven list you need to check if you are at the last item and then break.
                //This is because we take steps of 2 in this forloop (a+=2)
                if (a == input.Count - 1)           
                {
                    output.Add(input[a]);
                    break;
                }
                //blocknew is new list of blocks where block n and block n + 1 are concatenated togerther.
                //This means that all the lines from blockold1 aka block n and all the lines from blockold2 aka block n + 1 are added together.
                //This will make one big block with sym + sym as a seperator thus making 2 boxes next to each other
                List<string> blocknew = new List<string>();
                List<string> blockold1 = input[a];
                List<string> blockold2 = input[a + 1];

                if (input[a].Count > input[a + 1].Count)
                {
                    blockold2.AddRange(Enumerable.Repeat(input[a + 1][^1], input[a].Count - input[a + 1].Count));
                    //blockold2.Add(input[a + 1][^1]);
                }
                else if (input[a].Count < input[a + 1].Count)
                {
                    blockold1.AddRange(Enumerable.Repeat(input[a][^1], input[a + 1].Count - input[a].Count));
                    //blockold1.Add(input[a][^1]);
                }

                for (int b = 0; b < blockold1.Count; b++)
                {
                    blocknew.Add(blockold1[b] + $"{sym + sym}  " + blockold2[b]);
                }
                output.Add(blocknew);
            }

            return output;
        }

        protected static void Geluid(bool gelukt)
        {
            if (gelukt)
            {
                Console.Beep(330, 100);
                Console.Beep(330, 100);
                Console.Beep(440, 100);
                Console.Beep(988, 300);
                return;
            }
            else
            {
                Console.Beep(440, 100);
                Console.Beep(440, 100);
                Console.Beep(330, 100);
                Console.Beep(247, 300);
                return;
            }

        }

        protected static List<string> MakeDayOfWeekView(List<List<string>> box1andbox2Lines, List<List<string>> box3Lines, string sym, int maxLength)
        {
            List<List<string>> box1andbox2 = MakeDubbelBoxes(box1andbox2Lines, sym);
            for (int a = 1, b = 0; a < 4; a += 2, b++)
            {
                box1andbox2.Insert(a, box3Lines[b]);
            }
            return BoxAroundText(MakeDubbelBoxes(box1andbox2, sym), sym, 2, 0, maxLength, true);
        }
    }

    internal class TestDataGeneratorScreen : Screen
    {
        public TestDataGeneratorScreen(DateTime newSetDate) : base(newSetDate) { }

        private static int MaakRondleidingen()
        {
            Console.WriteLine();
            Console.WriteLine("Geef de start datum op vanaf wanneer je rondleidingen wilt maken. Format: dd-MM-YYYY");
            DateTime start = Convert.ToDateTime(ReadLine());
            Console.WriteLine();
            Console.WriteLine("Geef de eind datum op vanaf wanneer je rondleidingen wilt maken. Format: dd-MM-YYYY");
            DateTime end = Convert.ToDateTime(ReadLine());
            Console.WriteLine();
            (List<Rondleiding>, Exception) result = TestDataGenerator.MaakRondleidingen(start, end, false);
            if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
            {
                Console.WriteLine($"Er is een error opgetreden: {result.Item2}");
                Thread.Sleep(4000);
                return 1;
            }

            JsonManager.SerializeRondleidingen(result.Item1);

            Console.WriteLine("Rondleidingen zijn opgeslagen!");
            Thread.Sleep(4000);

            return -1;
        }
        /// <summary>
        /// This is the main entrypoint for the current screen. In here you can do whatever you want your screen to do.
        /// </summary>
        /// <returns>here you return the index of the next screen. This index is based on the Screens field in the program class</returns>
        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            Console.WriteLine("TestDataGeneratorScreen");
            Console.WriteLine("Druk op [1] om gebruikers aan te maken.");
            Console.WriteLine("Druk op [2] om rondleidingen aan te maken.");
            Console.WriteLine("Druk op [3] om PR-1 test data aan te maken.");
            ConsoleKeyInfo input = ReadKey();
            //(string, int) answer = AskForInput(0);
            
            if (IsKeyPressed(input, "D1") || IsKeyPressed(input, "NUMPAD1"))
            {
                int amount = 0;
                Console.WriteLine();
                bool isNum;
                do
                {
                    Console.WriteLine("Hoeveel gebruikers wilt u aanmaken: ");
                    (string?, int) answer = AskForInput(0);
                    isNum = int.TryParse(answer.Item1, out _);
                    if (!isNum)
                    {
                        Console.WriteLine("Dit was geen getal...");
                        Thread.Sleep(2000);
                        Console.Clear();
                    }
                    else
                    {
                        amount = Convert.ToInt32(answer.Item1);
                    }
                } while (!isNum);

                List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen();
                if (rondleidingen.Count <= 0)
                {
                    Console.WriteLine("Er zijn nog geen rondleidingen in het systeem.");
                    int screen = MaakRondleidingen();
                    if (screen != -1)
                    {
                        return screen;
                    }
                }
                (List<User>, Exception) result = TestDataGenerator.MaakGebruikers(Convert.ToInt32(amount));
                if (result.Item2.Message != "Exception of type 'System.Exception' was thrown.")
                {
                    Console.WriteLine($"Er is een error opgetreden: {result.Item2}");
                    Thread.Sleep(4000);
                    return 1;
                }
                Console.WriteLine();
                JsonManager.SerializeGebruikers(result.Item1);
                Console.WriteLine("De gebruikers zijn aangemaakt. Druk op een toets om terug te gaan of druk op [1] om de aangemaakte users te zien.");
                ConsoleKeyInfo key = ReadKey();
                Console.WriteLine();
                if (IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1"))
                {
                    List<List<string>> gebruikers = new List<List<string>>();
                    foreach (var gebruiker in result.Item1)
                    {
                        List<string> gebruikerInfo = new List<string>
                        {
                            "".PadRight(40),
                            "".PadRight(40),
                            $"Unieke code: {gebruiker.UniekeCode}".PadRight(40),
                            $"Reservering datum: {gebruiker.Reservering}".PadRight(40),
                            "".PadRight(40),
                            "".PadRight(40),
                        };
                        gebruikers.Add(gebruikerInfo);
                    }

                    List<string> boxes = BoxAroundText(MakeDubbelBoxes(gebruikers, "#"), "#", 2, 0, 84, false);
;
                    for (int a = 0; a < boxes.Count; a++)
                    {
                        Console.WriteLine(boxes[a]);
                    }
                    Console.WriteLine("Druk op een toets om terug te gaan.");
                    ReadKey();
                }
            }
            else if (IsKeyPressed(input, "D2") || IsKeyPressed(input, "NUMPAD2"))
            {
                int screen = MaakRondleidingen();
                if (screen != -1)
                {
                    return screen;
                }
            }
            else if (IsKeyPressed(input, "D3") || IsKeyPressed(input, "NUMPAD3"))
            {
                Console.WriteLine();
                Console.WriteLine("Test data aanmaken voor PR-1.");
                File.Delete("gebruikers.json");
                File.Delete("rondleidingen.json");
                File.Delete("rondleidingenweekschema.json");
                File.Copy(@"..\..\..\testing\preconditions\PR-1\gebruikers.json", "gebruikers.json");
                File.Copy(@"..\..\..\testing\preconditions\PR-1\rondleidingen.json", "rondleidingen.json");
                File.Copy(@"..\..\..\testing\preconditions\PR-1\rondleidingenweekschema.json", "rondleidingenweekschema.json");
                List<User> gebruikers = JsonManager.DeserializeGebruikers();
                List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen();

                for (int a = 0; a < gebruikers.Count; a++)
                {
                    if (gebruikers[a].Reservering != new DateTime(1, 1, 1))
                    {
                        gebruikers[a].Reservering = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, gebruikers[a].Reservering.Hour, gebruikers[a].Reservering.Minute, 0);
                    }
                }

                for (int b = 0; b < rondleidingen.Count; b++)
                {
                    rondleidingen[b].Datum = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, rondleidingen[b].Datum.Hour, rondleidingen[b].Datum.Minute, 0);
                }

                JsonManager.SerializeGebruikers(gebruikers);
                JsonManager.SerializeRondleidingen(rondleidingen);

                Console.WriteLine("Test data is aangemaakt. U wordt nu terug gestuurd.");
                dualOutput.Close();
                File.Delete("output.txt");
                dualOutput.ReStartWriter();
                Thread.Sleep(3000);
            }
            else if (IsKeyPressed(input, ESCAPE_KEY))
            {
                return 0;
            }
            else
            {
                return 1;
            }
            return 0;
        }
    }

    internal class HomeScreen : Screen {

        public HomeScreen(DateTime newSetDate) : base(newSetDate) { }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            if (newSetDate.DayOfWeek == DayOfWeek.Sunday)
            {
                Console.WriteLine("Op zondag zijn wij gesloten. \nMorgen vanaf 10:00 zijn we weer open.");
                ReadKey();
                return 0;
            }

            int pos = 0;
            bool cont = true;
            List<List<string>> rondleidingInformatie = new List<List<string>>();
            List<Rondleiding> alleRondleidingen = JsonManager.DeserializeRondleidingen();
            List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(DATE_FORMAT) == newSetDate.ToString(DATE_FORMAT)).OrderBy(r => r.Datum).ToList();
            if (!File.Exists("rondleidingenweekschema.json"))
            {
                JsonManager.SerializeRondleidingenWeekschema(TestDataGenerator.MaakStdWeekschema());
            }

            if (rondleidingen.Count <= 0)
            {
                JsonManager.SerializeRondleidingen(TestDataGenerator.MaakRondleidingen(new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0),
                    new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 17, 0, 0), true).Item1);

                rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(DATE_FORMAT) == newSetDate.ToString(DATE_FORMAT)).OrderBy(r => r.Datum).ToList();
            }

            if (rondleidingen.Count <= 0)
            {
                Console.WriteLine("Vandaag zijn wij gesloten.");
                ReadKey();
                return 0;
            }
            List<DateTime> tijden = new List<DateTime>();
            DateTime time = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0);
            for (int i = 0; i < rondleidingen.Count; i++)
            {
                tijden.Add(time);
                if (rondleidingen[i].RondleidingGestart)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(21),
                        $"Rondleiding gestart".PadRight(21),
                        "".PadRight(21),
                    });
                }
                else if (rondleidingen[i].Bezetting >= rondleidingen[i].MaxGrootte - 5 && rondleidingen[i].Bezetting < rondleidingen[i].MaxGrootte)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(21),
                        $"Nog {rondleidingen[i].MaxGrootte - rondleidingen[i].Bezetting} {(rondleidingen[i].Bezetting == rondleidingen[i].MaxGrootte - 1 ? "plek" : "plekken")}".PadRight(21),
                        "".PadRight(21),
                    });
                }
               else if (rondleidingen[i].Bezetting < rondleidingen[i].MaxGrootte - 5)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(21),
                        "".PadRight(21),
                    });
                }
                else
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(21),
                        $"VOL!!!!!".PadRight(21),
                        "".PadRight(21),
                    });
                }

                time = time.AddMinutes(20);
            }

            do
            {
                List<string> boxes = MakeInfoBoxes(rondleidingInformatie, pos, "[1] Reserveren       ", 
                    rondleidingen[pos].Bezetting == 13 || rondleidingen[pos].RondleidingGestart, 46, 21);

                if (!Console.IsInputRedirected) Console.Clear();
                for (int i = 0; i < boxes.Count; i++)
                {
                    Console.Write(boxes[i]);
                }

                Console.WriteLine(new string('#', 52));
                Console.WriteLine("Gebruik de pijltoesten om te navigeren.");
                Console.WriteLine("Druk op [2] om je reservering en unieke te bekijken.");
                Console.WriteLine("Druk op [3] om je reservering te annuleren.");
                Console.WriteLine("Druk op [4] om naar de gidsomgeving te gaan.");
                Console.WriteLine("Druk op [5] om naar de afdelingshoofdomgeving te gaan.");
                Console.WriteLine("Druk op [9] voor developper scherm.");
                Console.WriteLine("Druk op escape om terug te gaan.");
                ConsoleKeyInfo key = ReadKey();

                pos = NavigateBoxes(pos, rondleidingInformatie, key);

                
                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    cont = false;
                }
                else if ((IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1")) && rondleidingen[pos].Bezetting < 13)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(new string('_', 48));
                    Console.WriteLine("Vul hier uw unieke code in: ");
                    (string?, int) answer = AskForInput(0);
                    if (answer.Item2 != -1)
                    {
                        return answer.Item2;
                    }
                    List<User> gebruikers = JsonManager.DeserializeGebruikers();

                    for (int a = 0; a < gebruikers.Count; a++)
                    {
                        if (gebruikers[a].UniekeCode == answer.Item1 && gebruikers[a].Reservering != new DateTime(1, 1, 1))
                        {
                            Console.WriteLine();
                            Geluid(false);
                            Console.WriteLine($"U heeft al een reservering geplaatst om {gebruikers[a].Reservering.ToString(DATE_TIME_FORMAT)}");
                            Console.WriteLine($"Wilt u de uw reservering verplaatsen naar: {tijden[pos].ToString(DATE_TIME_FORMAT)}? (y/n)");
                            key = ReadKey();
                            if (key.Key.ToString().ToUpper() == "Y")
                            {
                                gebruikers[a].Reservering = tijden[pos];

                                JsonManager.SerializeGebruikers(gebruikers);

                                alleRondleidingen[alleRondleidingen.FindIndex(r => r.Datum == rondleidingen[pos].Datum)].Bezetting += 1;

                                JsonManager.SerializeRondleidingen(alleRondleidingen);

                                Console.WriteLine();
                                Geluid(true);
                                Console.WriteLine($"De reservering om {tijden[pos].ToString(DATE_TIME_FORMAT)} is geplaatst. U wordt terug gestuurd...");
                            }
                            Thread.Sleep(2000);
                            return 0;
                        }
                        else if (gebruikers[a].UniekeCode == answer.Item1)
                        {
                            gebruikers[a].Reservering = tijden[pos];

                            JsonManager.SerializeGebruikers(gebruikers);

                            alleRondleidingen[alleRondleidingen.FindIndex(r => r.Datum == rondleidingen[pos].Datum)].Bezetting += 1;

                            JsonManager.SerializeRondleidingen(alleRondleidingen);

                            Console.WriteLine();
                            Geluid(true);
                            Console.WriteLine($"De reservering om {tijden[pos].ToString(DATE_TIME_FORMAT)} is geplaatst. U wordt terug gestuurd...");
                            Thread.Sleep(3000);
                            return 0;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Deze unieke code is bij ons niet bekent");
                    Thread.Sleep(2000);
                }
                else if (IsKeyPressed(key, "D2") || IsKeyPressed(key, "NUMPAD2"))
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(new string('_', 48));
                    Console.WriteLine("Voer hier uw unieke code in: ");
                    (string?, int) answer = AskForInput(0);
                    if (answer.Item2 != -1)
                    {
                        return answer.Item2;
                    }
                    List<User> gebruikers = JsonManager.DeserializeGebruikers();

                    //Als de gebruiker is gevonden, returned hij het naar een nieuwe variabel
                    User gebruiker = gebruikers.First(geb => geb.UniekeCode == answer.Item1);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Uw gegevens:");
                    Console.WriteLine(BoxAroundText(new List<string>
                    {
                        $"Uw unieke code: {gebruiker.UniekeCode}".PadRight(43),
                        gebruiker.Reservering != default ? $"Uw heb uw reservering staan op: {gebruiker.Reservering.ToString(TIME_FORMAT)}".PadRight(43)
                        : "U heeft nog geen reservering".PadRight(43),
                    }, "#", 2, 1, 43, false));
                    Console.WriteLine("Druk op een knop om terug te gaan...");
                    ReadKey();
                }
                else if (IsKeyPressed(key, "D3") || IsKeyPressed(key, "NUMPAD3"))
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(new string('_', 48));
                    Console.WriteLine("Voer hier uw unieke code in: ");
                    (string?, int) answer = AskForInput(0);
                    if (answer.Item2 != -1)
                    {
                        return answer.Item2;
                    }
                    List<User> gebruikers = JsonManager.DeserializeGebruikers();

                    //Als de gebruiker is gevonden, returned hij het naar een nieuwe variabel
                    int index = gebruikers.FindIndex(geb => geb.UniekeCode == answer.Item1);

                    if (index != -1 && gebruikers[index].Reservering == default)
                    {
                        //Als de gebruiker geen reservering heeft, geef de volgende melding
                            Console.WriteLine();
                            Console.WriteLine("U heeft nog geen reservering geplaatst");
                            Thread.Sleep(2000);
                            return 0;
                    }
                    else if (index == -1)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Deze unieke code is bij ons niet bekend. U wordt weer terug gestuurd.");
                        Thread.Sleep(2000);
                        return 0;
                    }

                    alleRondleidingen[alleRondleidingen.FindIndex(r => r.Datum == gebruikers[index].Reservering)].Bezetting -= 1;

                    JsonManager.SerializeRondleidingen(alleRondleidingen);

                    //Zet de resveringsdatum naar default om te legen / resetten
                    gebruikers[index].Reservering = default;

                    JsonManager.SerializeGebruikers(gebruikers);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Uw reservering is succesvol geannuleerd");
                    Thread.Sleep(2000);
                    return 0;
                }
                else if (IsKeyPressed(key, "D4") || IsKeyPressed(key, "NUMPAD4"))
                {
                    return 3;
                }
                else if (IsKeyPressed(key, "D5") || IsKeyPressed(key, "NUMPAD5"))
                { 
                    return 2;
                }

                else if (IsKeyPressed(key, "D9") || IsKeyPressed(key, "NUMPAD9"))
                {
                    return 1;
                }

            } while (cont);
            return 0;
        }
    }

    internal class AfdelingshoofdScherm : Screen {

        public AfdelingshoofdScherm(DateTime newSetDate) : base(newSetDate) { }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            Console.WriteLine("AfdelingshoofdScherm");
            Console.WriteLine("Druk op [1] om de rondleidings bezettingsgraad te zien.");
            Console.WriteLine("Druk op [2] om het schema voor een bepaalde dag aan te passen.");
            Console.WriteLine("Druk op escape om terug te gaan.");
            ConsoleKeyInfo key = ReadKey();

            if (IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1"))
            {
/*                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r =>
                cultureInfo.Calendar.GetWeekOfYear(r.Datum, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek) ==
                cultureInfo.Calendar.GetWeekOfYear(newSetDate, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek)).ToList();*/
                List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.Month == newSetDate.Month).ToList();
                List<List<Rondleiding>> rondleidingenPerDay = new List<List<Rondleiding>>
                {
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Monday).ToList(),
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Tuesday).ToList(),
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Wednesday).ToList(),
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Thursday).ToList(),
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Friday).ToList(),
                    rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Saturday).ToList(),
                };
                List<List<string>> dayofweeklines1and2 = new List<List<string>>
                {
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Maandag".PadLeft(17).PadRight(30),
                        { rondleidingenPerDay[0].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[0].Select(r => r.Bezetting).Average()}".PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Dinsdag".PadLeft(18).PadRight(30),
                        { rondleidingenPerDay[1].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[1].Select(r => r.Bezetting).Average()}".PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Donderdag".PadLeft(18).PadRight(30),
                        { rondleidingenPerDay[3].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[3].Select(r => r.Bezetting).Average()}".PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Vrijdag".PadLeft(18).PadRight(30),
                        { rondleidingenPerDay[4].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[4].Select(r => r.Bezetting).Average()}".PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                };
                List<List<string>> dayofweeklines1and3 = new List<List<string>>
                {
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Woensdag".PadLeft(18).PadRight(30),
                        { rondleidingenPerDay[2].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[2].Select(r => r.Bezetting).Average()}".PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                    new List<string>
                    {
                        "".PadRight(30),
                        "".PadRight(30),
                        "Zaterdag".PadLeft(18).PadRight(30),
                        { rondleidingenPerDay[5].Count() != 0 ? $"Gemiddeld aantal bezoekers: {(int)rondleidingenPerDay[5].Select(r => r.Bezetting).Average()}".PadLeft(12).PadRight(30) :
                        $"Geen rondleidingen bekend".PadRight(30)},
                        "".PadRight(30),
                        "".PadRight(30),
                    },
                };

                List<string> weekboxes = MakeDayOfWeekView(dayofweeklines1and2, dayofweeklines1and3, "#", 98);

                for (int a = 0; a < weekboxes.Count; a++)
                {
                    Console.Write(weekboxes[a]);
                }
                Console.WriteLine(new string('#', 104));

                Console.WriteLine("");
                RondleidingenAdviesAlgoritme adviesRondleidingen = new RondleidingenAdviesAlgoritme();

                Console.WriteLine(adviesRondleidingen.PrintAdviesPerRondleiding(rondleidingen));
                Console.WriteLine(adviesRondleidingen.PrintAdviesPerDag(adviesRondleidingen.BerekenGemiddeldeGroepsgroottePerDag(rondleidingenPerDay), adviesRondleidingen.BerekenGemiddeldeBezettingPerDag(rondleidingenPerDay)));
                Console.WriteLine("");
                Console.WriteLine("Wilt u de data naar een bestand overbrengen? Y / N?");
                ConsoleKeyInfo input = ReadKey();

                if (IsKeyPressed(input, "Y"))
                {
                    JsonManager.SerializeBezettingsgraden(rondleidingen);
                    return 0;
                }
                else if (IsKeyPressed(input, "N"))
                {
                    return 0;
                }
                else
                {
                    Console.WriteLine("Voer Y of N in!");
                }
            }
            else if (IsKeyPressed(key, "D2") || IsKeyPressed(key, "NUMPAD2"))
            {
                return 5;

            }
            else if (IsKeyPressed(key, "Escape"))
            {
                return 2;
            }
            return 0;
        }
    }

    internal class InlogGidsScherm : Screen 
    {

        public InlogGidsScherm(DateTime newSetDate) : base(newSetDate) { }

        internal override int DoWork(DualConsoleOutput dualOutput) 
        {
            string username = "gids";
            string password = "123";

            Console.WriteLine("Gebruikersnaam:");
            string usernameingevoerd = ReadLine();
            Console.WriteLine("Wachtwoord:");
            string passwordingevoerd = ReadLine();

            if (username == usernameingevoerd && password == passwordingevoerd)
            {
                Console.WriteLine();
                Console.WriteLine("U wordt doorverwezen naar het gidsscherm");
                Thread.Sleep(2500);
                return 4;
            }
            else 
            {
                Console.WriteLine();
                Console.WriteLine("Onjuiste gegevens");
                Thread.Sleep(2000);
                return 3;
            }         
        }
    }

    internal class GidsScherm : Screen
    {

        public GidsScherm(DateTime newSetDate) : base(newSetDate) { }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            int pos = 0;
            bool cont = true;
            List<List<string>> rondleidingInformatie = new List<List<string>>();
            List<Rondleiding> allerondleidingen = JsonManager.DeserializeRondleidingen();
            List<Rondleiding> rondleidingen = allerondleidingen.Where(r => r.Datum.ToString(DATE_FORMAT) == newSetDate.ToString(DATE_FORMAT)).ToList();
            if (rondleidingen.Count <= 0)
            {
                JsonManager.SerializeRondleidingen(TestDataGenerator.MaakRondleidingen(new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0),
                    new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 17, 0, 0), true).Item1);

                rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(DATE_FORMAT) == newSetDate.ToString(DATE_FORMAT)).ToList();
            }
            List<DateTime> tijden = new List<DateTime>();
            DateTime time = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0);
            for (int i = 0; i < rondleidingen.Count; i++)
            {
                tijden.Add(time);
                if (rondleidingen[i].Bezetting >= rondleidingen[i].MaxGrootte - 5 && rondleidingen[i].Bezetting < rondleidingen[i].MaxGrootte)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(24),
                        $"Nog {rondleidingen[i].MaxGrootte - rondleidingen[i].Bezetting} {(rondleidingen[i].Bezetting == rondleidingen[i].MaxGrootte - 1 ? "plek" : "plekken")}".PadRight(24),
                    });
                }
                else if (rondleidingen[i].Bezetting < rondleidingen[i].MaxGrootte - 5)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(24),
                    });
                }
                else
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(TIME_FORMAT) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(TIME_FORMAT)).PadRight(24),
                        $"VOL!!!!!".PadRight(24),
                    });
                }

                if (rondleidingen[i].RondleidingGestart)
                {
                    rondleidingInformatie[^1].Add("Tour is al gestart".PadRight(24));
                    rondleidingInformatie[^1].Add("".PadRight(24));
                }
                else 
                {
                    rondleidingInformatie[^1].Add("".PadRight(24));
                }

                time = time.AddMinutes(20);
            }

            do
            {
                List<string> boxes = MakeInfoBoxes(rondleidingInformatie, pos, "[1] Rondleiding starten ", 
                    rondleidingen[pos].RondleidingGestart, 52, 24);
                
                
                if (!Console.IsInputRedirected) Console.Clear(); 
                for (int i = 0; i < boxes.Count; i++)
                {
                    Console.Write(boxes[i]);
                }

                Console.WriteLine(new string('#', 58));
                Console.WriteLine("Druk op escape om terug te gaan.");
                ConsoleKeyInfo key = ReadKey();

                pos = NavigateBoxes(pos, rondleidingInformatie, key);


                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    cont = false;
                }
                else if ((IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1")) && !rondleidingen[pos].RondleidingGestart)
                {
                    Console.WriteLine("Vul de unieke codes in");
                    Console.WriteLine(new string('_', 48));

                    List<string?> gebruikers = JsonManager.DeserializeGebruikers().Where(geb => geb.Reservering == rondleidingen[pos].Datum).Select(geb => geb.UniekeCode).ToList();
                    int bezetting = rondleidingen[pos].Bezetting;
                    while (bezetting > 0)
                    {
                        (string?, int) answer = AskForInput(0);
                        if (answer.Item2 != -1)
                        {
                            return answer.Item2;
                        }
                        if (answer.Item1.ToUpper() == "BEGIN")
                        {
                            bezetting = 0;
                            Console.WriteLine();
                            break;
                        }

                        if (gebruikers.Contains(answer.Item1))
                        {
                            Geluid(true);
                            bezetting --;
                        }
                        else Geluid(false);

                        Console.WriteLine();
                        Console.WriteLine(new string('_', 48));
                    }

                    allerondleidingen[allerondleidingen.IndexOf(rondleidingen[pos])].RondleidingGestart = true;
                    JsonManager.SerializeRondleidingen(allerondleidingen);
                    Console.WriteLine($"De rondleiding om {rondleidingen[pos].Datum.ToString(DATE_TIME_FORMAT)} kan beginnen!");
                    Thread.Sleep(2500);
                    return 4;
                }
            } while (cont);
            return 0;
        }
    }

    internal class WeekSchema : Screen
    {
        public WeekSchema(DateTime newSetDate) : base(newSetDate) { }

        private static List<string> AddRondleidingenInfo(List<string> input, List<RondleidingSettingsDayOfWeek> defaultWeekschedule, int pos)
        {
            for (int j = 0; j < defaultWeekschedule[pos].Rondleidingen.Count; j++)
            {
                input.Add(($"{defaultWeekschedule[pos].Rondleidingen[j].Item1.ToString(TIME_FORMAT)}".PadLeft(15) + $"{defaultWeekschedule[pos].Rondleidingen[j].Item2}".PadLeft(17)).PadRight(45));
            }
            input.Add("".PadRight(45));
            return input;
        }

        private static List<string> AddRondleidingenAvg(List<string> input, List<RondleidingSettingsDayOfWeek> defaultWeekschedule, List<List<Rondleiding>> rondleidingenPerDay, int pos)
        {
            for (int j = 0; j < rondleidingenPerDay.Count; j++)
            {
                if (rondleidingenPerDay[pos].Count == 0) continue;
                IEnumerable<Rondleiding> bezettingPerDay = rondleidingenPerDay[pos].Where(x => x.Datum.ToString(TIME_FORMAT) == defaultWeekschedule[pos].Rondleidingen[j].Item1.ToString(TIME_FORMAT));
                input.Add($"{bezettingPerDay.Average(x => x.Bezetting)}");
            }
            return input;
        }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            bool cont = true;
            int editDay = 0;
            int bezetting = 0;
            TimeOnly tijd = new TimeOnly();
            List<RondleidingSettingsDayOfWeek> defaultWeekschedule = new List<RondleidingSettingsDayOfWeek>();

            if (!Console.IsInputRedirected) Console.Clear();

            if (!File.Exists("rondleidingenweekschema.json"))
            {
                defaultWeekschedule = TestDataGenerator.MaakStdWeekschema();
                JsonManager.SerializeRondleidingenWeekschema(defaultWeekschedule);
            }
            else
            {
                defaultWeekschedule = JsonManager.DeserializeRondleidingenWeekschema();
            }

            RondleidingenAdviesAlgoritme adviesRondleidingen = new RondleidingenAdviesAlgoritme();
            List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.Month == newSetDate.Month).ToList();
            List<List<Rondleiding>> rondleidingenPerDay = new List<List<Rondleiding>>
            {
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Monday).ToList(),
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Tuesday).ToList(),
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Wednesday).ToList(),
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Thursday).ToList(),
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Friday).ToList(),
                rondleidingen.Where(r => r.Datum.DayOfWeek == DayOfWeek.Saturday).ToList(),
            };

            List<List<string>> dayofweeklines1and2 = new List<List<string>>
            {
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Maandag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Dinsdag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Donderdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Vrijdag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
            };
            List<List<string>> dayofweeklines1and3 = new List<List<string>>
            {
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Woensdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Zaterdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Max bezetting".PadLeft(18)).PadRight(45),

                },
            };

            List<List<string>> dayofweeklines1and2Avg = new List<List<string>>
            {
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Maandag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Dinsdag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Donderdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Vrijdag".PadLeft(25).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
            };
            List<List<string>> dayofweeklines1and3Avg = new List<List<string>>
            {
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Woensdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
                new List<string>
                {
                    "".PadRight(45),
                    "".PadRight(45),
                    "Zaterdag".PadLeft(26).PadRight(45),
                    "".PadRight(45),
                    ("Rondleidingen".PadLeft(19) + "Gemiddelde bezetting".PadLeft(18)).PadRight(45),

                },
            };

            for (int i = 0; i < defaultWeekschedule.Count; i++)
            {
                if (defaultWeekschedule[i].Day == DayOfWeek.Wednesday)
                {
                    dayofweeklines1and3[0] = AddRondleidingenInfo(dayofweeklines1and3[0], defaultWeekschedule, i);
                    dayofweeklines1and3Avg[0] = AddRondleidingenAvg(dayofweeklines1and3Avg[0], defaultWeekschedule, rondleidingenPerDay, i);
                }
                else if (defaultWeekschedule[i].Day == DayOfWeek.Saturday)
                {
                    dayofweeklines1and3[1] = AddRondleidingenInfo(dayofweeklines1and3[1], defaultWeekschedule, i);
                    dayofweeklines1and3Avg[1] = AddRondleidingenAvg(dayofweeklines1and3Avg[1], defaultWeekschedule, rondleidingenPerDay, i);
                }
                else if (defaultWeekschedule[i].Day == DayOfWeek.Monday)
                {
                    dayofweeklines1and2[0] = AddRondleidingenInfo(dayofweeklines1and2[0], defaultWeekschedule, i);
                    dayofweeklines1and2Avg[0] = AddRondleidingenAvg(dayofweeklines1and2Avg[0], defaultWeekschedule, rondleidingenPerDay, i);
                }
                else if (defaultWeekschedule[i].Day == DayOfWeek.Tuesday)
                {
                    dayofweeklines1and2[1] = AddRondleidingenInfo(dayofweeklines1and2[1], defaultWeekschedule, i);
                    dayofweeklines1and2Avg[1] = AddRondleidingenAvg(dayofweeklines1and2Avg[1], defaultWeekschedule, rondleidingenPerDay, i);
                }
                else if (defaultWeekschedule[i].Day == DayOfWeek.Thursday)
                {
                    dayofweeklines1and2[2] = AddRondleidingenInfo(dayofweeklines1and2[2], defaultWeekschedule, i);
                    dayofweeklines1and2Avg[2] = AddRondleidingenAvg(dayofweeklines1and2Avg[2], defaultWeekschedule, rondleidingenPerDay, i);
                }
                else if (defaultWeekschedule[i].Day == DayOfWeek.Friday)
                {
                    dayofweeklines1and2[3] = AddRondleidingenInfo(dayofweeklines1and2[3], defaultWeekschedule, i);
                    dayofweeklines1and2Avg[3] = AddRondleidingenAvg(dayofweeklines1and2Avg[3], defaultWeekschedule, rondleidingenPerDay, i);
                }
            }
            List<string> weekboxes = MakeDayOfWeekView(dayofweeklines1and2, dayofweeklines1and3, "#", 143);
            List<string> weekboxesAverages = MakeDayOfWeekView(dayofweeklines1and2Avg, dayofweeklines1and3Avg, "#", 143);

            for (int a = 0; a < weekboxes.Count; a++)
            {
                Console.Write(weekboxes[a]);
            }
            Console.WriteLine(new string('#', 149));

            for (int b = 0; b < weekboxesAverages.Count; b++)
            {
                Console.Write(weekboxes[b]);
            }
            Console.WriteLine(new string('#', 149));

            Console.WriteLine("Welke dag wilt u aanpassen?");
            Console.WriteLine("Druk op [1] voor Maandag");
            Console.WriteLine("Druk op [2] voor Dinsdag");
            Console.WriteLine("Druk op [3] voor Woensdag");
            Console.WriteLine("Druk op [4] voor Donderdag");
            Console.WriteLine("Druk op [5] voor Vrijdag");
            Console.WriteLine("Druk op [6] voor Zaterdag");
            Console.WriteLine("Druk op escape om terug te gaan.");
            ConsoleKeyInfo key = ReadKey();

            if (key.Key.ToString() == "Escape")
            {
                return 2;
            }
            try
            {
                editDay = Convert.ToInt32(key.KeyChar.ToString());
            }
            catch
            {
                return 5;
     
            }
            if (editDay < 1 || editDay > 6)
            {
                return 5;
            }

            do
            {
                Console.WriteLine("Welke tijd wilt u aanpassen? (hh:mm)");

                (string?, int) input = AskForInput(2);
                if (input.Item2 != -1) return input.Item2;

                try
                {
                    tijd = TimeOnly.Parse(input.Item1);
                    if (!defaultWeekschedule[editDay - 1].Rondleidingen.Select(r => r.Item1).Contains(tijd)) throw new Exception();
                }
                catch
                {
                    Console.WriteLine("Onjuiste tijd ingevoerd.");
                    Thread.Sleep(2000);
                    continue;
                }
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Wat is de maximale bezetting van de rondleiding? (hh:mm)");

                    input = AskForInput(2);
                    bezetting = Convert.ToInt32(input.Item1);
                    if (input.Item2 != -1) return input.Item2;
                    if (bezetting < 0) throw new Exception();
                }

                catch
                {
                    Console.WriteLine("Onjuiste bezetting ingevoerd.");
                    Thread.Sleep(2000);
                    continue;
                }

                cont = false;

            } while (cont);

            int location = defaultWeekschedule[editDay - 1].Rondleidingen.IndexOf(defaultWeekschedule[editDay - 1].Rondleidingen.First(r => r.Item1 == tijd));
            if (location == -1)
            {
                Console.WriteLine("Er is een probleem opgetreden in het systeem. Deze tijd kan niet gevonden worden in het schema.");
                Console.WriteLine("U wordt terug gestuurd...");
                Thread.Sleep(3000);
                return 5;
            }
            defaultWeekschedule[editDay - 1].Rondleidingen[location] = Tuple.Create(tijd, bezetting);

            JsonManager.SerializeRondleidingenWeekschema(defaultWeekschedule);
            Console.WriteLine();
            Console.WriteLine("het standaard weekschema is aangepast.");
            Thread.Sleep(3000);
            return 5;
        }
    }
}