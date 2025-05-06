using System;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Diagnostics;

public class SpectreUI
{    
    private static string _clients = "Client 1\nClient 2\nClient 3";
    private static Dictionary<string, List<string>> _chatHistory = new Dictionary<string, List<string>>();
    private static string _messagePrompt = "Type your message here...";
    private static int _recievedCount = 0;
    private static int _sentCount = 0;
    // Helper renderable that stacks items vertically.
    public sealed class RowsRenderable : IRenderable
    {
        private readonly IRenderable[] _rows;
        public RowsRenderable(params IRenderable[] rows) => _rows = rows;

        public Measurement Measure(RenderOptions options, int maxWidth) =>
            new Measurement(0, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            foreach (var row in _rows)
                foreach (var seg in row.Render(options, maxWidth))
                    yield return seg;
        }
    }
    public static void Render(Layout layout) => AnsiConsole.Write(layout);

    public static (Layout root, Layout chatHistory, Layout clients, Layout messageBox, Layout statusBar) BuildChatLayout(
    string historyMarkup = "", string clientsMarkup = "", string messagePrompt = "Enter your message...", string statusText = "Ready")
    {
        // Root layout: three rows with percentage-based ratios (50%, 40%, 10%)
        var root = new Layout("Root").SplitRows(
            new Layout("Top").Ratio(50),     // Top half of screen
            new Layout("Message").Ratio(40), // 40% of screen
            new Layout("Status").Ratio(10)   // Remaining 10%
        );

        // Top row: split into two equal columns
        root["Top"].SplitColumns(
            new Layout("ChatHistory").Ratio(1),
            new Layout("Clients").Ratio(1)
        );

        // Chat History panel
        root["Top"]["ChatHistory"].Update(
            new Panel(new Markup(historyMarkup).Centered())
                .Header("[bold]Chat History[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("cyan"))
                .Expand()
        );

        // Clients panel
        root["Top"]["Clients"].Update(
            new Panel(new Markup(clientsMarkup).Centered())
                .Header("[bold]Clients[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("green"))
                .Expand()
        );

        // Message input box
        root["Message"].Update(
            new Panel(messagePrompt)
                .Header("[bold]Message[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("grey"))
                .Expand()
        );

        // Status bar at bottom
        root["Status"].Update(
            new Panel(statusText)
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("magenta"))
                .Expand()
        );

        // Return references to each section for further updates
        return (
            root,
            root["Top"]["ChatHistory"],
            root["Top"]["Clients"],
            root["Message"],
            root["Status"]
        );
    }

    public static void LiveConsole()
    {
        
    }
}


public class Program
{
    public static void Main(string[] args)
    {
        // Initialize the chat layout
        var (root, chatHistory, clients, messageBox, statusBar) = SpectreUI.BuildChatLayout(
            historyMarkup: "[bold]Welcome to the chat![/]",
            clientsMarkup: "[bold]Client 1\nClient 2\nClient 3[/]",
            messagePrompt: "Type your message here...",
            statusText: "Ready"
        );

        // Render the initial layout
        SpectreUI.Render(root);
    }
}