using Syncfusion.Maui.Chat;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Text.Json;
using AITrackerAgent.Interfaces;
using AITrackerAgent.Classes;

namespace AITrackerAgent
{
    /// <summary>
    /// A ViewModel for GettingStarted sample.
    /// </summary>
    public class AITrackerAgentViewModel : INotifyPropertyChanged
    {
        #region Fields

        private bool isBadgeViewVisible = false;
        private ObservableCollection<object>? messages;
        private bool? showTypingIndicator;
        private ChatTypingIndicator? typingIndicator;
        private IAgentService agentChatbotService;
        private Author agentAuthor = new Author() { Name = "Your AI Assistant" };

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AITrackerAgentViewModel"/> class.
        /// </summary>
        public AITrackerAgentViewModel(IAgentService agentChatbotService)
        {
            this.promptRequestCommand = new Command(this.ExecutePromptRequestCommand, this.CanExecutePromptRequestCommand);
            this.Messages = new ObservableCollection<object>();
            this.TypingIndicator = new ChatTypingIndicator();
            this.agentChatbotService = agentChatbotService;
        }

        #endregion

        #region Public Properties

        public ChatTypingIndicator? TypingIndicator
        {
            get
            {
                return this.typingIndicator;
            }

            private set
            {
                this.typingIndicator = value;
                RaisePropertyChanged("TypingIndicator");
            }
        }

        public bool? ShowTypingIndicator
        {
            get
            {
                return this.showTypingIndicator;
            }

            private set
            {
                this.showTypingIndicator = value;
                RaisePropertyChanged("ShowTypingIndicator");
            }
        }

        public bool IsBadgeViewVisible
        {
            get
            {
                return this.isBadgeViewVisible;
            }

            set
            {
                this.isBadgeViewVisible = value;
                RaisePropertyChanged("IsBadgeViewVisible");
            }
        }

        public ObservableCollection<object>? Messages
        {
            get
            {
                return this.messages;
            }

            set
            {
                this.messages = value;
                RaisePropertyChanged("Messages");
            }
        }

        #endregion

        #region Property Changed

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region Private Methods

        #region Init

        /// <summary>
        /// Initializes the conversation and adds messages.
        /// </summary>
        public async Task StartMessaging()
        {
            ExecutePromptRequest("Give a warm welcome message for the user of this ai agent asking either for the name of the driver or the brand and model of the vehicle or its numberplate");
        }


        /// <summary>
        /// Initializes Position.
        /// </summary>
        public void SetPosition(double latitude, double longitude)
        {
            var localMemory = $"The user current position is latitude = {latitude} and longitude = {longitude} ";
            agentChatbotService.AddLocalMemory(localMemory);
        }

        /// <summary>
        /// Initializes Position.
        /// </summary>
        public async Task CreateChatBot()
        {
            await agentChatbotService.CreateChatBot();
        }


        #endregion

        /// <summary>
        /// Creating message to based on the given string.
        /// </summary>
        /// <param name="text">The text of the new message.</param>
        /// <param name="auth">The author of the new message.</param>
        /// <returns>The <see cref="TextMessage"/> created with the given string.</returns>
        private TextMessage CreateMessage(string text, Author auth)
        {
            if (text.StartsWith("{") && text.EndsWith("}"))
            {
                var bingMapJson = JsonSerializer.Deserialize<BingData>(text);
                if (bingMapJson != null)
                {
                    return new HyperlinkMessage()
                    {
                        DateTime = DateTime.Now,
                        Author = auth,
                        Text = bingMapJson.Address,
                        Url = bingMapJson.BingUrl
                    };
                }
            }
            return new TextMessage()
            {
                DateTime = DateTime.Now,
                Author = auth,
                Text = text,
            };
        }

        /// <summary>
        /// Updates the typing indicator based on the current typing author.
        /// </summary>
        private void AuthorStartedTyping(Author auth)
        {
            this.TypingIndicator!.Authors.Clear();
            this.TypingIndicator.Authors.Add(auth);
            this.TypingIndicator.Text = auth.Name + " is typing ...";

            this.TypingIndicator.AvatarViewType = AvatarViewType.Image;
            this.ShowTypingIndicator = true;
        }

        /// <summary>
        /// Updates the typing indicator based on the current typing author.
        /// </summary>
        private void ClearStartedTyping()
        {
            this.TypingIndicator!.Authors.Clear();
            this.TypingIndicator.Text = string.Empty;

            this.ShowTypingIndicator = false;
        }

        #endregion

        #region Prompt Command
        public ICommand PromptRequestCommand => this.promptRequestCommand;
        private ICommand promptRequestCommand;
        private void ExecutePromptRequestCommand(object arg)
        {
            var msg = (SendMessageEventArgs)arg;
            ExecutePromptRequest(msg.Message?.Text);
        }
        private bool CanExecutePromptRequestCommand(object arg)
        {
            return true;
        }
        #endregion

        #region Command
        private async void ExecutePromptRequest(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                AuthorStartedTyping(agentAuthor);
                try 
                {
                    var response = await agentChatbotService.AddMessage(message);
                    this.Messages!.Add(this.CreateMessage($"{response}", agentAuthor));
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error: {ex.Message}";
                    this.Messages!.Add(this.CreateMessage($"{errorMessage}", agentAuthor));
                    ClearStartedTyping();
                    return;
                }
                ClearStartedTyping();
            }
        }
        #endregion

    }
}