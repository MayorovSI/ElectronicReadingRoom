namespace ElectronicReadingRoom.Client.Services;

public enum AppMode
{
    Reader,
    Librarian
}

public class ModeService
{
    private const string ModeKey = "app_mode";
    private AppMode _currentMode = AppMode.Reader;

    public AppMode CurrentMode => _currentMode;

    public event Action? ModeChanged;

    public void SetMode(AppMode mode)
    {
        _currentMode = mode;
        ModeChanged?.Invoke();
    }

    public bool IsLibrarian => _currentMode == AppMode.Librarian;
}
