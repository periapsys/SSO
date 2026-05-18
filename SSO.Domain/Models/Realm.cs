namespace SSO.Domain.Models
{
    public class Realm
    {
        public Guid RealmId { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = "admin";
        public DateTime DateModified { get; set; } = DateTime.Now;
        public string ModifiedBy { get; set; } = "admin";
        public DateTime? DateInactive { get; set; }

        public virtual ICollection<Application> Applications { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<RealmUser> Users { get; set; }
        public virtual ICollection<RealmIdpSettings> IdpSettingsCollection { get; set; }
        public virtual RealmMailerSettings RealmMailerSettings { get; set; }
    }
}
