CREATE TABLE [dbo].[TrackerEvent] (
    [id] BIGINT NOT NULL PRIMARY KEY,
    [name] NVARCHAR(255) NOT NULL,
    [gsmmcc] NVARCHAR(50) NULL,
    [trackerstate] NVARCHAR(100) NULL,
    [rolllsonfoot] NVARCHAR(50) NULL,
    [planthestep] NVARCHAR(100) NULL,
    [batterylevel] NVARCHAR(50) NULL,
    [gsmlevel] NVARCHAR(50) NULL,
    [satlevel] NVARCHAR(50) NULL,
    [direction] NVARCHAR(50) NULL,
    [elevation] NVARCHAR(50) NULL,
    [speeddec] DECIMAL(18, 2) NULL,
    [lnglogo] NVARCHAR(100) NULL,
    [gsmmnc] NVARCHAR(50) NULL,
    [lng] FLOAT NOT NULL,
    [lat] FLOAT NOT NULL,
    [gpsdataeffective] NVARCHAR(50) NULL,
    [gpstime] NVARCHAR(50) NULL,
    [gpsdate] NVARCHAR(50) NULL,
    [equipmentid] NVARCHAR(100) NULL,
    [state] NVARCHAR(50) NULL,
    [user] NVARCHAR(100) NULL,
    [createdat] DATETIME NOT NULL,
    [title] NVARCHAR(255) NULL,
    [latlogo] NVARCHAR(100) NULL,
    [gps_data] NVARCHAR(4000) NULL,    
    [address] NVARCHAR(255) NULL,    
    [address_street] NVARCHAR(255) NULL,    
    [address_province] NVARCHAR(255) NULL,    
    [address_country] NVARCHAR(255) NULL,    
    [address_postalcode] NVARCHAR(255) NULL,    
    [address_resolved] BIT NOT NULL DEFAULT 0,     
    [vehicle_id] UNIQUEIDENTIFIER,
);

CREATE TABLE [dbo].[Drivers] (
    [driver_id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [driver_name] NVARCHAR(255) NOT NULL,
    [driver_phone] NVARCHAR(50) NULL,
    [driver_email] NVARCHAR(255) NULL
);

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

ALTER TABLE [dbo].[Vehicle]
ADD CONSTRAINT FK_Vehicle_Drivers
FOREIGN KEY ([driver_id])
REFERENCES [dbo].[Drivers] ([driver_id]);

INSERT INTO Drivers (driver_name, driver_phone, driver_email) 
VALUES ('Alberto Valenti', '+393274776313', 'alberto.valenti@gmail.com')     
INSERT INTO Drivers (driver_name, driver_phone, driver_email) 
VALUES ('John Doe', '+393398050711', 'alberto.valenti@gmail.com')     

DECLARE @driver_id UNIQUEIDENTIFIER
SELECT @driver_id =  driver_id FROM Drivers WHERE driver_name = 'Alberto Valenti'
INSERT INTO Vehicle (vehicle_name, driver_name, vehicle_numberplate, gps_equipmentid, gps_phone, driver_id)
VALUES ('Peugeot 2008', 'Alberto Valenti', 'GR422VL', '9058723497', '+423663940084040', @driver_id)     

--DECLARE @driver_id UNIQUEIDENTIFIER
SELECT @driver_id =  driver_id FROM Drivers WHERE driver_name = 'John Doe'
INSERT INTO Vehicle (vehicle_name, driver_name, vehicle_numberplate, gps_equipmentid, gps_phone, driver_id) 
VALUES ('Kymco Agility 150', 'John Doe', 'FB42668', '9059109989', '+423663940084041', @driver_id)     

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[find_position_history_by_date] 
@vehicle_id uniqueidentifier, 
@date Date = TOODAY
AS
BEGIN
SELECT 
      [vehicle_id]
      ,[address]
      ,[address_street]
      ,[address_province]
      ,[address_country]
      ,[address_postalcode]
      ,[createdat]
  FROM [dbo].[TrackerEvent]
  WHERE address_street IS NOT NULL  
  AND [vehicle_id] = @vehicle_id
  AND DATEPART(year,createdat) =  DATEPART(year,@Date)
  AND DATEPART(month,createdat) =  DATEPART(month,@Date)
  AND DATEPART(day,createdat) =  DATEPART(day,@Date)
  ORDER BY [createdat] DESC
  
END

CREATE TABLE [dbo].[claims]
(
    [claim_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [vehicle_id] UNIQUEIDENTIFIER NOT NULL,
    [claim_date] DATETIME NOT NULL DEFAULT GETDATE(),
    [claim_details] NVARCHAR(MAX) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([claim_id] ASC)
);

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
ALTER TABLE [dbo].[maintenances]
ADD CONSTRAINT FK_maintenances_Vehicles
FOREIGN KEY ([vehicle_id])
REFERENCES [dbo].[Vehicle] ([vehicle_id]);

