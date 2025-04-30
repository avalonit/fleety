using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Memory;
using AITrackerAgent.Classes;
using AITrackerAgent.Interfaces;
using System.Data;

#pragma warning disable SKEXP0001

namespace AITrackerAgent.Services;

public class AgentSessionPlugin(Kernel kernel, ISemanticTextMemory memory, IAddressService addressService, ICommunicationService communicationService, string connectionString)
{   
    //private readonly ILogger logger = logger;
    private readonly Kernel kernel = kernel;    
    private readonly ISemanticTextMemory memory = memory;
    private readonly string connectionString = connectionString;
    private readonly IAddressService addressService = addressService;
    private readonly ICommunicationService communicationService = communicationService;

    [KernelFunction("query_vehicles_table")]
    [Description("""
        Query the database to find driver's data
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[Vehicle]
        COLUMNS:
        [vehicle_id]: internal vehicle id
        [vehicle_name]: vehicle brand and model
        [driver_name]: driver name                 
        [vehicle_numberplate]: vehicle numberplate
        [gps_lat]: vehicle latitude
        [gps_lng]: vehicle longitude
        [gps_batterylevel]: vehicle battery level
        [lastupdatedat]: last time position updated
        """)]
    public async Task<IEnumerable<dynamic>> QueryVehiclesTable(string logical_sql_query)
    {        
        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains driver information
 
        CREATE TABLE [dbo].[Vehicle] (
            [vehicle_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY  DEFAULT NEWID(), 
            [driver_id] UNIQUEIDENTIFIER,
            [vehicle_name] NVARCHAR(255) NOT NULL,
            [driver_name] NVARCHAR(255) NOT NULL,
            [vehicle_numberplate] NVARCHAR(255) NOT NULL,
            [gps_phone] NVARCHAR(100) NOT NULL,
            [gps_equipmentid] NVARCHAR(100) NOT NULL,
            [gps_batterylevel] NVARCHAR(50) NULL,
            [gps_gsmlevel] NVARCHAR(50) NULL,
            [gps_satlevel] NVARCHAR(50) NULL,
            [gps_direction] NVARCHAR(50) NULL,
            [gps_speeddec] DECIMAL(18, 2) NULL,
            [gps_lnglogo] NVARCHAR(100) NULL,
            [gps_lng] FLOAT NULL,
            [gps_lat] FLOAT NULL,
            [gps_gpstime] NVARCHAR(50) NULL,
            [gps_gpsdate] NVARCHAR(50) NULL,
            [createdat] DATETIME NOT NULL DEFAULT GETDATE(),
            [lastupdatedat] DATETIME NULL,
        );
        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
            return [];

        var sqlQuery = GenerateSQL(response.Content);
        //Console.WriteLine($"Executing the following query: {sqlQuery}");
        
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

   
    [KernelFunction("query_drivers_table")]
    [Description("""
        Query the database to find drivers data for vehicles
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[Drivers]
        COLUMNS:
        [driver_id]: driver id
        [driver_name]: driver name
        [driver_phone]: driver phone
        [driver_email]: driver email
        """)]
    public async Task<IEnumerable<dynamic>> QueryDriversTable(string logical_sql_query)
    {        
        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains the drivers of the vehicles
        CREATE TABLE [dbo].[Drivers] (
            [driver_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
            [driver_name] NVARCHAR(255) NOT NULL,
            [driver_phone] NVARCHAR(50) NULL,
            [driver_email] NVARCHAR(255) NULL
        );       

        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
            return [];

        var sqlQuery = GenerateSQL(response.Content);
        //Console.WriteLine($"Executing the following query: {sqlQuery}");

        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    [KernelFunction("find_address_by_coordinates")]
    [Description("Return the address of a specified latitude and longitude.")]
    public async Task<string> GetAddressFromCordinates([Description("Latitude.")] double lat, [Description("Longitude.")] double lng)
    {
        return await addressService.GetFormattedAddressFromCordinates(lat, lng);
    }

    [KernelFunction("show_bingmap_by_coordinates")]
    [Description("Returns the bing maps link to open the specified longitude and latitude in a map.")]
    public async Task<BingData> ShowMapCoordinatesOnBingMap([Description("Latitude.")] double lat, [Description("Longitude.")] double lng)
    {
        var bingData = new BingData()
        { 
            Address = await GetAddressFromCordinates(lat, lng),
            BingUrl = $"https://www.bing.com/maps?cp={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}%7E{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lvl=16.7"
        };
        return bingData;
    }

    [KernelFunction("show_googlemap_by_coordinates")]
    [Description("Returns the google maps link to open the specified longitude and latitude in a map.")]
    public async Task<BingData> ShowCoordinatesOnGoogleMap([Description("Latitude.")] double lat, [Description("Longitude.")] double lng)
    {
        var bingData = new BingData()
        {
            Address = await GetAddressFromCordinates(lat, lng),
            BingUrl = $"https://maps.google.com/?q={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}&z=15%7E"
        };
        return bingData;
    }

    [KernelFunction("send_email_driver")]
    [Description("Send an email to the driver.")]
    public async Task SendEmail([Description("Latitude.")] double lat, 
        [Description("Longitude.")] double lng, 
        [Description("Email address of the driver.")] string emailTo, 
        [Description("Full name of the driver.")] string driverName)
    {
        var subject = "Vehicle position";
        var coord = await ShowCoordinatesOnGoogleMap(lat, lng);
        if (coord != null)
        {
            var htmlBody = $"<p>Dear {driverName},</p><p>Your vehicle is located at this address <a href=\"{coord.BingUrl}\">{coord.Address}</a> at the following coordinates {lat} {lng}</p>";
            var plainBody = $"Dear {driverName},\nYour vehicle is located at the following address {coord.Address} ({lat},{lng})";
            await communicationService.SendMail(emailTo, driverName, subject, htmlBody, plainBody);
        }
    }

    [KernelFunction("find_history_locations_by_date")]
    [Description("Return location history for a vehicle based on the vehicle id on a specified date. If date is not provided, return all locations.")]
    public async Task<IEnumerable<LocationHistory>> GetCustomerInteractions(Guid vehicle_id, DateTime? date)
    {        
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        await using var connection = new SqlConnection(connectionString);
        var isDateValid = date!=null && date.Value.Year >= DateTime.Now.Year-1;
        var locations = await connection.QueryAsync<LocationHistory>("dbo.find_position_history_by_date", 
            new { 
                vehicle_id,
                date = isDateValid ? date : DateTime.Now
            }, 
            commandType: CommandType.StoredProcedure
        );
        return locations;    
    }


    [KernelFunction("query_maintenance_table")]
    [Description("""
        Query the database to find maintenance data for vehicles
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[maintenances]
        COLUMNS:
        [maintenance_id]: internal maintenance id
        [vehicle_id]: vehicle id
        [maintenance_date]: maintenance date
        [maintenance_details]: details and notes about the maintenance
        [maintenance_completed]: is maintenance completed
        """)]
    public async Task<IEnumerable<dynamic>> QueryMaintenanceTable(string logical_sql_query)
    {
        Console.WriteLine($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains vehicle's maintenances information
        CREATE TABLE [dbo].[maintenances]
        (
            [maintenance_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            [vehicle_id] UNIQUEIDENTIFIER NOT NULL,
            [maintenance_date] DATETIME NOT NULL DEFAULT GETDATE(),
            [maintenance_details] NVARCHAR(MAX) NOT NULL,
            [maintenance_usernote] NVARCHAR(MAX) NOT NULL,
            [maintenance_completed] BIT NOT NULL DEFAULT 0,
            PRIMARY KEY NONCLUSTERED ([maintenance_id] ASC)
        );       

        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
            return [];

        var sqlQuery = GenerateSQL(response.Content);
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }


    [KernelFunction("query_claim_table")]
    [Description("""
        Query the database to find claim data for vehicles
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[claim]
        COLUMNS:
        [claim_id]: internal claim id
        [vehicle_id]: vehicle id
        [claim_date]: claim date
        [claim_details]: details and notes about the claim
        """)]
    public async Task<IEnumerable<dynamic>> QueryClaimTable(string logical_sql_query)
    {
        Console.WriteLine($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains vehicle's claim information
           CREATE TABLE [dbo].[claims]
        (
            [claim_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            [vehicle_id] UNIQUEIDENTIFIER NOT NULL,
            [claim_date] DATETIME NOT NULL DEFAULT GETDATE(),
            [claim_details] NVARCHAR(MAX) NOT NULL,
            PRIMARY KEY NONCLUSTERED ([claim_id] ASC)
        );    
        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
            return [];

        var sqlQuery = GenerateSQL(response.Content);
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    [KernelFunction("create_claim_record")]
    [Description("""
        Create a new record on database to insert a new claim data for vehicles
        The high-level schema of the database is the following:
        
        TABLE: [dbo].[claim]
        COLUMNS:
        [claim_id]: internal claim id
        [vehicle_id]: vehicle id
        [claim_date]: claim date
        [claim_details]: details and notes about the claim
        """)]
    public async Task<IEnumerable<dynamic>> CreateClaim(string logical_sql_query)
    {
        Console.WriteLine($"Querying the database for '{logical_sql_query}'");

        var ai = kernel.GetRequiredService<IChatCompletionService>();
        var chat = new ChatHistory(@"You create T-SQL queries based on the given user request and the provided schema. Just return T-SQL query to be executed. Do not return other text or explanation. Don't use markdown or any wrappers.
        The database schema is the following:

        // this table contains vehicle's claim information
        CREATE TABLE [dbo].[claims]
        (
            [claim_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            [vehicle_id] UNIQUEIDENTIFIER NOT NULL,
            [claim_date] DATETIME NOT NULL DEFAULT GETDATE(),
            [claim_details] NVARCHAR(MAX) NOT NULL,
            PRIMARY KEY NONCLUSTERED ([claim_id] ASC)
        );    

        ");

        chat.AddUserMessage(logical_sql_query);
        var response = await ai.GetChatMessageContentAsync(chat);
        if (response.Content == null)
            return [];

        var sqlQuery = GenerateSQL(response.Content);
        await using var connection = new SqlConnection(connectionString);
        var result = await connection.QueryAsync(sqlQuery);

        return result;
    }

    private string GenerateSQL(string reponse)
    {
        return reponse.Replace("```sql", string.Empty).Replace("```", string.Empty);
    }


}