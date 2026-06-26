using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberBot3
{//start of namespace
    public partial class MainWindow : Window
    {//start of class
        
        //  PART 2 DATA (keyword responses + ignore list)
        
        private ArrayList _reply  = new ArrayList();
        private ArrayList _ignore = new ArrayList();
        private string    _username = string.Empty;
        private int       _chatCounter = 0;

        
        //  TASK 1  Task Assistant
        
        private class CyberTask
        {//start of CyberTask class
            public int    Id          { get; set; }
            public string Title       { get; set; }
            public string Description { get; set; }
            public string Reminder    { get; set; }
            public bool   IsComplete  { get; set; }
            public DateTime CreatedAt { get; set; }

        }//end of CyberTask class
        private List<CyberTask> _tasks   = new List<CyberTask>();
        private int             _nextId  = 1;

        
        //  TASK 2 – Quiz
        
        private class QuizQuestion
        {//start of QuizQuestion class
            public string   Question { get; set; }
            public string[] Options  { get; set; }   // null for T/F
            public string   Answer   { get; set; }   // "True"/"False" or option text
            public string   Explanation { get; set; }
        }//end of QuizQuestion class

        private List<QuizQuestion> _questions;
        private int  _qIndex    = 0;
        private int  _score     = 0;
        private int  _answered  = 0;
        private bool _quizActive = false;
        private bool _awaitingNext = false;

        // ══════════════════════════════════════════════════════════════
        //  TASK 4 – Activity Log
        // ══════════════════════════════════════════════════════════════
        private List<string> _activityLog = new List<string>();

        // ══════════════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ══════════════════════════════════════════════════════════════
        public MainWindow()
        {//start of MainWindow constructor
            InitializeComponent();
            LoadIgnoreWords();
            LoadReplies();
            LoadQuizQuestions();

            // Play the Part 2 audio greeting on startup
            var greeting = new voice_greeting();
            greeting.greet();
        }//end of MainWindow constructor

        
        //  NAVIGATION BUTTONS
        
        private void HomeStartBtn_Click(object sender, RoutedEventArgs e)
        {//start of HomeStartBtn_Click
            HomeGrid.Visibility     = Visibility.Hidden;
            UsernameGrid.Visibility = Visibility.Visible;
        }//end of HomeStartBtn_Click

        private void SubmitNameBtn_Click(object sender, RoutedEventArgs e)
        {//start of SubmitNameBtn_Click
            string name = UsernameInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {//start of if
                UsernameErrorText.Text = "Please enter your name.";
                return;
            }//end of if

            _username = name;
            HeaderUsername.Text     = name;
            UsernameGrid.Visibility = Visibility.Hidden;
            MainGrid.Visibility     = Visibility.Visible;

            AppendChat("CyberBot",
                $"Hello {_username}! Welcome to CyberBot 3.0 🛡\n" +
                "You can:\n" +
                "• Chat with me about cybersecurity\n" +
                "• Manage tasks in the 📋 Tasks tab\n" +
                "• Test your knowledge in the 🎮 Quiz tab\n" +
                "• View your history in the 📜 Activity Log tab\n\n" +
                "Type 'show activity log' or 'what have you done for me?' to see recent actions.\n" +
                "Type 'help' to see all commands.");

            LogAction("Session started by " + _username);
        }//end of SubmitNameBtn_Click

        private void UsernameInput_KeyDown(object sender, KeyEventArgs e)
        {//start of UsernameInput_KeyDown
            if (e.Key == Key.Enter) SubmitNameBtn_Click(sender, e);
        }//end of UsernameInput_KeyDown

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {//start of ExitBtn_Click
            AppendChat("CyberBot", $"Goodbye {_username}! Stay safe online 🛡");
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                Dispatcher.Invoke(() => Application.Current.Shutdown()));
        }//end of ExitBtn_Click

        // ══════════════════════════════════════════════════════════════
        //  TASK 3 – NLP / CHAT HANDLER
        // ══════════════════════════════════════════════════════════════
        private void SendBtn_Click(object sender, RoutedEventArgs e) => ProcessChat();

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {//start of MessageInput_KeyDown
            if (e.Key == Key.Enter) ProcessChat();
        }//end of MessageInput_KeyDown

        private void ProcessChat()
        {//start of ProcessChat
            string raw = MessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;

            AppendChat(_username, raw);
            MessageInput.Clear();

            string lower = raw.ToLower();

            // ── Activity log commands ──────────────────────────────
            if (lower.Contains("show activity log") ||
                lower.Contains("what have you done for me") ||
                lower.Contains("activity log") ||
                lower.Contains("recent actions"))
            {//start of if
                ShowActivityLogInChat();
                LogAction("User requested activity log");
                return;
            }// end of if

            // ── Help command ───────────────────────────────────────
            if (lower == "help" || lower.Contains("what can you do"))
            {//start of if
                AppendChat("CyberBot",
                    "Here's what I can help you with:\n" +
                    "• Ask any cybersecurity question (passwords, phishing, VPN…)\n" +
                    "• Type 'add task <title>' to add a task\n" +
                    "• Type 'view tasks' to list your tasks\n" +
                    "• Type 'start quiz' to launch the quiz\n" +
                    "• Type 'show activity log' to see recent actions\n" +
                    "• Type 'exit' to close the app");
                return;
            }// end of if

            // ── Exit ───────────────────────────────────────────────
            if (lower == "exit" || lower == "quit" || lower == "bye")
            {//start of if
                ExitBtn_Click(null, null);
                return;
            }// end of if

            // ── NLP: Task operations ───────────────────────────────
            if (NlpIsAddTask(lower))
            {//start of if
                string title = ExtractTaskTitle(raw);
                if (!string.IsNullOrWhiteSpace(title))
                {//start of if
                    QuickAddTask(title);
                    AppendChat("CyberBot",
                        $"Task added: '{title}'\n" +
                        "Would you like a reminder? Go to the 📋 Tasks tab to add one, " +
                        "or type: remind me in 3 days for <task name>");
                    LogAction($"Task added via chat: '{title}'");
                }// end of if
                else
                {//start of else
                    AppendChat("CyberBot",
                        "I noticed you want to add a task! Please use the 📋 Tasks tab " +
                        "or type: add task <title>");
                }// end of else
                return;
            }// end of if

            // ── NLP: View tasks ────────────────────────────────────
            if (lower.Contains("view task") || lower.Contains("show task") ||
                lower.Contains("list task") || lower.Contains("my task"))
            {
                ShowTasksInChat();
                return;
            }

            // ── NLP: Start quiz ────────────────────────────────────
            if (lower.Contains("start quiz") || lower.Contains("take quiz") ||
                lower.Contains("play quiz") || lower.Contains("quiz me"))
            {
                AppendChat("CyberBot", "Head over to the 🎮 Quiz tab to start your quiz! 🎯");
                LogAction("User directed to quiz via chat");
                return;
            }

            // ── NLP: Reminder ──────────────────────────────────────
            if (NlpIsReminder(lower))
            {
                string reminder = ExtractReminderInfo(raw);
                AppendChat("CyberBot",
                    $"Got it! Reminder noted: '{reminder}'\n" +
                    "You can also set reminders in the 📋 Tasks tab.");
                LogAction($"Reminder set via chat: '{reminder}'");
                return;
            }

            // ── Keyword-based cybersecurity response (Part 2 logic) ─
            string cleaned = SanitiseInput(raw);
            string response = GetKeywordResponse(cleaned);
            AppendChat("CyberBot", response);
            LogAction($"NLP chat: user asked about '{cleaned.Split(' ').FirstOrDefault()}'");
        }

        // NLP detection helpers
        private bool NlpIsAddTask(string lower)
        {
            return (lower.Contains("add task") || lower.Contains("add a task") ||
                    lower.Contains("create task") || lower.Contains("new task") ||
                    lower.Contains("add reminder") || lower.Contains("set task") ||
                    (lower.Contains("add") && (lower.Contains("2fa") || lower.Contains("password") ||
                     lower.Contains("authentication") || lower.Contains("privacy") ||
                     lower.Contains("firewall") || lower.Contains("backup"))));
        }

        private bool NlpIsReminder(string lower)
        {
            return (lower.Contains("remind me") || lower.Contains("set a reminder") ||
                    lower.Contains("reminder for") || lower.Contains("remind me to"));
        }

        private string ExtractTaskTitle(string raw)
        {
            // Remove common prefixes
            string[] prefixes = { "add task", "create task", "new task", "add a task",
                                   "set task", "add reminder" };
            string result = raw.Trim();
            foreach (string p in prefixes)
            {
                if (result.ToLower().StartsWith(p))
                {
                    result = result.Substring(p.Length).Trim().TrimStart('-', ':').Trim();
                    break;
                }
            }
            // If it contains "add" + keyword, infer the task
            if (result.ToLower().Contains("2fa") || result.ToLower().Contains("two-factor"))
                return "Enable two-factor authentication";
            return result;
        }

        private string ExtractReminderInfo(string raw)
        {
            // Try to pull out "remind me in X days for Y" style
            var match = Regex.Match(raw, @"remind\s+me\s+(in\s+\d+\s+\w+|tomorrow|on\s+\S+)",
                                    RegexOptions.IgnoreCase);
            return match.Success ? match.Value : raw;
        }

        private void ShowTasksInChat()
        {
            if (_tasks.Count == 0)
            {
                AppendChat("CyberBot", "You have no tasks yet. Use the 📋 Tasks tab or type 'add task <title>'.");
                return;
            }
            var sb = new StringBuilder("Here are your current tasks:\n");
            foreach (var t in _tasks)
            {
                string status = t.IsComplete ? "✔ Done" : "⏳ Pending";
                sb.AppendLine($"  {t.Id}. [{status}] {t.Title}");
                if (!string.IsNullOrWhiteSpace(t.Reminder))
                    sb.AppendLine($"     Reminder: {t.Reminder}");
            }
            AppendChat("CyberBot", sb.ToString().TrimEnd());
            LogAction("User viewed tasks via chat");
        }

        private void ShowActivityLogInChat()
        {
            if (_activityLog.Count == 0)
            {
                AppendChat("CyberBot", "No activity recorded yet.");
                return;
            }
            var recent = _activityLog.Skip(Math.Max(0, _activityLog.Count - 10)).ToList();
            var sb = new StringBuilder("Here's a summary of recent actions:\n");
            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"  {i + 1}. {recent[i]}");
            AppendChat("CyberBot", sb.ToString().TrimEnd());
        }

        // ══════════════════════════════════════════════════════════════
        //  TASK 1 – TASK ASSISTANT BUTTONS
        // ══════════════════════════════════════════════════════════════
        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            string title    = TaskTitleInput.Text.Trim();
            string desc     = TaskDescInput.Text.Trim();
            string reminder = TaskReminderInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "CyberBot",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new CyberTask
            {
                Id          = _nextId++,
                Title       = title,
                Description = string.IsNullOrWhiteSpace(desc)
                              ? AutoDescription(title)
                              : desc,
                Reminder    = reminder,
                IsComplete  = false,
                CreatedAt   = DateTime.Now
            };

            _tasks.Add(task);
            RefreshTaskList();

            string logMsg = $"Task added: '{title}'" +
                            (string.IsNullOrWhiteSpace(reminder) ? "" : $" (Reminder: {reminder})");
            LogAction(logMsg);

            // Clear inputs
            TaskTitleInput.Clear();
            TaskDescInput.Clear();
            TaskReminderInput.Clear();

            MessageBox.Show(
                $"Task '{title}' added!\n" +
                (string.IsNullOrWhiteSpace(reminder) ? "No reminder set." : $"Reminder: {reminder}"),
                "CyberBot – Task Added", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string AutoDescription(string title)
        {
            // Provide smart auto-descriptions for common security tasks
            string t = title.ToLower();
            if (t.Contains("password"))    return "Update and strengthen your account passwords to prevent unauthorised access.";
            if (t.Contains("2fa") || t.Contains("two-factor")) return "Enable two-factor authentication to add an extra layer of security.";
            if (t.Contains("privacy"))     return "Review account privacy settings to ensure your data is protected.";
            if (t.Contains("firewall"))    return "Check and configure your firewall rules to block unwanted traffic.";
            if (t.Contains("backup"))      return "Back up important data to prevent loss from attacks or failure.";
            if (t.Contains("phishing"))    return "Learn to identify phishing emails and report suspicious messages.";
            if (t.Contains("antivirus") || t.Contains("anti-virus")) return "Update and run your antivirus software to detect threats.";
            if (t.Contains("vpn"))         return "Set up a VPN to secure your internet connection, especially on public Wi-Fi.";
            if (t.Contains("update") || t.Contains("patch")) return "Apply the latest security patches and software updates.";
            return "Complete this cybersecurity task to improve your online safety.";
        }

        private void RefreshTaskList()
        {
            TaskListBox.Items.Clear();
            foreach (var task in _tasks)
            {
                // Build a card for each task
                Border card = new Border
                {
                    Background      = new SolidColorBrush(Color.FromRgb(62, 39, 35)),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(255, 143, 0)),
                    BorderThickness = new Thickness(1),
                    CornerRadius    = new CornerRadius(6),
                    Margin          = new Thickness(0, 0, 0, 6),
                    Padding         = new Thickness(10, 8, 10, 8)
                };

                Grid cardGrid = new Grid();
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Task text
                StackPanel textStack = new StackPanel();

                TextBlock titleBlock = new TextBlock
                {
                    Text       = (task.IsComplete ? "✔ " : "⏳ ") + task.Title,
                    FontSize   = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = task.IsComplete
                        ? new SolidColorBrush(Colors.LightGreen)
                        : new SolidColorBrush(Color.FromRgb(255, 224, 178)),
                    TextWrapping = TextWrapping.Wrap
                };

                TextBlock descBlock = new TextBlock
                {
                    Text         = task.Description,
                    FontSize     = 12,
                    Foreground   = new SolidColorBrush(Color.FromRgb(239, 235, 233)),
                    TextWrapping = TextWrapping.Wrap,
                    Margin       = new Thickness(0, 3, 0, 0)
                };

                textStack.Children.Add(titleBlock);
                textStack.Children.Add(descBlock);

                if (!string.IsNullOrWhiteSpace(task.Reminder))
                {
                    TextBlock remBlock = new TextBlock
                    {
                        Text       = "🔔 Reminder: " + task.Reminder,
                        FontSize   = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 143, 0)),
                        Margin     = new Thickness(0, 3, 0, 0)
                    };
                    textStack.Children.Add(remBlock);
                }

                Grid.SetColumn(textStack, 0);

                // Buttons
                StackPanel btnStack = new StackPanel
                {
                    Orientation       = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin            = new Thickness(8, 0, 0, 0)
                };

                if (!task.IsComplete)
                {
                    Button doneBtn = CreateCardButton("✔ Done", "#2E7D32", task.Id, CompleteTask_Click);
                    btnStack.Children.Add(doneBtn);
                }

                Button delBtn = CreateCardButton("🗑 Delete", "#C62828", task.Id, DeleteTask_Click);
                delBtn.Margin = new Thickness(0, 4, 0, 0);
                btnStack.Children.Add(delBtn);

                Grid.SetColumn(btnStack, 1);

                cardGrid.Children.Add(textStack);
                cardGrid.Children.Add(btnStack);
                card.Child = cardGrid;

                TaskListBox.Items.Add(card);
            }
        }

        private Button CreateCardButton(string label, string hexColor, int taskId, RoutedEventHandler handler)
        {
            var color = (Color)ColorConverter.ConvertFromString(hexColor);
            var btn = new Button
            {
                Content    = label,
                Tag        = taskId,
                Width      = 80,
                Height     = 28,
                FontSize   = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(color),
                BorderThickness = new Thickness(0),
                Cursor     = Cursors.Hand
            };
            btn.Template = RoundedButtonTemplate(color);
            btn.Click   += handler;
            return btn;
        }

        private ControlTemplate RoundedButtonTemplate(Color bg)
        {
            // Build a rounded template programmatically
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new SolidColorBrush(bg));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            border.SetValue(Border.PaddingProperty, new Thickness(6, 2, 6, 2));
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(presenter);
            template.VisualTree = border;
            return template;
        }

        private void QuickAddTask(string title)
        {
            _tasks.Add(new CyberTask
            {
                Id          = _nextId++,
                Title       = title,
                Description = AutoDescription(title),
                Reminder    = string.Empty,
                IsComplete  = false,
                CreatedAt   = DateTime.Now
            });
            RefreshTaskList();
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            int id   = (int)((Button)sender).Tag;
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                task.IsComplete = true;
                RefreshTaskList();
                LogAction($"Task completed: '{task.Title}'");
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            int id   = (int)((Button)sender).Tag;
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
                RefreshTaskList();
                LogAction($"Task deleted: '{task.Title}'");
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  TASK 2 – QUIZ
        // ══════════════════════════════════════════════════════════════
        private void LoadQuizQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                // Multiple choice
                new QuizQuestion {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options  = new[] { "A) Reply with your password", "B) Delete the email",
                                       "C) Report it as phishing", "D) Ignore it" },
                    Answer      = "C) Report it as phishing",
                    Explanation = "Reporting phishing emails helps prevent scams and alerts your email provider." },

                new QuizQuestion {
                    Question = "Which of the following is the STRONGEST password?",
                    Options  = new[] { "A) password123", "B) MyName1990",
                                       "C) qwerty", "D) !T7#mK9$pL2&" },
                    Answer      = "D) !T7#mK9$pL2&",
                    Explanation = "A strong password uses a mix of upper/lower case, numbers and symbols with no dictionary words." },

                new QuizQuestion {
                    Question = "What does HTTPS in a URL indicate?",
                    Options  = new[] { "A) The site is fast", "B) The connection is encrypted",
                                       "C) The site is popular", "D) The site has no viruses" },
                    Answer      = "B) The connection is encrypted",
                    Explanation = "HTTPS means data between your browser and the site is encrypted using TLS/SSL." },

                new QuizQuestion {
                    Question = "What is two-factor authentication (2FA)?",
                    Options  = new[] { "A) Using two passwords", "B) Logging in from two devices",
                                       "C) Verifying identity with two methods", "D) Having two email accounts" },
                    Answer      = "C) Verifying identity with two methods",
                    Explanation = "2FA adds a second verification step (e.g. SMS code) beyond your password for extra security." },

                new QuizQuestion {
                    Question = "Which is a sign of a phishing website?",
                    Options  = new[] { "A) HTTPS in the URL", "B) A padlock icon",
                                       "C) Misspelled domain names", "D) Fast loading speed" },
                    Answer      = "C) Misspelled domain names",
                    Explanation = "Phishing sites often use slightly misspelled domains (e.g. 'paypa1.com') to fool users." },

                new QuizQuestion {
                    Question = "What is a VPN primarily used for?",
                    Options  = new[] { "A) Speeding up the internet", "B) Encrypting internet traffic and hiding your IP",
                                       "C) Blocking ads", "D) Downloading files faster" },
                    Answer      = "B) Encrypting internet traffic and hiding your IP",
                    Explanation = "A VPN creates an encrypted tunnel for your traffic, protecting privacy especially on public Wi-Fi." },

                new QuizQuestion {
                    Question = "What does malware stand for?",
                    Options  = new[] { "A) Malicious Software", "B) Managed Layered Ware",
                                       "C) Multiple Access Layer", "D) Manual Layer Reset" },
                    Answer      = "A) Malicious Software",
                    Explanation = "Malware is any software designed to disrupt, damage or gain unauthorised access to systems." },

                new QuizQuestion {
                    Question = "How often should you update your software and operating system?",
                    Options  = new[] { "A) Never – it breaks things", "B) Only when something breaks",
                                       "C) Regularly and as soon as updates are available", "D) Once a year" },
                    Answer      = "C) Regularly and as soon as updates are available",
                    Explanation = "Updates patch security vulnerabilities; delaying them leaves your system exposed." },

                // True / False
                new QuizQuestion {
                    Question    = "TRUE or FALSE: It is safe to use the same password for all your accounts.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "Reusing passwords means one breach exposes ALL your accounts. Use a unique password per site." },

                new QuizQuestion {
                    Question    = "TRUE or FALSE: Public Wi-Fi networks are generally safe for online banking.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "Public Wi-Fi is unsecured. Attackers can intercept traffic. Use a VPN or mobile data for banking." },

                new QuizQuestion {
                    Question    = "TRUE or FALSE: A firewall can help block unauthorised access to your computer.",
                    Options     = null,
                    Answer      = "True",
                    Explanation = "A firewall monitors and controls incoming/outgoing network traffic based on security rules." },

                new QuizQuestion {
                    Question    = "TRUE or FALSE: Clicking 'Unsubscribe' in a spam email is always safe.",
                    Options     = null,
                    Answer      = "False",
                    Explanation = "Clicking unsubscribe in spam can confirm your email is active, leading to more spam or malware." },

                new QuizQuestion {
                    Question    = "TRUE or FALSE: Social engineering attacks target human psychology rather than technology.",
                    Options     = null,
                    Answer      = "True",
                    Explanation = "Social engineering manipulates people into revealing confidential information rather than hacking systems directly." },
            };

            // Shuffle questions
            Shuffle(_questions);
        }

        private void Shuffle<T>(List<T> list)
        {
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T temp = list[i]; list[i] = list[j]; list[j] = temp;
            }
        }

        private void StartQuizBtn_Click(object sender, RoutedEventArgs e)
        {
            _qIndex     = 0;
            _score      = 0;
            _answered   = 0;
            _quizActive = true;
            Shuffle(_questions);

            StartQuizBtn.Content    = "🔄 Restart";
            QuizScoreText.Text      = "Score: 0 / 0";
            QuizFeedbackBorder.Visibility = Visibility.Collapsed;
            QuizNextBtn.Visibility        = Visibility.Collapsed;

            LogAction("Quiz started");
            ShowQuestion();
        }

        private void ShowQuestion()
        {
            if (_qIndex >= _questions.Count)
            {
                EndQuiz();
                return;
            }

            var q = _questions[_qIndex];
            QuizQuestionNum.Text    = $"🎮 Question {_qIndex + 1} of {_questions.Count}";
            QuizQuestionText.Text   = q.Question;
            QuizQuestionBorder.Visibility = Visibility.Visible;
            QuizFeedbackBorder.Visibility = Visibility.Collapsed;
            QuizNextBtn.Visibility        = Visibility.Collapsed;
            _awaitingNext = false;

            if (q.Options != null)
            {
                // Multiple choice
                QuizOptionsPanel.Visibility = Visibility.Visible;
                QuizTFPanel.Visibility      = Visibility.Collapsed;

                var opts = new[] { QuizOpt1, QuizOpt2, QuizOpt3, QuizOpt4 };
                for (int i = 0; i < opts.Length; i++)
                {
                    if (i < q.Options.Length)
                    {
                        opts[i].Content    = q.Options[i];
                        opts[i].IsEnabled  = true;
                        opts[i].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        opts[i].Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                // True / False
                QuizOptionsPanel.Visibility = Visibility.Collapsed;
                QuizTFPanel.Visibility      = Visibility.Visible;
                QuizTrueBtn.IsEnabled  = true;
                QuizFalseBtn.IsEnabled = true;
            }
        }

        private void QuizOpt_Click(object sender, RoutedEventArgs e)
        {
            if (_awaitingNext) return;
            string chosen = ((Button)sender).Content.ToString();
            EvaluateAnswer(chosen);
        }

        private void QuizTF_Click(object sender, RoutedEventArgs e)
        {
            if (_awaitingNext) return;
            string chosen = ((Button)sender).Content.ToString().Contains("True") ? "True" : "False";
            EvaluateAnswer(chosen);
        }

        private void EvaluateAnswer(string chosen)
        {
            _awaitingNext = true;
            var q = _questions[_qIndex];
            bool correct = chosen.Trim().Equals(q.Answer.Trim(), StringComparison.OrdinalIgnoreCase);

            if (correct) _score++;
            _answered++;

            QuizScoreText.Text = $"Score: {_score} / {_answered}";

            QuizFeedbackBorder.Background = correct
                ? new SolidColorBrush(Color.FromRgb(27, 94, 32))
                : new SolidColorBrush(Color.FromRgb(183, 28, 28));

            QuizFeedbackText.Text = correct
                ? $"✔ Correct! {q.Explanation}"
                : $"✖ Incorrect. The correct answer is: {q.Answer}\n{q.Explanation}";

            QuizFeedbackBorder.Visibility = Visibility.Visible;

            // Disable buttons
            foreach (Button b in new[] { QuizOpt1, QuizOpt2, QuizOpt3, QuizOpt4 })
                b.IsEnabled = false;
            QuizTrueBtn.IsEnabled  = false;
            QuizFalseBtn.IsEnabled = false;

            QuizNextBtn.Visibility = Visibility.Visible;
        }

        private void QuizNextBtn_Click(object sender, RoutedEventArgs e)
        {
            _qIndex++;
            ShowQuestion();
        }

        private void EndQuiz()
        {
            QuizQuestionBorder.Visibility = Visibility.Collapsed;
            QuizOptionsPanel.Visibility   = Visibility.Collapsed;
            QuizTFPanel.Visibility        = Visibility.Collapsed;
            QuizNextBtn.Visibility        = Visibility.Collapsed;

            int pct = _questions.Count > 0 ? (_score * 100) / _questions.Count : 0;

            string grade;
            if (pct >= 90)      grade = "🏆 Outstanding! You're a cybersecurity pro!";
            else if (pct >= 70) grade = "🥈 Great job! You know your cybersecurity!";
            else if (pct >= 50) grade = "🥉 Good effort! Keep learning to stay safe online!";
            else                grade = "📚 Keep learning – cybersecurity is important!";

            QuizFeedbackBorder.Background = new SolidColorBrush(Color.FromRgb(62, 39, 35));
            QuizFeedbackText.Text =
                $"Quiz Complete!\n\nFinal Score: {_score} / {_questions.Count} ({pct}%)\n\n{grade}";
            QuizFeedbackBorder.Visibility = Visibility.Visible;

            QuizQuestionNum.Text = "🎮 Quiz Finished";

            LogAction($"Quiz completed – Score: {_score}/{_questions.Count} ({pct}%)");
            _quizActive = false;
        }

        // ══════════════════════════════════════════════════════════════
        //  TASK 4 – ACTIVITY LOG
        // ══════════════════════════════════════════════════════════════
        private void LogAction(string description)
        {
            string entry = $"[{DateTime.Now:HH:mm:ss}] {description}";
            _activityLog.Add(entry);
            RefreshActivityLog();
        }

        private void RefreshActivityLog()
        {
            ActivityLogListBox.Items.Clear();
            // Show last 10
            var recent = _activityLog.Skip(Math.Max(0, _activityLog.Count - 10)).ToList();
            for (int i = recent.Count - 1; i >= 0; i--)  // newest first
            {
                Border entry = new Border
                {
                    Background      = new SolidColorBrush(Color.FromRgb(62, 39, 35)),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(141, 110, 99)),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding         = new Thickness(10, 6, 10, 6)
                };

                TextBlock tb = new TextBlock
                {
                    Text         = recent[i],
                    Foreground   = new SolidColorBrush(Color.FromRgb(239, 235, 233)),
                    FontSize     = 12,
                    TextWrapping = TextWrapping.Wrap
                };

                entry.Child = tb;
                ActivityLogListBox.Items.Add(entry);
            }
        }

        private void ClearLogBtn_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Clear();
            ActivityLogListBox.Items.Clear();
        }

        // ══════════════════════════════════════════════════════════════
        //  PART 2 CHAT DISPLAY + KEYWORD RESPONSE ENGINE
        // ══════════════════════════════════════════════════════════════
        private void AppendChat(string sender, string message)
        {
            bool isBot = sender.ToLower().Contains("cyberbot") ||
                         sender.ToLower().Contains("chatbot");

            Border bubble = new Border
            {
                Margin          = new Thickness(0, 4, 0, 4),
                Padding         = new Thickness(12, 8, 12, 8),
                CornerRadius    = new CornerRadius(8),
                Background      = isBot
                    ? new SolidColorBrush(Color.FromRgb(78, 52, 46))
                    : new SolidColorBrush(Color.FromRgb(109, 76, 65)),
                BorderBrush     = isBot
                    ? new SolidColorBrush(Color.FromRgb(255, 143, 0))
                    : new SolidColorBrush(Color.FromRgb(141, 110, 99)),
                BorderThickness = new Thickness(1)
            };

            TextBlock tb = new TextBlock { TextWrapping = TextWrapping.Wrap };
            tb.Inlines.Add(new Run
            {
                Text       = sender + ": ",
                FontWeight = FontWeights.Bold,
                Foreground = isBot
                    ? new SolidColorBrush(Color.FromRgb(255, 143, 0))
                    : new SolidColorBrush(Color.FromRgb(255, 224, 178))
            });
            tb.Inlines.Add(new Run
            {
                Text       = message,
                Foreground = new SolidColorBrush(Color.FromRgb(239, 235, 233))
            });

            bubble.Child = tb;
            ChatListBox.Items.Add(bubble);
            ChatListBox.ScrollIntoView(ChatListBox.Items[ChatListBox.Items.Count - 1]);
        }

        private string GetKeywordResponse(string cleaned)
        {
            if (string.IsNullOrWhiteSpace(cleaned))
                return "I could not understand that. Could you rephrase?";

            string[] words = cleaned.ToLower().Split(
                new[] { ' ', ',', '.', '?', '!', ';', ':' },
                StringSplitOptions.RemoveEmptyEntries);

            // Sentiment detection first
            string sentiment = DetectSentiment(words);
            if (!string.IsNullOrEmpty(sentiment)) return sentiment;

            // Score keyword matches
            var scores = new Dictionary<string, int>();
            foreach (string word in words)
            {
                if (word.Length < 3 || _ignore.Contains(word)) continue;
                foreach (string answer in _reply)
                {
                    if (answer.ToLower().Contains(word))
                    {
                        if (scores.ContainsKey(answer)) scores[answer]++;
                        else scores[answer] = 1;
                    }
                }
            }

            if (scores.Count > 0)
            {
                string best = scores.OrderByDescending(kv => kv.Value).First().Key;

                // Strip keyword prefix that was used for matching
                string clean = Regex.Replace(best, @"^\w+\s+", "").Trim();
                // Capitalise first letter
                if (clean.Length > 0)
                    clean = char.ToUpper(clean[0]) + clean.Substring(1);

                _chatCounter++;
                if (_chatCounter % 3 == 0)
                    clean += "\n\n💡 Remember: always keep your software updated and use strong passwords!";

                return clean;
            }

            string[] fallback = {
                "I'm not sure about that. Could you rephrase your question?",
                "Try asking about passwords, phishing, VPNs or firewalls.",
                "I didn't quite understand that. Could you rephrase?",
                "Ask me anything about cybersecurity – I'm here to help!"
            };

            return fallback[new Random().Next(fallback.Length)];
        }

        private string DetectSentiment(string[] words)
        {
            var positives = new HashSet<string> { "happy", "great", "good", "excellent", "awesome" };
            var negatives = new HashSet<string> { "sad", "upset", "unhappy", "bad" };
            var frustrated = new HashSet<string> { "frustrated", "annoyed", "angry", "mad", "furious" };
            var worried = new HashSet<string> { "worried", "scared", "afraid", "anxious", "concern", "nervous" };
            var confused = new HashSet<string> { "confused", "lost", "unsure", "don't understand", "unclear" };

            foreach (string w in words)
            {
                if (positives.Contains(w))   return "That's great to hear! 😊 How can I help you stay safe online today?";
                if (negatives.Contains(w))   return "I'm sorry to hear that. 😔 I'm here to help you with any cybersecurity concerns.";
                if (frustrated.Contains(w))  return "I understand you're frustrated. Let's work through this step by step together.";
                if (worried.Contains(w))     return "It's okay to feel worried about online safety. Let me help you stay protected! 🛡";
                if (confused.Contains(w))    return "No worries! I'll explain everything clearly. What would you like to know about cybersecurity?";
            }
            return null;
        }

        private string SanitiseInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in input)
                sb.Append(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-' ? c : ' ');
            return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        }

        // ══════════════════════════════════════════════════════════════
        //  DATA LOADERS (Part 2 replies + ignore list)
        // ══════════════════════════════════════════════════════════════
        private void LoadReplies()
        {
            // Each reply starts with a keyword prefix for matching
            string[] replies = {
                "greeting i'm doing well, thanks for asking! how can i help you with cybersecurity today?",
                "greeting great to chat with you! what cybersecurity topic can i help with?",
                "purpose my purpose is to educate you on how to stay safe online and answer cybersecurity questions.",
                "purpose i help users understand online safety and digital protection strategies.",
                "cybersecurity cybersecurity is about protecting systems, networks and data from digital threats.",
                "cybersecurity it involves securing devices and online accounts from attackers.",
                "phishing phishing is a scam where attackers pretend to be trusted sources to steal your information.",
                "phishing phishing uses fake emails or websites to trick users into revealing sensitive data.",
                "phishing never click suspicious links in emails – verify the sender before acting.",
                "firewall a firewall controls network traffic based on security rules to block threats.",
                "firewall it acts as a protective barrier between trusted and untrusted networks.",
                "password a password should be long, complex and unique for each of your accounts.",
                "password use a password manager to generate and store strong passwords safely.",
                "password never share your password with anyone, including support staff.",
                "hacked immediately change your password and enable two-factor authentication if hacked.",
                "hacked contact support and log out from all devices if your account is compromised.",
                "fraud contact your bank immediately if you suspect fraudulent activity on your account.",
                "fraud report suspicious financial activity to the relevant authorities right away.",
                "malware malware is malicious software designed to damage or gain unauthorised access to systems.",
                "malware keep your antivirus updated to detect and remove malware threats.",
                "vpn a vpn encrypts your internet traffic and hides your ip address for privacy.",
                "vpn always use a vpn on public wi-fi to protect your data from eavesdroppers.",
                "encryption encryption converts data into a coded format so only authorised parties can read it.",
                "scam if something seems too good to be true online, it is probably a scam.",
                "privacy review your privacy settings regularly to control what data apps collect about you.",
                "update keep your software and operating system updated to patch security vulnerabilities.",
                "antivirus install reputable antivirus software and keep it updated for best protection.",
                "backup regularly back up your important data to protect it from ransomware and hardware failure.",
                "authentication two-factor authentication adds an extra layer of security beyond your password.",
                "social social engineering attacks exploit human psychology to trick people into revealing information.",
                "ransomware ransomware encrypts your files and demands payment – always keep backups to recover.",
                "otp never share your one-time password with anyone, even if they claim to be support.",
                "frustrated i understand you're frustrated. let's work through the issue step by step.",
                "confused that's okay, let me explain it clearly for you.",
                "worried don't panic – most cybersecurity issues can be fixed quickly. i'm here to help.",
                "happy great to hear! positivity is always welcome. let me know if you need help.",
                "sad i'm sorry you're feeling this way. i'm here for you anytime.",
                "angry i understand. let's stay calm and solve the problem together."
            };

            foreach (string r in replies) _reply.Add(r);
        }

        private void LoadIgnoreWords()
        {
            string[] stops = {
                "a","about","above","after","again","against","all","almost","also","although",
                "always","am","an","and","another","any","are","around","as","at","be","because",
                "been","before","being","below","between","both","but","by","can","could","did",
                "do","does","doing","done","down","during","each","either","else","enough","even",
                "ever","every","for","from","further","had","has","have","having","he","her","here",
                "him","his","how","however","i","if","in","into","is","it","its","itself",
                "just","last","later","least","let","like","lot","many","may","me","more",
                "most","much","must","my","neither","never","no","nor","not","now","of","off",
                "often","on","once","one","only","or","other","our","out","over","own","per",
                "please","put","quite","rather","re","same","see","several","she","should","so",
                "some","still","such","than","that","the","their","them","then","there","these",
                "they","this","those","though","through","thus","to","together","too","toward",
                "under","unless","until","up","us","used","very","via","was","we","well","were",
                "what","whatever","when","where","whether","which","while","who","why","will",
                "with","within","would","yes","yet","you","your","yours","hey","hi"
            };
            foreach (string s in stops) _ignore.Add(s);
        }
    }
}
