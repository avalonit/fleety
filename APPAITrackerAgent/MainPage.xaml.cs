namespace AITrackerAgent
{
    public partial class MainPage : ContentPage
    {
        public MainPage(AITrackerAgentViewModel trackerViewModel)
        {
            InitializeComponent();
            BindingContext = trackerViewModel;

            this.Loaded += async (s, e) => {
                await trackerViewModel.CreateChatBot();
                
                await trackerViewModel.StartMessaging();

                var location = await Geolocation.Default.GetLastKnownLocationAsync();
                if (location != null)
                    trackerViewModel.SetPosition(location.Latitude, location.Longitude);
            };
        }

        public MainPage()
        {
            InitializeComponent();

        }

        

    }

}
