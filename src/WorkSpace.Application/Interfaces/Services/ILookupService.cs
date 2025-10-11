using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Lookup;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Interfaces.Services;

public interface ILookupService
{
    Task<Response<List<WardDto>>> GetAllWardsAsync(string? q = null, int? take = null);
}
