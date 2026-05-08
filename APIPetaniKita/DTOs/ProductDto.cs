using System;

namespace APIPetaniKita.DTOs
{
    public class ProductRequestDto
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ProvinceId { get; set; }
        public int RegencyId { get; set; }
        public int DistrictId { get; set; }
    }

    namespace APIPetaniKita.DTOs
    {
        public class ProductResponseDto
        {
            public int ProductId { get; set; }
            public int FarmerId { get; set; }
            public string FarmName { get; set; }
            public string ProductName { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }

            public int ProvinceId { get; set; }
            public int RegencyId { get; set; }
            public int DistrictId { get; set; }

            public string ProvinceName { get; set; }
            public string RegencyName { get; set; }
            public string DistrictName { get; set; }
            public System.DateTime CreatedAt { get; set; }
            public double? DistanceKm { get; set; } // Opsional jika kamu pakai fitur nearby sebelumnya
        }
    }
}