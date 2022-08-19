USE [Tournaments]
GO

-- MatchupEntries
CREATE PROCEDURE [dbo].[spMatchupEntries_GetByMatchup] @MatchupId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [MatchupEntries].[Id]
		,[MatchupEntries].[MatchupId]
		,[MatchupEntries].[ParentMatchupId]
		,[MatchupEntries].[TeamCompetingId]
		,[MatchupEntries].[Score]
	FROM [dbo].[MatchupEntries]
	WHERE [MatchupEntries].[MatchupId] = @MatchupId;
END
GO

CREATE PROCEDURE [dbo].[spMatchupEntries_Insert] @MatchupId INT
	,@ParentMatchupId INT
	,@TeamCompetingId INT
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[MatchupEntries] (
		MatchupId
		,ParentMatchupId
		,TeamCompetingId
		)
	VALUES (
		@MatchupId
		,@ParentMatchupId
		,@TeamCompetingId
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE [dbo].[spMatchupEntries_Update] @Id INT
	,@TeamCompetingId INT = NULL
	,@Score FLOAT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[MatchupEntries]
	SET TeamCompetingId = @TeamCompetingId
		,Score = @Score
	WHERE Id = @Id;
END
GO

-- Matchups
CREATE PROCEDURE [dbo].[spMatchups_GetByTournament] @TournamentId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Matchups].[Id]
		,[Matchups].[TournamentId]
		,[Matchups].[WinnerId]
		,[Matchups].[MatchupRound]
	FROM [dbo].[Matchups]
	WHERE Matchups.TournamentId = @TournamentId
	ORDER BY Matchups.MatchupRound;
END
GO

CREATE PROCEDURE [dbo].[spMatchups_Insert] @TournamentId INT
	,@MatchupRound INT
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Matchups] (
		TournamentId
		,MatchupRound
		)
	VALUES (
		@TournamentId
		,@MatchupRound
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE [dbo].[spMatchups_Update] @Id INT
	,@WinnerId INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Matchups]
	SET WinnerId = @WinnerId
	WHERE id = @id;
END
GO

-- People
CREATE PROCEDURE [dbo].[spPeople_GetAll]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [People].[Id]
		,[People].[FirstName]
		,[People].[LastName]
		,[People].[EmailAddress]
		,[People].[CellphoneNumber]
	FROM [dbo].[People];
END
GO

CREATE PROCEDURE [dbo].[spPeople_Insert] @FirstName NVARCHAR(100)
	,@LastName NVARCHAR(100)
	,@EmailAddress NVARCHAR(100)
	,@CellphoneNumber NVARCHAR(20)
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[People] (
		FirstName
		,LastName
		,EmailAddress
		,CellphoneNumber
		)
	VALUES (
		@FirstName
		,@LastName
		,@EmailAddress
		,@CellphoneNumber
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Prizes
CREATE PROCEDURE [dbo].[spPrizes_GetByTournament] @TournamentId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Prizes].[Id]
		,[Prizes].[PlaceNumber]
		,[Prizes].[PlaceName]
		,[Prizes].[PrizeAmount]
		,[Prizes].[PrizePercentage]
	FROM [dbo].[Prizes]
	INNER JOIN [dbo].[TournamentPrizes] ON TournamentPrizes.PrizeId = Prizes.Id
	WHERE TournamentPrizes.TournamentId = @TournamentId;
END
GO

CREATE PROCEDURE [dbo].[spPrizes_Insert] @PlaceNumber INT
	,@PlaceName NVARCHAR(50)
	,@PrizeAmount MONEY
	,@PrizePercentage FLOAT
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Prizes] (
		PlaceNumber
		,PlaceName
		,PrizeAmount
		,PrizePercentage
		)
	VALUES (
		@PlaceNumber
		,@PlaceName
		,@PrizeAmount
		,@PrizePercentage
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Teams
CREATE PROCEDURE [dbo].[spTeam_getByTournament] @TournamentId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Teams].[Id]
		,[Teams].[TeamName]
	FROM [dbo].[Teams]
	INNER JOIN [dbo].[TournamentEntries] ON TournamentEntries.TeamId = Teams.Id
	WHERE [TournamentEntries].[TournamentId] = @TournamentId;
END
GO

CREATE PROCEDURE [dbo].[spTeams_GetAll]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [Teams].[Id]
		,[Teams].[TeamName]
	FROM [dbo].[Teams];
END
GO

CREATE PROCEDURE [dbo].[spTeams_Insert] @TeamName NVARCHAR(100)
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Teams] (TeamName)
	VALUES (@TeamName)

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Team Members
CREATE PROCEDURE [dbo].[spTeamMembers_GetByTeam] @TeamId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT [People].[Id]
		,[People].[FirstName]
		,[People].[LastName]
		,[People].[EmailAddress]
		,[People].[CellphoneNumber]
	FROM [dbo].[TeamMembers]
	INNER JOIN [dbo].[People] ON [People].[Id] = [TeamMembers].[PersonId]
	WHERE [TeamMembers].[TeamId] = @TeamId;
END
GO

CREATE PROCEDURE [dbo].[spTeamMembers_Insert] @TeamId INT
	,@PersonId INT
	,@id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[TeamMembers] (
		TeamId
		,PersonId
		)
	VALUES (
		@TeamId
		,@PersonId
		)

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Tournament Entries
CREATE PROCEDURE [dbo].[spTournamentEntries_Insert] @TournamentId INT
	,@TeamId INT
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[TournamentEntries] (
		TournamentId
		,TeamId
		)
	VALUES (
		@TournamentId
		,@TeamId
		)

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Tournament Prizes
CREATE PROCEDURE [dbo].[spTournamentPrizes_Insert] @TournamentId INT
	,@PrizeId INT
	,@Id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[TournamentPrizes] (
		TournamentId
		,PrizeId
		)
	VALUES (
		@TournamentId
		,@PrizeId
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO

-- Tournaments
CREATE PROCEDURE [dbo].[spTournaments_Complete] @Id INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Tournaments]
	SET Active = 0
	WHERE Id = @Id;
END
GO

CREATE PROCEDURE [dbo].[spTournaments_GetAll]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Tournaments.Id
		,Tournaments.TournamentName
		,Tournaments.EntryFee
		,Tournaments.Active
	FROM [dbo].[Tournaments]
	WHERE Active = 1;
END
GO

CREATE PROCEDURE [dbo].[spTournaments_Insert] @TournamentName NVARCHAR(200)
	,@EntryFee MONEY
	,@id INT = 0 OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Tournaments] (
		TournamentName
		,EntryFee
		,Active
		)
	VALUES (
		@TournamentName
		,@EntryFee
		,1
		);

	SELECT @Id = SCOPE_IDENTITY();
END
GO