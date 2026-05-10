namespace LUAToCSV
{
    class Puzzle
    {
        public required string Number { get; set; }
        public required string Name { get; set; }
        public required string Author { get; set; }
        public required int XP { get; set; }
        public required string Size { get; set; }
        public required string Category1 { get; set; }
        public required string Category2 { get; set; }
        public required string PuzzleType { get; set; }
    }

    class Program
    {
        private static string _directory = $"{AppDomain.CurrentDomain.BaseDirectory}../../../config/";


        static async Task Main(string[] args)
        {
            List<Puzzle> _colorPuzzles = GetPuzzlesFromLua(Path.Combine(_directory, "Colors.lua"));
            List<Puzzle> _BWPuzzles = GetPuzzlesFromLua(Path.Combine(_directory, "BWs.lua"));

            WritePuzzlesToCsv("Colors.csv", _colorPuzzles);
            WritePuzzlesToCsv("BWs.csv", _BWPuzzles);
        }

        static List<Puzzle> GetPuzzlesFromLua(string luaFile)
        {
            MoonSharp.Interpreter.Script lua = new();
            var result = lua.DoFile(luaFile);

            List<Puzzle> puzzles = new();
            foreach (var item in result.Table.Values)
            {
                var table = item.Table;

                string name = table.Get("link").String;
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(name, @"#(\d+)");
                string number = $"#{match.Groups[1].Value}";

                string author = table.Get("author").String;

                int xp = new Func<int>(() =>
                {
                    string xp = table.Get("new_xp").String;
                    xp = xp.Replace("~", "");
                    return int.Parse(xp);
                })();

                string size = table.Get("size").String;
                string category_1 = table.Get("category_1").String;
                string category_2 = table.Get("category_2").String;
                string puzzle_type = table.Get("puzzle_type").String switch
                {
                    "0" => "No type",
                    "1" => "True nonogram",
                    "2" => "Recursion method",
                    "3" => "Contradiction method",
                    "4" => "Mosaic",
                    "5" => "Symmetry",
                    "6" => "Coloring",
                    "7" => "Few trivial lines",
                    _ => "Unknown"
                };

                puzzles.Add(new Puzzle
                {
                    Number = number,
                    Name = name,
                    Author = author,
                    XP = xp,
                    Size = size,
                    Category1 = category_1,
                    Category2 = category_2,
                    PuzzleType = puzzle_type
                });
            }
            return puzzles;
        }

        static void WritePuzzlesToCsv(string puzzlesFile, List<Puzzle> puzzles)
        {
            CsvHelper.Configuration.CsvConfiguration config = new(System.Globalization.CultureInfo.InvariantCulture);
            using StreamWriter writer = new(puzzlesFile);
            using CsvHelper.CsvWriter csv = new(writer, config);
            csv.WriteRecords(puzzles);
        }
    }
}