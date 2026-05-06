namespace APIPetaniKita.DTOs
{
    public class FarmerProfileRequestDto
    {
        public string FarmName { get; set; }
        public string Description { get; set; }
        public int ProvinceId { get; set; }
        public int RegencyId { get; set; }
        public int DistrictId { get; set; }
        public string Address { get; set; }
    }
}
