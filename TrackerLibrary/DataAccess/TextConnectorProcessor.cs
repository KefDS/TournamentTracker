using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            return $"{ConfigurationManager.AppSettings["filePath"]}\\{fileName}";
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);
                output.Add(p);
            }

            return output;
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new();

            foreach (PrizeModel prize in models)
            {
                lines.Add($"{prize.Id},{prize.PlaceNumber},{prize.PlaceName},{prize.PrizeAmount},{prize.PrizePercentage}");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines);
        }

        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new();
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellPhoneNumber = cols[4];
                output.Add(p);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
            List<TeamModel> teams = new();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel p = new();
                p.Id = int.Parse(cols[0]);
                p.TeamName = cols[1];

                string[] personIds = cols[2].Split('|');
                foreach (string teamMemberId in personIds)
                {
                    p.TeamMembers.Add(people.Find(person => person.Id == int.Parse(teamMemberId)));
                }

                teams.Add(p);
            }

            return teams;
        }

        public static void SaveToPersonFile(this List<PersonModel> models)
        {
            List<string> lines = new();

            foreach (PersonModel person in models)
            {
                lines.Add($"{person.Id},{person.FirstName},{person.LastName},{person.EmailAddress},{person.CellPhoneNumber}");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> models)
        {
            List<string> lines = new();

            foreach (TeamModel team in models)
            {
                string line = $"{team.Id},{team.TeamName},";
                foreach (PersonModel person in team.TeamMembers) line += $"{person.Id}|";

                if (team.TeamMembers.Count > 0) line = line[0..^1];
                lines.Add(line);
            }

            File.WriteAllLines(GlobalConfig.TeamsFile.FullFilePath(), lines);
        }

        public static List<TournamentModel> ConvertToTournamentModels(this List<string> lines)
        {
            //id,TournamentName,EntryFee,(id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)
            List<TeamModel> teams = GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            List<TournamentModel> output = new List<TournamentModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TournamentModel t = new();
                t.Id = int.Parse(cols[0]);
                t.TournamentName = cols[1];
                t.EntryFee = decimal.Parse(cols[2]);

                string[] teamIds = cols[3].Split('|');
                foreach (string teamId in teamIds)
                {
                    int id = int.Parse(teamId);
                    t.EnteredTeams.Add(teams.Find(t => t.Id == id));
                }

                string[] prizeIds = cols[4].Split('|');
                if (!(prizeIds[0] == string.Empty))
                {
                    foreach (string prizeId in prizeIds)
                    {
                        int id = int.Parse(prizeId);
                        t.Prizes.Add(prizes.Find(p => p.Id == id));
                    }
                }

                string[] rounds = cols[5].Split('|');
                foreach (string round in rounds)
                {
                    List<MatchupModel> ms = new();
                    string[] msText = round.Split('^');

                    foreach (string matchupId in msText)
                    {
                        int mId = int.Parse(matchupId);
                        ms.Add(matchups.Find(m => m.Id == mId));
                    }
                    t.Rounds.Add(ms);
                }
                output.Add(t);
            }

            return output;
        }

        private static TeamModel LookupTeamById(int id)
        {
            return GlobalConfig.TeamsFile.FullFilePath().LoadFile().ConvertToTeamModels().Find(t => t.Id == id);
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModels(this List<string> lines)
        {
            List<MatchupEntryModel> output = new();
            foreach (string line in lines)
            {
                MatchupEntryModel model = new();
                string[] cols = line.Split(',');

                // id=0,teamCompetingId=1,score=2,parentId=3
                model.Id = int.Parse(cols[0]);
                model.TeamCompeting = int.TryParse(cols[1], out int teamCompetingId) ? LookupTeamById(teamCompetingId) : null;
                model.Score = double.Parse(cols[2]);
                // It needs to load ParentMatchup without the associated entries to stop a circular reference.
                model.ParentMatchup = int.TryParse(cols[3], out int parentId) ? LookupMatchupById(parentId) : null;
                output.Add(model);
            }

            return output;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            // In order to stop a circular reference (Matchup -> Entry -> ParentMachup -> Entry...), parentMatchup will not load the entries associated.
           return GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels(withoutEntries: true).Find(m => m.Id == id);
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModels(string input)
        {
            string[] ids = input.Split('|');
            List<MatchupEntryModel> output = new();
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            foreach (string idStr in ids)
            {
                int id = int.Parse(idStr);
                output.Add(entries.Find(e => e.Id == id));
            }

            return output;
        }

        public static List<MatchupModel> ConvertToMatchupModels(this List<string> lines, bool withoutEntries = false)
        {
            //id=0,entries=1(id|id...),winner=2,matchupRound=3
            List<MatchupModel> output = new();

            foreach (string line in lines)
            {
                MatchupModel matchup = new();
                string[] cols = line.Split(',');
                
                matchup.Id = int.Parse(cols[0]);
                matchup.Entries = withoutEntries ? null : ConvertStringToMatchupEntryModels(cols[1]);
                matchup.Winner = int.TryParse(cols[2], out int winnerId) ? LookupTeamById(winnerId) : null;
                matchup.MatchupRound = int.Parse(cols[3]);
                output.Add(matchup);
            }
            return output;
        }

        public static void SaveToTournamentFile(this List<TournamentModel> models)
        {
            List<string> lines = new();

            foreach (TournamentModel tournament in models)
            {
                lines.Add($"{tournament.Id},{tournament.TournamentName},{tournament.EntryFee},{tournament.EnteredTeams.ExtractIdsFromModels()},{tournament.Prizes.ExtractIdsFromModels()},{ConvertRoundListToString(tournament.Rounds)}");
            }
            File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    matchup.SaveMatchupToFile();
                }
            }
        }

        public static void SaveMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            int currentId = 1;
            if(matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(m => m.Id).First().Id + 1;
            }
            matchup.Id = currentId;
            matchups.Add(matchup);

            foreach (MatchupEntryModel entry in matchup.Entries)
            {
                entry.SaveEntryToFile();
            }

            List<string> lines = new();
            foreach (MatchupModel m in matchups)
            {
                string winnerId = m.Winner?.Id.ToString() ?? string.Empty; 
                lines.Add($"{m.Id},{m.Entries.ExtractIdsFromModels()},{winnerId},{m.MatchupRound}");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel model)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile().ConvertToMatchupModels();

            MatchupModel matchup = matchups.Find(m => m.Id == model.Id);
            matchups.Remove(matchup);
            matchups.Add(model);

            foreach (MatchupEntryModel entry in model.Entries)
            {
                entry.UpdateEntryToFile();
            }

            List<string> lines = new();
            foreach (MatchupModel m in matchups)
            {
                string winnerId = m.Winner == null ? string.Empty : m.Winner.Id.ToString();
                lines.Add($"{m.Id},{m.Entries.ExtractIdsFromModels()},{winnerId},{m.MatchupRound}");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            int currentId = 1;
            if(entries.Count > 0)
            {
                currentId = entries.OrderByDescending(e => e.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            List<string> lines = new();
            foreach (MatchupEntryModel e in entries)
            {
                string parentMatchupId = e.ParentMatchup?.Id.ToString() ?? string.Empty;
                string teamCompetingId = e.TeamCompeting?.Id.ToString() ?? string.Empty;
                lines.Add($"{e.Id},{teamCompetingId},{e.Score},{parentMatchupId}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        public static void UpdateEntryToFile(this MatchupEntryModel model)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModels();

            MatchupEntryModel savedEntry = entries.Find(e => e.Id == model.Id);
            entries.Remove(savedEntry);
            entries.Add(model);

            List<string> lines = new();
            foreach (MatchupEntryModel e in entries)
            {
                string parentMatchupId = e.ParentMatchup?.Id.ToString() ?? string.Empty;
                string teamCompetingId = e.TeamCompeting?.Id.ToString() ?? string.Empty;
                lines.Add($"{e.Id},{teamCompetingId},{e.Score},{parentMatchupId}");
            }

            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            string output = "";

            if (rounds.Count == 0) return "";
            foreach (var round in rounds)
            {
                if (round.Count != 0)
                {
                    output += $"{round.Aggregate("", (acc, r) => acc += $"{r.Id}^")[..^1]}|";
                }
            }

            return output[..^1];
        }

        private static string ExtractIdsFromModels<T>(this List<T> models)
            where T : IIdentifiable
        {
            if (models.Count == 0) return "";
            return models.Aggregate("", (acc, m) => acc += $"{m.Id}|")[..^1];
        }
    }
}
