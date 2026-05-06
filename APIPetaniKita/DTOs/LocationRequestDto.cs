namespace APIPetaniKita.DTOs
{
    public class LocationRequestDto
    {
        public int ProvinceId { get; set; }
        public int RegencyId { get; set; }
        public int DistrictId { get; set; }
        public string Address { get; set; }
    }
}
