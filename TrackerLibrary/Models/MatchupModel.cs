using System.Collections.Generic;

namespace TrackerLibrary
{
    public class MatchupModel
    {
        public int Id { get; set; }

        public List<MatchupEntryModel> Entries { get; set; } = new List<MatchupEntryModel>();

        public int WinnerId { get; set; }

        public TeamModel Winner { get; set; }

        public int MatchupRound { get; set; }

        public string DisplayName {
            get
            {
                string output = string.Empty;
                foreach(MatchupEntryModel me in Entries)
                {
                    if (output.Length == 0) 
                        output = me.TeamCompeting?.TeamName ?? " - ";
                    else 
                        output += $" vs {me.TeamCompeting?.TeamName ?? " - "}";
                }
                return output;
            } 
        }
    }
}