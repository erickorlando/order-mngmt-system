using MediatR;
using Vendors.Application.DTOs;

namespace Vendors.Application.Queries;

public record GetAllVendorsQuery : IRequest<IEnumerable<VendorDto>>;