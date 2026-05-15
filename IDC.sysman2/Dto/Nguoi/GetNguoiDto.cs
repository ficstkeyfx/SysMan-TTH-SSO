namespace SysMan.Dto.Nguoi
{
    public class GetNguoiDto
    {
        public int IdNguoi { get; set; }
        public string HoDem { get; set; } = null!;
        public string Ten { get; set; } = null!;
        public string? DienThoai { get; set; }
        public string? GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? QueQuan { get; set; }
        public int? IdDanToc { get; set; }
        public int? IdTonGiao { get; set; }
        public string? ChoOHienNay { get; set; }
        public string? NoiSinh { get; set; }
        public string? HKTT { get; set; }
        public string? DiaChi { get; set; }
        public string? Email { get; set; }

    }
}
