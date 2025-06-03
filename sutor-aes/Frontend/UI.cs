using Spectre.Console;

public class Message
{
    public string? From    { get; set; }
    public string? To      { get; set; }
    public string? Content { get; set; }
    public DateTime Time   { get; set; }
}

public class SpectreUI
{
    // --------------------- state ----------------------------
    private static readonly Dictionary<string, bool> _clients  = new();
    private static readonly List<Message>            _messages = new();

    private static int _receivedCount;
    private static int _sentCount;

    private static string _selectedClient = "Client2"; // logical selection
    private static readonly string _ourUser = "Client1";

    private static string _messageBox = "Type your message here...";

    // focus & navigation helpers
    private enum Focus { Clients, ChatHistory, MessageInput }
    private static Focus _currentFocus = Focus.Clients;
    private static int   _clientSelectedIndex;   // cursor inside Clients panel
    private static int   _chatScrollOffset;      // how many lines user scrolled up from the bottom (0 == stick to bottom)

    // --------------------- layout handles -------------------
    private static Layout _rootLayout         = new("root");
    private static Layout _chatHistoryLayout  = new("chat");
    private static Layout _clientsLayout      = new("clients");
    private static Layout _messageBoxLayout   = new("msg");
    private static Layout _statusBarLayout    = new("status");

    // clock
    private static System.Timers.Timer? _clockTimer;

    // sync when multiple threads update console
    private static readonly object _renderLock = new();

    // ** CHANGED **: store the LiveContext returned inside Start(...) as dynamic
    private static dynamic? _live;

