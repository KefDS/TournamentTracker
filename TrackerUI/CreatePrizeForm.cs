using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;

namespace TrackerUI
{
    public partial class CreatePrizeForm : Form
    {
        IPrizeRequester callingForm;

        public CreatePrizeForm(IPrizeRequester caller)
        {
            InitializeComponent();

            callingForm = caller;
        }

        private void CreatePrizeButton_Click(object sender, EventArgs e)
        {
            if (ValidateForm())
            {
                PrizeModel model = new(
                    placeNameValue.Text,
                    placeNumberValue.Text,
                    prizeAmountValue.Text,
                    prizePercentageValue.Text);

                GlobalConfig.Connection.CreatePrize(model);
                callingForm.PrizeComplete(model);

                this.Close();

                var fields = new[] {placeNameValue, placeNumberValue};
                foreach (var field in fields) field.Text = "";

                fields = new[] {prizeAmountValue, prizePercentageValue};
                foreach (var field in fields) field.Text = "0";
            }
            else
            {
                MessageBox.Show("This form has invalid information");
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;
            if (!int.TryParse(placeNumberValue.Text, out int placeNumber) &&
                placeNumber < 1) isValid = false;

            if (placeNameValue.Text.Length == 0) isValid = false;

            bool isPrizeAmountValid = decimal.TryParse(prizeAmountValue.Text, out decimal prizeAmount);
            bool isPrizePercentageValid = double.TryParse(prizePercentageValue.Text, out double prizePercentage);

            if (!isPrizeAmountValid || !isPrizePercentageValid) isValid = false;

            if (prizeAmount < 0 || prizePercentage < 0 || prizePercentage > 100) isValid = false;

            return isValid;
        }
    }
}
