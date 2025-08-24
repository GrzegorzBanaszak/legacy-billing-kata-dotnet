using System;
using System.Diagnostics;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Billing.Tests;

public class GoldenMasterTests
{
    [Fact]
    public async Task Output_should_match_golden_master()
    {
        // Ustal kulturę, żeby kropki/przecinki nie rozjeżdżały się między maszynami
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        var repoRoot = FindRepoRoot();
        var legacyProj = Path.Combine(repoRoot, "src", "Billing.Legacy");
        var samples = Path.Combine(repoRoot, "samples", "input-sample.json");
        var golden = Path.Combine(repoRoot, "tests", "Resources", "golden-master.txt");

        // Dla stabilności: zbuduj projekt przed uruchomieniem
        Run("dotnet", "build", legacyProj).ExitCode.Should().Be(0);

        // Uruchom: dotnet run --project src/Billing.Legacy -- samples/input-sample.json
        var runResult = Run("dotnet", $"run --project \"{legacyProj}\" -- \"{samples}\"");

        runResult.ExitCode.Should().Be(0, because: runResult.Stderr);
        var actual = NormalizeNewLines(runResult.Stdout);
        var expected = NormalizeNewLines(await File.ReadAllTextAsync(golden, Encoding.UTF8));

        actual.Should().Be(expected, "program output must match the golden master snapshot");

    }


   private static (int ExitCode, string Stdout, string Stderr) Run(string fileName, string arguments, string? workingDir = null)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDir ?? Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        return (p.ExitCode, stdout, stderr);
    }


    private static string NormalizeNewLines(string s)
       => s.Replace("\r\n", "\n").TrimEnd() + "\n"; // ujednolicenie CRLF/LF i trailing newline
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "LegacyBilling.sln")))
        {
            dir = dir.Parent ?? throw new DriveNotFoundException("Repo root not found.");
        }
        return dir!.FullName;
    }
}
