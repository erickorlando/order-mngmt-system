using MediatR;
using Vendors.Application.DTOs;

namespace Vendors.Application.Queries;

public record GetVendorByIdQuery(Guid Id) : IRequest<VendorDto?>;