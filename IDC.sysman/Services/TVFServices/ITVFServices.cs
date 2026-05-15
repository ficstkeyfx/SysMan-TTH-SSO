using SysMan.Dto;
using SysMan.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IDC.Shared.Models.SysMan;

namespace SysMan.Services.ITVFServices
{
    public interface ITVFServices
    {
        Task<List<tbChucNang>> getChucNang(string apiPath,int IdUser);     // GET request
        Task<List<tbSystemUser>> getNguoiDungThuocNhom(string apiPath, int IdNhom);     // GET request
    }
}
