using MVDC.FullTime;

// Mole Valley Girls U9 Blacks — SCWGL 2025-26, U9 Grey v4
const int defaultSeasonId = 310037110;
const string defaultGroupKey = "1_478076013";

Console.WriteLine("===========================================");
Console.WriteLine("  MVDC.FullTime - FA Full-Time Console");
Console.WriteLine("===========================================");
Console.WriteLine();

var seasonId = ReadInt("Season ID", defaultSeasonId);
var groupKey = ReadString("Fixture Group Key", defaultGroupKey);

Console.WriteLine();
Console.WriteLine($"Using Season: {seasonId}, Group: {groupKey}");
Console.WriteLine();

var division = new Division(new FullTimeClient(new HttpClient()));

while (true)
{
    Console.WriteLine("-------------------------------------------");
    Console.WriteLine("  1. Get Teams");
    Console.WriteLine("  2. Get Fixtures (raw)");
    Console.WriteLine("  3. Get Fixtures (formatted)");
    Console.WriteLine("  4. Get Results (raw)");
    Console.WriteLine("  5. Get Results (formatted)");
    Console.WriteLine("  6. Change Season/Group");
    Console.WriteLine("  0. Exit");
    Console.WriteLine("-------------------------------------------");
    Console.Write("Choice: ");

    var choice = Console.ReadLine()?.Trim();

    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "1":
                await ShowTeams(division, seasonId, groupKey);
                break;
            case "2":
                await ShowRawFixtures(division, seasonId, groupKey);
                break;
            case "3":
                await ShowFormattedFixtures(division, seasonId, groupKey);
                break;
            case "4":
                await ShowRawResults(division, seasonId, groupKey);
                break;
            case "5":
                await ShowFormattedResults(division, seasonId, groupKey);
                break;
            case "6":
                seasonId = ReadInt("Season ID", seasonId);
                groupKey = ReadString("Fixture Group Key", groupKey);
                Console.WriteLine($"Updated to Season: {seasonId}, Group: {groupKey}");
                break;
            case "0":
            case null:
                Console.WriteLine("Bye!");
                return;
            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP Error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine();
}

// --- Helper methods ---

static async Task ShowTeams(Division division, int seasonId, string groupKey)
{
    Console.WriteLine("Fetching teams...");
    var teams = await division.GetTeamsAsync(seasonId, groupKey);
    Console.WriteLine($"Found {teams.Count} team(s):");
    for (var i = 0; i < teams.Count; i++)
        Console.WriteLine($"  {i + 1}. {teams[i]}");
}

static async Task ShowRawFixtures(Division division, int seasonId, string groupKey)
{
    Console.WriteLine("Fetching raw fixtures...");
    var fixtures = await division.GetFixturesAsync(seasonId, groupKey);
    Console.WriteLine($"Found {fixtures.Count} fixture(s):");
    foreach (var fixture in fixtures)
        Console.WriteLine($"  [{string.Join(" | ", fixture)}]");
}

static async Task ShowFormattedFixtures(Division division, int seasonId, string groupKey)
{
    Console.WriteLine("Fetching formatted fixtures...");
    var fixtures = await division.GetFormattedFixturesAsync(seasonId, groupKey);
    Console.WriteLine($"Found {fixtures.Count} fixture(s):");
    Console.WriteLine();
    Console.WriteLine($"  {"Date",-12} {"Time",-6} {"Home",-40} {"Away",-40} {"Type"}");
    Console.WriteLine($"  {new string('-', 110)}");
    foreach (var f in fixtures)
        Console.WriteLine($"  {f.Date,-12} {f.Time,-6} {f.Home,-40} {f.Away,-40} {f.FixtureType}");
}

static async Task ShowFormattedResults(Division division, int seasonId, string groupKey)
{
    Console.WriteLine("Fetching formatted results...");
    var results = await division.GetFormattedResultsAsync(seasonId, groupKey);
    Console.WriteLine($"Found {results.Count} result(s):");
    Console.WriteLine();
    Console.WriteLine($"  {"Date",-12} {"Time",-6} {"Home",-35} {"Score",-7} {"Away",-35}");
    Console.WriteLine($"  {new string('-', 100)}");
    foreach (var r in results)
        Console.WriteLine($"  {r.Date,-12} {r.Time,-6} {r.Home,-35} {r.HomeScore,2} - {r.AwayScore,-2}  {r.Away,-35}");
}

static async Task ShowRawResults(Division division, int seasonId, string groupKey)
{
    Console.WriteLine("Fetching raw results...");
    var results = await division.GetResultsAsync(seasonId, groupKey);
    Console.WriteLine($"Found {results.Count} result(s):");
    foreach (var result in results)
        Console.WriteLine($"  [{string.Join(" | ", result)}]");
}

static int ReadInt(string prompt, int defaultValue)
{
    Console.Write($"{prompt} [{defaultValue}]: ");
    var input = Console.ReadLine()?.Trim();
    return string.IsNullOrEmpty(input) ? defaultValue : int.TryParse(input, out var val) ? val : defaultValue;
}

static string ReadString(string prompt, string defaultValue)
{
    Console.Write($"{prompt} [{defaultValue}]: ");
    var input = Console.ReadLine()?.Trim();
    return string.IsNullOrEmpty(input) ? defaultValue : input;
}
