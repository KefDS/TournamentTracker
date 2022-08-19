namespace TrackerLibrary
{
    public class PersonModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string CellPhoneNumber { get; set; }

        public string FullName { 
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }
}