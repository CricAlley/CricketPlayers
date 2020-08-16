CREATE TABLE [dbo].[ExcludedTeams]
(
	[Id]               INT            IDENTITY (1, 1) NOT NULL,    
    [IsActive]         BIT            DEFAULT ((1)) NOT NULL,
    [TeamName]         NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_dbo.ExcludedTeams] PRIMARY KEY CLUSTERED ([Id] ASC)
)
