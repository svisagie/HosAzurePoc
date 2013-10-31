CREATE TABLE [dbo].[DriverWorkstates]
(
    [DriverWorkStateId] BIGINT NOT NULL IDENTITY,
	[DriverId] INT NOT NULL , 
    [WorkStateId] INT NOT NULL, 
    [Timestamp] DATETIME NOT NULL, 
    PRIMARY KEY ([DriverWorkStateId]), 
)
