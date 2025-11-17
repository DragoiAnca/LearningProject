namespace LearningProject.Models
{
    public partial class Cereri
    {
        //Adaugă colectia de documente(relația 1:N):
        public ICollection<Signature> Documente { get; set; } = new List<Signature>();
        public bool AllSigned => Documente != null && Documente.Any() && Documente.All(d => d.Status == StatusDocument.Semnat);

    }
}
