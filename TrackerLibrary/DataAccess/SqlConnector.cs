using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using Dapper;

namespace TrackerLibrary
{
    public class SqlConnector : IDataConnection
    {
        private const string db = "Tournaments";

        public void CreatePerson(PersonModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            var p = new DynamicParameters();
            p.Add("@FirstName", model.FirstName);
            p.Add("@LastName", model.LastName);
            p.Add("@EmailAddress", model.EmailAddress);
            p.Add("@CellphoneNumber", model.CellPhoneNumber);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        public void CreatePrize(PrizeModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            var p = new DynamicParameters();
            p.Add("@PlaceNumber", model.PlaceNumber);
            p.Add("@placeName", model.PlaceName);
            p.Add("@prizeAmount", model.PrizeAmount);
            p.Add("@prizePercentage", model.PrizePercentage);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");
        }

        public void CreateTeam(TeamModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            var p = new DynamicParameters();
            p.Add("@TeamName", model.TeamName);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

            model.Id = p.Get<int>("@id");

            foreach (PersonModel person in model.TeamMembers)
            {
                p = new DynamicParameters();
                p.Add("@TeamId", model.Id);
                p.Add("@PersonId", person.Id);

                connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        public void CreateTournament(TournamentModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));

            SaveTournament(connection, model);
            SaveTournamentPrizes(connection, model);
            SaveTournamentEntries(connection, model);
            SaveTournamentRounds(connection, model);
            TournamentLogic.UpdateTournamentResults(model);
        }

        private void SaveTournament(IDbConnection connection, TournamentModel model)
        {
            var p = new DynamicParameters();
            p.Add("@TournamentName", model.TournamentName);
            p.Add("@EntryFee", model.EntryFee);
            p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

            connection.Execute("dbo.spTournaments_Insert", p, commandType: CommandType.StoredProcedure);
            model.Id = p.Get<int>("@id");
        }

        private void SaveTournamentPrizes(IDbConnection connection, TournamentModel model)
        {
            foreach (PrizeModel prize in model.Prizes)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@PrizeId", prize.Id);
                p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentPrizes_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentEntries(IDbConnection connection, TournamentModel model)
        {
            foreach (TeamModel team in model.EnteredTeams)
            {
                var p = new DynamicParameters();
                p.Add("@TournamentId", model.Id);
                p.Add("@TeamId", team.Id);
                p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTournamentEntries_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private void SaveTournamentRounds(IDbConnection connection, TournamentModel model)
        {
            foreach (List<MatchupModel> currentRound in model.Rounds)
            {
                foreach (MatchupModel currentMatchup in currentRound)
                {
                    var p = new DynamicParameters();
                    p.Add("@TournamentId", model.Id);
                    p.Add("@MatchupRound", currentMatchup.MatchupRound);
                    p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                    connection.Execute("dbo.spMatchups_Insert", p, commandType: CommandType.StoredProcedure);
                    currentMatchup.Id = p.Get<int>("@Id");

                    foreach (MatchupEntryModel entry in currentMatchup.Entries)
                    {
                        p = new();
                        p.Add("@MatchupId", currentMatchup.Id);
                        p.Add("@ParentMatchupId", entry.ParentMatchup?.Id);
                        p.Add("@TeamCompetingId", entry.TeamCompeting?.Id);
                        p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                        connection.Execute("dbo.spMatchupEntries_Insert", p, commandType: CommandType.StoredProcedure);
                        entry.Id = p.Get<int>("@Id");
                    }
                }
            }
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            output = connection.Query<TeamModel>("dbo.spTeams_GetAll").ToList();

            foreach (var team in output)
            {
                var p = new DynamicParameters();
                p.Add("@TeamId", team.Id);

                team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
            }
            return output;
        }

        public List<TournamentModel> GetTournament_All()
        {
            List<TournamentModel> output;
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            output = connection.Query<TournamentModel>("dbo.spTournaments_GetAll").ToList();
            var p = new DynamicParameters();

            List<TeamModel> allTeams = GetTeam_All();

            foreach (TournamentModel tournament in output)
            {
                p = new();
                p.Add("@TournamentId", tournament.Id);
                tournament.Prizes = connection.Query<PrizeModel>("dbo.spPrizes_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();
                tournament.EnteredTeams = connection.Query<TeamModel>("dbo.spTeam_getByTournament", p, commandType: CommandType.StoredProcedure).ToList();
                foreach (TeamModel team in tournament.EnteredTeams)
                {
                    p = new();
                    p.Add("@TeamId", team.Id);
                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }

                p = new();
                p.Add("@TournamentId", tournament.Id);
                List<MatchupModel> matchups = connection.Query<MatchupModel>("dbo.spMatchups_GetByTournament", p, commandType: CommandType.StoredProcedure).ToList();
                foreach (MatchupModel m in matchups)
                {
                    p = new();
                    p.Add("@MatchupId", m.Id);
                    m.Entries = connection.Query<MatchupEntryModel>("dbo.spMatchupEntries_GetByMatchup", p, commandType: CommandType.StoredProcedure).ToList();
                    foreach (var me in m.Entries)
                    {
                        if (me.TeamCompetingId > 0)
                            me.TeamCompeting = allTeams.Find(t => t.Id == me.TeamCompetingId);

                        if (me.ParentMatchupId > 0)
                            me.ParentMatchup = matchups.Find(m => m.Id == me.ParentMatchupId);
                    }
                }
                tournament.Rounds = matchups.GroupBy(m => m.MatchupRound).Select(x => x.ToList()).ToList();
            }

            return output;
        }

        public void UpdateMatchup(MatchupModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));

            var p = new DynamicParameters();
            if (model.Winner is not null)
            {
                p.Add("@Id", model.Id);
                p.Add("@WinnerId", model.Winner.Id);

                connection.Execute("dbo.spMatchups_Update", p, commandType: CommandType.StoredProcedure); 
            }

            foreach (MatchupEntryModel me in model.Entries)
            {
                if (me.TeamCompeting is not null)
                {
                    p = new DynamicParameters();
                    p.Add("@Id", me.Id);
                    p.Add("@TeamCompetingId", me.TeamCompeting.Id);
                    p.Add("@Score", me.Score);

                    connection.Execute("dbo.spMatchupEntries_Update", p, commandType: CommandType.StoredProcedure); 
                }
            }
        }

        public void CompleteTournament(TournamentModel model)
        {
            using IDbConnection connection = new SqlConnection(GlobalConfig.CnnString(db));
            var p = new DynamicParameters();
            p.Add("@Id", model.Id);
            connection.Execute("dbo.spTournaments_Complete", p, commandType: CommandType.StoredProcedure);
        }
    }
}
