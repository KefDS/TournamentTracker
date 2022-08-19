using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public class TeamModel : IIdentifiable
    {
        public int Id { get; set; }
        public string TeamName { get; set; }

        public List<PersonModel> TeamMembers { get; set; } = new List<PersonModel>();
    }
}
