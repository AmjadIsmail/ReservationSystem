namespace ReservationApi.Model
{
    public class AnimalModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? species { get; set; }
        public string? family { get; set; }
        public string? habitat { get; set; }
        public string? place_of_found { get; set; }
        public string? diet { get; set; }
        public string? description { get; set; }
        public decimal? weight_kg { get; set; }
        public decimal? height_cm { get; set; }
        public string? image { get; set; }
    }
}
