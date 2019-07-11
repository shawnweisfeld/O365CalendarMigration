using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace CalendarTest2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private string ClientId = "<<guid from Azure AD>>";
        private string Tenant = "<<guid from Azure AD>>";
        //NOTE: you will need to create an app in AAD and give it permissions to the Calendar and Group scopes

        private string SourceCalendarUser = "<<email of the user that owns the source calendar>>";
        private string SourceCalendar = "<<big long string with the ID of the source calendar, i got this using the graph explorer>>";
        private string DestCalendar = "<<guid of the dest calendar, i go tthis using the graph explorer>>";


        private GraphServiceClient GraphClient = null;

        public MainWindow()
        {
            InitializeComponent();

            var app = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, Tenant)
                .Build();

            var authProvider = new InteractiveAuthenticationProvider(app);

            GraphClient = new GraphServiceClient(authProvider);

        }

        private async void ListSourceEvents_Click(object sender, RoutedEventArgs e)
        {
            WriteLine(" -- Searching --");
            var items = await GraphClient.Users[SourceCalendarUser]
                  .Calendars[SourceCalendar]
                  .Events
                  .Request().GetAsync();

            int i = 0;


            while (items != null)
            {
                foreach (var item in items)
                {
                    string message = $"{++i}) {item.Subject.Trim()} ({item.Start.DateTime} - {item.End.DateTime})";
                    WriteLine(message);
                }

                var npr = items.NextPageRequest;
                if (npr == null)
                {
                    items = null;
                }
                else
                {
                    WriteLine(" -- Next page --");
                    items = await npr.GetAsync();
                }
            }

            WriteLine(" -- Complete --");
        }

        private void WriteLine(string message)
        {
            Dispatcher.Invoke(() =>
            {
                Output.Text = $"{message}{Environment.NewLine}{Output.Text}";
            });
        }

        private async void ListDestEvents_Click(object sender, RoutedEventArgs e)
        {
            WriteLine(" -- Searching --");
            var items = await GraphClient.Groups[DestCalendar]
                .Calendar
                .Events
                .Request().GetAsync();

            int i = 0;


            while (items != null)
            {
                foreach (var item in items)
                {
                    string message = $"{++i}) {item.Subject.Trim()} ({item.Start.DateTime} - {item.End.DateTime})";
                    WriteLine(message);
                }

                var npr = items.NextPageRequest;
                if (npr == null)
                {
                    items = null;
                }
                else
                {
                    WriteLine(" -- Next page --");
                    items = await npr.GetAsync();
                }
            }

            WriteLine(" -- Complete --");
        }

        private async void CopyEvents_Click(object sender, RoutedEventArgs e)
        {
            WriteLine(" -- Copying --");
            var items = await GraphClient.Users[SourceCalendarUser]
                  .Calendars[SourceCalendar]
                  .Events
                  .Request().GetAsync();

            int i = 0;


            while (items != null)
            {
                foreach (var item in items)
                {
                    await GraphClient.Groups[DestCalendar]
                        .Calendar
                        .Events
                        .Request().AddAsync(Cleanup(item));

                    string message = $"{++i}) {item.Subject.Trim()} ({item.Start.DateTime} - {item.End.DateTime})";
                    WriteLine(message);
                }

                var npr = items.NextPageRequest;
                if (npr == null)
                {
                    items = null;
                }
                else
                {
                    WriteLine(" -- Next page --");
                    items = await npr.GetAsync();
                }
            }

            WriteLine(" -- Complete --");
        }

        private Event Cleanup(Event item)
        {
            string tz = "Eastern Standard Time";
            
            if (string.IsNullOrEmpty(item.OriginalStartTimeZone) || item.OriginalStartTimeZone == "tzone://Microsoft/Custom")
                item.OriginalStartTimeZone = tz;

            if (string.IsNullOrEmpty(item.OriginalEndTimeZone) || item.OriginalStartTimeZone == "tzone://Microsoft/Custom")
                item.OriginalEndTimeZone = tz;

            if (item.Recurrence != null && item.Recurrence.Range != null && string.IsNullOrEmpty(item.Recurrence.Range.RecurrenceTimeZone))
            {
                item.Recurrence.Range.RecurrenceTimeZone = tz;
            }

            return item;
        }

        private async void DeleteDestEvents_Click(object sender, RoutedEventArgs e)
        {
            WriteLine(" -- Deleting --");
            var items = await GraphClient.Groups[DestCalendar]
                .Calendar
                .Events
                .Request().GetAsync();

            int i = 0;

            while (items != null)
            {
                foreach (var item in items)
                {
                    await GraphClient.Groups[DestCalendar]
                        .Calendar
                        .Events[item.Id]
                        .Request().DeleteAsync();

                    string message = $"{++i}) {item.Subject.Trim()} ({item.Start.DateTime} - {item.End.DateTime})";
                    WriteLine(message);
                }

                var npr = items.NextPageRequest;
                if (npr == null)
                {
                    items = null;
                }
                else
                {
                    WriteLine(" -- Next page --");
                    items = await npr.GetAsync();
                }
            }

            WriteLine(" -- Complete --");
        }
    }
}
