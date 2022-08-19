using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class CreateTeamForm : Form
    {
        private BindingList<PersonModel> availableTeamMembers = new(GlobalConfig.Connection.GetPerson_All());
        private BindingList<PersonModel> selectedTeamMembers = new();

        private readonly ITeamRequester callingForm;

        public CreateTeamForm(ITeamRequester caller)
        {
            callingForm = caller;

            InitializeComponent();
            InitializeLists();
        }

        private void InitializeLists()
        {
            selectTeamMemberDropDown.DataSource = availableTeamMembers;
            selectTeamMemberDropDown.DisplayMember = nameof(PersonModel.FullName);

            teamMemebersListBox.DataSource = selectedTeamMembers;
            teamMemebersListBox.DisplayMember = nameof(PersonModel.FullName);
        }

        private void createMemberButton_Click(object sender, EventArgs e)
        {
            if(ValidateForm())
            {
                PersonModel p = new();

                p.FirstName = firstNameValue.Text;
                p.LastName = lastNameValue.Text;
                p.EmailAddress = emailValue.Text;
                p.CellPhoneNumber = cellPhoneValue.Text;

                GlobalConfig.Connection.CreatePerson(p);

                var stringInputs = new[] { firstNameValue, lastNameValue, emailValue, cellPhoneValue };
                foreach (var s in stringInputs) s.Text = "";

                selectedTeamMembers.Add(p);
            }
            else
            {
                MessageBox.Show("You need to fill in all of fields.");
            }

        }

        private bool ValidateForm()
        {
            var stringInputs = new[] { firstNameValue.Text, lastNameValue.Text, emailValue.Text, cellPhoneValue.Text };
            if (stringInputs.Any(s => s.Length == 0)) return false;

            return true;
        }

        private void addMemberButton_Click(object sender, EventArgs e)
        {
            if (selectTeamMemberDropDown.SelectedItem != null)
            {
                PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem;

                availableTeamMembers.Remove(p);
                selectedTeamMembers.Add(p);
            }
        }

        private void removeSelectedTeamMemebersButton_Click(object sender, EventArgs e)
        {
            if(teamMemebersListBox.SelectedItem != null)
            {
                PersonModel p = (PersonModel)teamMemebersListBox.SelectedItem;
                selectedTeamMembers.Remove(p);
                availableTeamMembers.Add(p);
            }
        }

        private void createTeamButton_Click(object sender, EventArgs e)
        {
            TeamModel team = new();
            team.TeamName = teamNameValue.Text;
            team.TeamMembers = selectedTeamMembers.ToList();

            GlobalConfig.Connection.CreateTeam(team);
            callingForm.TeamComplete(team);

            Close();
        }

        private void CreateTeamForm_Load(object sender, EventArgs e)
        {

        }
    }
}
