using AutoMapper;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;

namespace EscolaOnLine.Profiles
{
    public class EnrollmentProfile : Profile
    {
        public EnrollmentProfile()
        {
            CreateMap<EnrollmentCreateDto, Enrollment>();
        }
    }
}
