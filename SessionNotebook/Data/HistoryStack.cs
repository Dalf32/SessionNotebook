using Microsoft.AspNetCore.Components;

namespace SessionNotebook.Data;

public class HistoryStack(int maxSize = 10)
{
    private LinkedList<string> History { get; } = [];

    public bool HasHistory => History.Count > 0;

    public void Push(string url)
    {
        if (History.Count == maxSize)
        {
            History.RemoveLast();
        }

        History.AddFirst(url);
    }

    public void Push(NavigationManager navigation)
    {
        Push(navigation.Uri);
    }

    public string Pop()
    {
        if (!HasHistory)
        {
            return string.Empty;
        }

        var popped = History.First?.Value;
        History.RemoveFirst();
        return popped;

    }

    public string GetReturnUrl(string fallbackUrl)
    {
        return HasHistory ? Pop() : fallbackUrl;
    }

    public void GoBack(NavigationManager navigation, string fallbackUrl)
    {
        string returnUrl;
        do
        {
            returnUrl = GetReturnUrl(fallbackUrl);
        } while (returnUrl.Equals(navigation.Uri));

        navigation.NavigateTo(returnUrl);
    }
}
