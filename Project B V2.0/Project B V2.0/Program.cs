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
        private static IntPtr ThisCon = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // State of the application once loaded
        private const int HIDE = 0;
        private const int MAXIMIZE = 3;
        private const int MINIMIZE = 6;
        private const int RESTORE = 9;


        private static int currentScreen;
        private static int lastscreen;
        private static List<Screen> screens = new List<Screen>();

        private static readonly DualConsoleOutput dualOutput = new DualConsoleOutput(@"output.txt", Console.Out);

        static void Main(string[] args)
        {
            DateTime newSetDate = new DateTime();

            Console.SetOut(dualOutput);
            ShowWindow(ThisCon, MAXIMIZE);

            if (args.Length > 0)
            {
                newSetDate = Convert.ToDateTime(args[0]);

                screens.Add(new HomeScreen(newSetDate)); // 0
                screens.Add(new TestDataGeneratorScreen(newSetDate)); // 1
                screens.Add(new AfdelingshoofdScherm(newSetDate)); //2
                screens.Add(new InlogGidsScherm(newSetDate)); //3
                screens.Add(new GidsScherm(newSetDate)); //4
            }
            else
            {
                screens.Add(new HomeScreen(DateTime.Now)); // 0
                screens.Add(new TestDataGeneratorScreen(DateTime.Now)); // 1
                screens.Add(new AfdelingshoofdScherm(DateTime.Now)); //2
                screens.Add(new InlogGidsScherm(DateTime.Now)); //3
                screens.Add(new GidsScherm(DateTime.Now)); //4
            }

            currentScreen = 0;
            do
            {
                Display();
                if (!Console.IsInputRedirected) Refresh();
            } while (currentScreen != -1);
        }

        static internal void Display()
        {
            lastscreen = currentScreen;
            currentScreen = screens[currentScreen].DoWork(dualOutput);
            screens = screens[lastscreen].Update(screens);
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
        protected const string dateFormat = "d-M-yyyy";
        protected const string timeFormat = "HH:mm";
        protected const string dateTimeFormat = "d-M-yyyy HH:mm";
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

        /// <summary>
        /// This function updates all screens with data from one screen to an other
        /// </summary>
        /// <param name="screens">This is the list of screens to update</param>
        /// <returns>This returns the same list you just gave as param but now it has been updated with information</returns>
        internal abstract List<Screen> Update(List<Screen> screens);

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

        protected static string ReadLine()
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


        [Obsolete]
        protected (string, int) AskForInput(int screenIndex)
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

                if (IsKeyPressed(CKInfo, ESCAPE_KEY)) return (null, screenIndex);

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
                    blockold2.Add(input[a + 1][^1]);
                }
                else if (input[a].Count < input[a + 1].Count)
                {
                    blockold1.Add(input[a][^1]);
                }

                for (int b = 0; b < blockold1.Count; b++)
                {
                    blocknew.Add(blockold1[b] + $"{sym + sym}  " + blockold2[b]);
                }
                output.Add(blocknew);
            }

            return output;
        }
    }

    internal class TestDataGeneratorScreen : Screen
    {
        public TestDataGeneratorScreen(DateTime newSetDate) : base(newSetDate) { }

        private int MaakRondleidingen()
        {
            Console.WriteLine();
            Console.WriteLine("Geef de start datum op vanaf wanneer je rondleidingen wilt maken. Format: dd-MM-YYYY");
            DateTime start = Convert.ToDateTime(ReadLine());
            Console.WriteLine("Geef de eind datum op vanaf wanneer je rondleidingen wilt maken. Format: dd-MM-YYYY");
            DateTime end = Convert.ToDateTime(ReadLine());
            (List<Rondleiding>, Exception) result = TestDataGenerator.MaakRondleidingen(start, end);
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
                    (string, int) answer = AskForInput(0);
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
                File.Copy(@"..\..\..\testing\preconditions\PR-1\gebruikers.json", "gebruikers.json");
                File.Copy(@"..\..\..\testing\preconditions\PR-1\rondleidingen.json", "rondleidingen.json");
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

        /// <summary>
        /// This function is needed when you need to update information on all the screens. For example when you are logged in or logged uit.
        /// It is done this way to avoid shared state.
        /// </summary>
        /// <param name="screens"></param>
        /// <returns></returns>
        internal override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }

    internal class HomeScreen : Screen {

        public HomeScreen(DateTime newSetDate) : base(newSetDate) { }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            int pos = 0;
            bool cont = true;
            List<List<string>> rondleidingInformatie = new List<List<string>>();
            List<Rondleiding> alleRondleidingen = JsonManager.DeserializeRondleidingen();
            List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(dateFormat) == newSetDate.ToString(dateFormat)).OrderBy(r => r.Datum).ToList();
            if (rondleidingen.Count <= 0)
            {
                JsonManager.SerializeRondleidingen(TestDataGenerator.MaakRondleidingen(new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0),
                    new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 17, 0, 0)).Item1);

                rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(dateFormat) == newSetDate.ToString(dateFormat)).OrderBy(r => r.Datum).ToList();
            }
            List<DateTime> tijden = new List<DateTime>();
            DateTime time = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0);
            for (int i = 0; i < rondleidingen.Count; i++)
            {
                tijden.Add(time);
                if (rondleidingen[i].Bezetting >= 8 && rondleidingen[i].Bezetting < 13)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(19),
                        $"Nog {13 - rondleidingen[i].Bezetting} {(rondleidingen[i].Bezetting == 12 ? "plek" : "plekken")}".PadRight(19),
                        "".PadRight(19),
                    });
                }
               else if (rondleidingen[i].Bezetting < 8)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(19),
                        "".PadRight(19),
                    });
                }
                else
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(19),
                        $"VOL!!!!!".PadRight(19),
                        "".PadRight(19),
                    });
                }

                time = time.AddMinutes(20);
            }

            do
            {
                List<string> boxes = MakeInfoBoxes(rondleidingInformatie, pos, "[1] Reserveren     ", 
                    rondleidingen[pos].Bezetting == 13 ? true : false, 42, 19);

                if (!Console.IsInputRedirected) Console.Clear();
                for (int i = 0; i < boxes.Count; i++)
                {
                    Console.Write(boxes[i]);
                }

                Console.WriteLine(new string('#', 48));
                Console.WriteLine("Gebruik de pijltoesten om te navigeren.");
                Console.WriteLine("Druk op [2] om je reservering te annuleren.");
                Console.WriteLine("Druk op [3] om naar het gidsscherm te gaan.");
                Console.WriteLine("Druk op [4] om naar het afdelingshoofdscherm te gaan.");
                Console.WriteLine("Druk op [9] voor developper scherm.");
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
                    (string, int) answer = AskForInput(0);
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
                            Console.WriteLine("U heeft al een reservering geplaatst");
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
                            Console.WriteLine($"De reservering om {tijden[pos].ToString(dateTimeFormat)} is geplaatst. U wordt terug gestuurd...");
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
                    (string, int) answer = AskForInput(0);
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
                else if (IsKeyPressed(key, "D3") || IsKeyPressed(key, "NUMPAD3"))
                {
                    return 3;
                }
                else if (IsKeyPressed(key, "D4") || IsKeyPressed(key, "NUMPAD4"))
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

        internal override List<Screen> Update(List<Screen> screens) {

            return screens;
        }
    }
    internal class AfdelingshoofdScherm : Screen {

        public AfdelingshoofdScherm(DateTime newSetDate) : base(newSetDate) { }

        private static List<string> MakeDayOfWeekView(List<List<string>> box1andbox2Lines, List<List<string>> box3Lines, string sym)
        {
            List<List<string>> box1andbox2 = MakeDubbelBoxes(box1andbox2Lines, sym);
            for (int a = 1, b = 0; a < 4; a += 2, b++)
            {
                box1andbox2.Insert(a, box3Lines[b]);
            }
            return BoxAroundText(MakeDubbelBoxes(box1andbox2, sym), sym, 2, 0, 68, true);
        }

        internal override int DoWork(DualConsoleOutput dualOutput)
        {
            Console.WriteLine("AfdelingshoofdScherm");
            Console.WriteLine("Druk op [1] om de rondleidings bezettingsgraad te zien.");
            Console.WriteLine("Druk op [2] om het schema voor een bepaalde dag aan te passen.");
            ConsoleKeyInfo key = ReadKey();

            if (IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1"))
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                List<Rondleiding> rondleidingen = JsonManager.DeserializeRondleidingen().Where(r =>
                cultureInfo.Calendar.GetWeekOfYear(r.Datum, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek) ==
                cultureInfo.Calendar.GetWeekOfYear(newSetDate, cultureInfo.DateTimeFormat.CalendarWeekRule, cultureInfo.DateTimeFormat.FirstDayOfWeek)).ToList();
                List<List<string>> dayofweeklines1and2 = new List<List<string>>
                {
                    new List<string> { "".PadRight(20), "".PadRight(20), "  Maandag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                    new List<string> { "".PadRight(20), "".PadRight(20), "  Dinsdag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                    new List<string> { "".PadRight(20), "".PadRight(20), "  Donderdag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                    new List<string> { "".PadRight(20), "".PadRight(20), "  Vrijdag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                };

                List<List<string>> dayofweeklines1and3 = new List<List<string>>
                {
                    new List<string> { "".PadRight(20), "".PadRight(20), "  Woensdag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                    new List<string> { "".PadRight(20), "".PadRight(20), "  zaterdag".PadRight(20), "".PadRight(20), "".PadRight(20), },
                };

                List<string> weekboxes = MakeDayOfWeekView(dayofweeklines1and2, dayofweeklines1and3, "#");

                for (int a = 0; a < weekboxes.Count; a++)
                {
                    Console.Write(weekboxes[a]);
                }
                Console.WriteLine(new string('#', 74));

                ReadLine();

            }
            else if (IsKeyPressed(key, "D2") || IsKeyPressed(key, "NUMPAD2"))
            {
                List<RondleidingSettingsDayOfWeek> defaultWeekschedule = new List<RondleidingSettingsDayOfWeek>();
                if (!File.Exists("rondleidingenweekschema.json"))
                {
                    defaultWeekschedule = TestDataGenerator.MaakStdWeekschema();
                }
                else 
                { 
                    defaultWeekschedule = JsonManager.DeserializeRondleidingenWeekschema();
                }

                






                






                /*Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Geef de start datum op vanaf wanneer je het rondleidingsschema wilt aanpassen. Format: dd-MM-YYYY");
                (string, int) input = AskForInput(0);
                Console.WriteLine();
                

                if (input.Item2 != -1)
                {
                    return input.Item2;
                }

                DateTime start = Convert.ToDateTime(input.Item1);*/



                /*Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Geef de start datum op vanaf wanneer je het rondleidingsschema wilt aanpassen. Format: dd-MM-YYYY");
                (string, int) input = AskForInput(0);
                Console.WriteLine();

                if (input.Item2 != -1)
                {
                    return input.Item2;
                }

                DateTime start = Convert.ToDateTime(input.Item1);
            }
            return 0;*/
            } 
            return 0;
        }

        internal override List<Screen> Update(List<Screen> screens)
        {

            return screens;
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

        internal override List<Screen> Update(List<Screen> screens)
        {
            return screens;
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
            List<Rondleiding> rondleidingen = allerondleidingen.Where(r => r.Datum.ToString(dateFormat) == newSetDate.ToString(dateFormat)).ToList();
            if (rondleidingen.Count <= 0)
            {
                JsonManager.SerializeRondleidingen(TestDataGenerator.MaakRondleidingen(new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0),
                    new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 17, 0, 0)).Item1);

                rondleidingen = JsonManager.DeserializeRondleidingen().Where(r => r.Datum.ToString(dateFormat) == newSetDate.ToString(dateFormat)).ToList();
            }
            List<DateTime> tijden = new List<DateTime>();
            DateTime time = new DateTime(newSetDate.Year, newSetDate.Month, newSetDate.Day, 11, 0, 0);
            for (int i = 0; i < rondleidingen.Count; i++)
            {
                tijden.Add(time);
                if (rondleidingen[i].Bezetting >= 8 && rondleidingen[i].Bezetting < 13)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(24),
                        $"Nog {13 - rondleidingen[i].Bezetting} {(rondleidingen[i].Bezetting == 12 ? "plek" : "plekken")}".PadRight(24),
                    });
                }
                else if (rondleidingen[i].Bezetting < 8)
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(24),
                    });
                }
                else
                {
                    rondleidingInformatie.Add(new List<string>
                    {
                        (rondleidingen[i].Datum.ToString(timeFormat) + "-" + rondleidingen[i].Datum.AddMinutes(40).ToString(timeFormat)).PadRight(24),
                        $"VOL!!!!!".PadRight(24),
                    });
                }

                if (rondleidingen[i].TourIsStarted)
                {
                    rondleidingInformatie[rondleidingInformatie.Count - 1].Add("Tour is al gestart".PadRight(24));
                    rondleidingInformatie[rondleidingInformatie.Count - 1].Add("".PadRight(24));
                }
                else 
                {
                    rondleidingInformatie[rondleidingInformatie.Count - 1].Add("".PadRight(24));
                }

                time = time.AddMinutes(20);
            }

            do
            {
                List<string> boxes = MakeInfoBoxes(rondleidingInformatie, pos, "[1] Rondleiding starten ", 
                    rondleidingen[pos].TourIsStarted ? true : false, 52, 24);
                
                
                if (!Console.IsInputRedirected) Console.Clear(); 
                for (int i = 0; i < boxes.Count; i++)
                {
                    Console.Write(boxes[i]);
                }

                Console.WriteLine(new string('#', 58));
                ConsoleKeyInfo key = ReadKey();

                pos = NavigateBoxes(pos, rondleidingInformatie, key);


                if (IsKeyPressed(key, ESCAPE_KEY))
                {
                    cont = false;
                }
                else if ((IsKeyPressed(key, "D1") || IsKeyPressed(key, "NUMPAD1")) && !rondleidingen[pos].TourIsStarted)
                {
                    rondleidingen[pos].TourIsStarted = true;
                    allerondleidingen[allerondleidingen.IndexOf(rondleidingen[pos])].TourIsStarted = true;
                    JsonManager.SerializeRondleidingen(allerondleidingen);
                    Console.WriteLine($"De rondleiding om {rondleidingen[pos].Datum.ToString(dateTimeFormat)} is gestart.");
                    Console.WriteLine("U wordt teruggestuurd naar het Gidsscherm.");
                    Thread.Sleep(2500);
                    return 4;
                }
            } while (cont);
            return 0;
        }


        internal override List<Screen> Update(List<Screen> screens)
        {
            return screens;
        }
    }
}