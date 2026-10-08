namespace WukongBenchmark.WinForms.Services;

public interface IGameLocator
{
    string? TryFindGameFolder();
    string FindAndValidateGameFolder(string? providedPath);
    string FindSteamFolder();
}
