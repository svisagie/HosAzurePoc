CREATE TABLE [dbo].[DriverSummaries]
(
	[DriverId] INT NOT NULL , 
    [WorkStateId] INT NOT NULL, 
    [TotalSeconds] BIGINT NOT NULL, 
    PRIMARY KEY ([DriverId], [WorkStateId])
)