    // --------------------------------------------------------
    //  public API
    // --------------------------------------------------------
    public static void Run()
    {
        BuildChatLayout();

        // Start a Live context around `_rootLayout`. Use the overload of Start(...) that takes a callback.
        AnsiConsole.Live(_rootLayout)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                // `ctx` is the LiveContext<Layout> at runtime; store it as `dynamic`
                _live = ctx;

                // Perform the first draw
                RedrawAll();

                // Start a 1-second timer to update the clock in the status bar
                _clockTimer = new System.Timers.Timer(1000) { AutoReset = true, Enabled = true };
                _clockTimer.Elapsed += (_, _) =>
                {
                    lock (_renderLock)
                    {
                        UpdateStatusBar();
                    }
                };

                // Enter our input loop (blocks indefinitely)
                InputLoop();
            });
        // When InputLoop() eventually exits (it never will), the Live scope ends.
    }

    public static void UpdateStatus(string user, bool connected)
    {
        lock (_renderLock)
        {
            _clients[user] = connected;
            UpdateClientLayout();
            UpdateStatusBar();
        }
    }

    public static void UpdateChat(Message message)
    {
        lock (_renderLock)
        {
            _messages.Add(message);
            if (message.From == _ourUser) _sentCount++;
            else                          _receivedCount++;

            // New message → snap chat to bottom
            _chatScrollOffset = 0;
            RedrawAll();
        }
    }
    
    public static void UpdateMessageBox(string text)
    {
        lock (_renderLock)
        {
            _messageBox = text;

            // Update only the message‐box panel
            _messageBoxLayout.Update(
                new Panel(new Markup(_messageBox))
                    .Header("[bold]Message[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(_currentFocus == Focus.MessageInput
                                     ? Style.Parse("yellow")
                                     : Style.Parse("green"))
                    .Expand()
            );

            // Push changes into the live context
            _live?.UpdateTarget(_rootLayout);
            _live?.Refresh();
        }
    }

    // --------------------------------------------------------
    //  Layout builders
    // --------------------------------------------------------
    private static void BuildChatLayout()
    {
        _rootLayout = new Layout("root").SplitRows(
            new Layout("top").Ratio(70),       // chat + clients
            new Layout("msg").Ratio(25),       // message input
            new Layout("status").Ratio(5)      // status bar (non‐focusable)
        );
        _rootLayout["top"].SplitColumns(
            new Layout("chat"),
            new Layout("clients")
        );

        _chatHistoryLayout = _rootLayout["top"]["chat"];
        _clientsLayout     = _rootLayout["top"]["clients"];
        _messageBoxLayout  = _rootLayout["msg"];
        _statusBarLayout   = _rootLayout["status"];
    }

    // --------------------------------------------------------
    //  Rendering helpers (called whenever something changes)
    // --------------------------------------------------------
    private static void RedrawAll()
    {
        UpdateClientLayout();
        UpdateChatLayout();
        UpdateMessageBox(_messageBox);
        UpdateStatusBar();
    }

    private static void UpdateClientLayout()
    {
        // Ensure the selected index is in range
        _clientSelectedIndex = Math.Clamp(_clientSelectedIndex, 0, Math.Max(0, _clients.Count - 1));
        if (_clients.Count > 0)
            _selectedClient = _clients.Keys.ElementAt(_clientSelectedIndex);

        var lines    = new List<string>();
        int widthHalf = Console.WindowWidth / 2 - 10;

        int idx = 0;
        foreach (var kvp in _clients)
        {
            var name        = kvp.Key;
            bool isConnected = kvp.Value;
            string statusMarkup = isConnected ? "[green]CONNECTED[/]" : "[red]DISCONNECTED[/]";
            int statusLen = isConnected ? 9 : 12;

            // ** ALWAYS show cursor next to selected client, regardless of focus **
            bool isCursor = idx == _clientSelectedIndex;

            string cursorStart = isCursor ? "[bold yellow]> " : string.Empty;
            string cursorEnd   = isCursor ? "[/]" : string.Empty;

            int dotCount = Math.Max(1, widthHalf - name.Length - statusLen - (isCursor ? 2 : 0));
            string line = $"{cursorStart}{name}{cursorEnd} {new string('.', dotCount)} {statusMarkup}";
            lines.Add(line);
            idx++;
        }

        _clientsLayout.Update(
            new Panel(new Markup(string.Join('\n', lines)))
                .Header("[bold]Clients[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(_currentFocus == Focus.Clients
                                 ? Style.Parse("yellow")
                                 : Style.Parse("green"))
                .Expand()
        );

        // Push into the live context
        _live?.UpdateTarget(_rootLayout);
        _live?.Refresh();
    }

    private static void UpdateChatLayout()
    {
        var thread = _messages
            .Where(m =>
                (m.From == _ourUser       && m.To == _selectedClient) ||
                (m.From == _selectedClient && m.To == _ourUser))
            .OrderBy(m => m.Time)
            .Select(m => $"[bold]{m.From}[/]: {m.Content}")
            .ToList();

        int visibleLines = (int)(Console.WindowHeight * 0.70) - 4;
        visibleLines = Math.Max(5, visibleLines);

        int start = Math.Max(0, thread.Count - visibleLines - _chatScrollOffset);
        var visible = thread.Skip(start).Take(visibleLines);

        _chatHistoryLayout.Update(
            new Panel(new Markup(string.Join('\n', visible)))
                .Header($"[bold]Chat with {_selectedClient}[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(_currentFocus == Focus.ChatHistory
                                 ? Style.Parse("yellow")
                                 : Style.Parse("cyan"))
                .Expand()
        );

        // Push into the live context
        _live?.UpdateTarget(_rootLayout);
        _live?.Refresh();
    }

    private static void UpdateStatusBar()
    {
        string sent = $"[green]↑[/] Sent: {_sentCount}";
        string recv = $" | [red]↓[/] Received: {_receivedCount}";
        string time = $" Time: {DateTime.Now:HH:mm:ss}";
        int pad = Math.Max(0, Console.WindowWidth - sent.Length - recv.Length - time.Length + 12);

        _statusBarLayout.Update(
            new Panel(new Markup(sent + recv + new string(' ', pad) + time))
                .Border(BoxBorder.Rounded)
                .BorderStyle(Style.Parse("magenta"))
                .Expand()
        );

        // Push into the live context
        _live?.UpdateTarget(_rootLayout);
        _live?.Refresh();
    }

    // --------------------------------------------------------
    //  Input handling
    // --------------------------------------------------------
    private static void InputLoop()
    {
        while (true)
        {
            var keyInfo = Console.ReadKey(intercept: true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    _currentFocus = (Focus)(((int)_currentFocus + 1) % 3);
                    UpdateMessageBox(_messageBox); // reapply focus‐style on message box
                    // Redraw everything so the border color around “Clients” and “Chat” updates
                    lock (_renderLock)
                        RedrawAll();
                    break;

                case ConsoleKey.UpArrow:
                    if (_currentFocus == Focus.Clients && _clientSelectedIndex > 0)
                        _clientSelectedIndex--;
                    else if (_currentFocus == Focus.ChatHistory)
                        _chatScrollOffset++;
                    break;

                case ConsoleKey.DownArrow:
                    if (_currentFocus == Focus.Clients && _clientSelectedIndex < _clients.Count - 1)
                        _clientSelectedIndex++;
                    else if (_currentFocus == Focus.ChatHistory && _chatScrollOffset > 0)
                        _chatScrollOffset--;
                    break;
            }

            if (_currentFocus == Focus.MessageInput && keyInfo.Key == ConsoleKey.Enter)
            {
                if (string.IsNullOrWhiteSpace(_messageBox) 
                    || _messageBox == "Type your message here...")
                {
                    // Ignore empty messages
                    continue;
                }
                // simulate sending a message
                var msg = new Message
                {
                    From    = _ourUser,
                    To      = _selectedClient,
                    Content = _messageBox,
                    Time    = DateTime.Now
                };
                UpdateChat(msg);
                _messageBox = "Type your message here...";
                UpdateMessageBox(_messageBox);
                continue; // skip RedrawAll() because UpdateChat + UpdateMessageBox did it
            }
            if (_currentFocus == Focus.MessageInput)
            {
                // Ignore Tab/Arrows in MessageInput; handle text entry only
                if (keyInfo.Key == ConsoleKey.Tab 
                    || keyInfo.Key == ConsoleKey.UpArrow 
                    || keyInfo.Key == ConsoleKey.DownArrow)
                {
                    continue;
                }
                // If input is empty reset to placeholder
                if (string.IsNullOrWhiteSpace(_messageBox))
                {
                    _messageBox = "Type your message here...";
                    UpdateMessageBox(_messageBox);
                    continue;
                }
                if (keyInfo.Key == ConsoleKey.Backspace
                    && _messageBox.Length > 0
                    && _messageBox != "Type your message here...")
                {
                    _messageBox = _messageBox.Substring(0, _messageBox.Length - 1);
                }
                // ** Only treat KeyChar as new text if it's not Backspace **
                else if (_messageBox == "Type your message here..."
                         && keyInfo.KeyChar != '\0'
                         && keyInfo.KeyChar != '\r'
                         && keyInfo.Key != ConsoleKey.Backspace)
                {
                    _messageBox = "" + keyInfo.KeyChar;
                }
                else if (keyInfo.KeyChar != '\0'
                         && keyInfo.KeyChar != '\r'
                         && keyInfo.Key != ConsoleKey.Backspace)
                {
                    _messageBox += keyInfo.KeyChar;
                }

                UpdateMessageBox(_messageBox);
                continue;
            }

            lock (_renderLock)
                RedrawAll();
        }
    }
}