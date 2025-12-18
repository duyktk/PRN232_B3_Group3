using Repository.Models;
using Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IGradeService
    {
        Task<GradeResultDto> CreateGradeAsync(int myUserId, string myRole, GradeUpsertDto dto);
        Task<GradeResultDto> UpdateGradeAsync(int myUserId, string myRole, GradeUpsertDto dto);
    }
}
