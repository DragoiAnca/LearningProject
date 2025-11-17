namespace LearningProject.Models
{
    public class Signature
    {
        public int Id { get; set; }
        public int order { get; set; }
        public StatusDocument Status { get; set; } = StatusDocument.Nesemnat;
        public DateTime? DataSemnarii { get; set; }
        public int CerereId { get; set; }
        public Cereri Cerere { get; set; }
        public int ClaimCanSignId { get; set; }//Fk Claim este null
        public Claim ClaimCanSign { get; set; }
        public int? SignByUserId { get; set; }
        public User? SignByUser { get; set; }
    }
    public enum StatusDocument
    {
        Nesemnat = 0,
        Semnat = 1,
        Refuzat = 2
    }
}
