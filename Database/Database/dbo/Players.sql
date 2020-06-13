CREATE TABLE [dbo].[Players] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (MAX) NULL,
    [FullName]         NVARCHAR (MAX) NULL,
    [PlayingRole]      NVARCHAR (MAX) NULL,
    [DateOfBirth]      DATETIME       NOT NULL,
    [BattingStyle]     NVARCHAR (MAX) NULL,
    [BowlingStyle]     NVARCHAR (MAX) NULL,
    [CricInfoId]       INT            NOT NULL,
    [IsActive]         BIT            DEFAULT ((1)) NOT NULL,
    [CricsheetName]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Players] PRIMARY KEY CLUSTERED ([Id] ASC)
);