CREATE TABLE [dbo].[DriverSummaries]
(
	[DriverId] INT NOT NULL PRIMARY KEY, 
    [WorkStateId] INT NOT NULL, 
    [TotalSeconds] BIGINT NOT NULL
)
