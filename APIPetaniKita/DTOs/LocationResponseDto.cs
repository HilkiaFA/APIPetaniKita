namespace APIPetaniKita.DTOs
{
    public class LocationResponseDto
    {
        public int LocationId { get; set; }
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public int RegencyId { get; set; }
        public string RegencyName { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string Address { get; set; }
    }
}
