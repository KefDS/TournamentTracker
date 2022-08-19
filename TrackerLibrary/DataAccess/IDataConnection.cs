using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerLibrary
{
    public interface IDataConnection
    {
        void CreatePrize(PrizeModel model);
        void CreatePerson(PersonModel model);
        void CreateTeam(TeamModel model);
        void UpdateMatchup(MatchupModel model);
        void CompleteTournament(TournamentModel model);
        void CreateTournament(TournamentModel model);
        List<PersonModel> GetPerson_All();
        List<TeamModel> GetTeam_All();
        List<TournamentModel> GetTournament_All();
    }
}
