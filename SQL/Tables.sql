-- Table creation script
USE [Tournaments]

CREATE TABLE [dbo].[People] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[FirstName] NVARCHAR(100) NOT NULL
	,[LastName] NVARCHAR(100) NOT NULL
	,[EmailAddress] NVARCHAR(200) NOT NULL
	,[CellphoneNumber] NVARCHAR(20) NULL
	)

CREATE TABLE [dbo].[Teams] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TeamName] NVARCHAR(100) NOT NULL
	)

CREATE TABLE [dbo].[Tournaments] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TournamentName] NVARCHAR(200) NOT NULL
	,[EntryFee] MONEY NOT NULL
	,[Active] BIT NOT NULL
	)

CREATE TABLE [dbo].[Prizes] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[PlaceNumber] INT NOT NULL
	,[PlaceName] NVARCHAR(50) NOT NULL
	,[PrizeAmount] MONEY NOT NULL
	,[PrizePercentage] FLOAT NOT NULL
	)

CREATE TABLE [dbo].[TeamMembers] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TeamId] INT NOT NULL
	,[PersonId] INT NOT NULL
	,CONSTRAINT [FK_TeamMembers_ToTeams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([Id])
	,CONSTRAINT [FK_TeamMembers_ToPeople] FOREIGN KEY ([PersonId]) REFERENCES [People]([Id])
	)

CREATE TABLE [dbo].[Matchups] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TournamentId] INT NOT NULL
	,[WinnerId] INT NULL
	,[MatchupRound] INT NOT NULL
	,CONSTRAINT [FK_Matchups_ToTeams] FOREIGN KEY ([WinnerId]) REFERENCES [Teams]([Id])
	,CONSTRAINT [FK_Matchups_ToTournaments] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments]([Id])
	)

CREATE TABLE [dbo].[MatchupEntries] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[MatchupId] INT NOT NULL
	,[ParentMatchupId] INT NULL
	,[TeamCompetingId] INT NULL
	,[Score] DECIMAL NULL
	,CONSTRAINT [FK_MatchupEntries_ToMatchups] FOREIGN KEY ([MatchupId]) REFERENCES [Matchups]([Id])
	,CONSTRAINT [FK_MatchupEntries_ToParentMatchups] FOREIGN KEY ([ParentMatchupId]) REFERENCES [Matchups]([Id])
	,CONSTRAINT [FK_MatchupEntries_ToTeams] FOREIGN KEY ([TeamCompetingId]) REFERENCES [Teams]([Id])
	)

CREATE TABLE [dbo].[TournamentEntries] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TournamentId] INT NOT NULL
	,[TeamId] INT NOT NULL
	,CONSTRAINT [FK_TournamentEntries_ToTournaments] FOREIGN KEY ([TournamentId]) REFERENCES Tournaments([Id])
	,CONSTRAINT [FK_TournamentEntries_ToTeams] FOREIGN KEY ([TeamId]) REFERENCES [Teams]([Id])
	)

CREATE TABLE [dbo].[TournamentPrizes] (
	[Id] INT NOT NULL PRIMARY KEY IDENTITY
	,[TournamentId] INT NOT NULL
	,[PrizeId] INT NOT NULL
	,CONSTRAINT [FK_TournamentPrizes_ToTournaments] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments]([Id])
	,CONSTRAINT [FK_TournamentPrizes_ToPrizes] FOREIGN KEY ([PrizeId]) REFERENCES [Prizes]([Id])
	)
