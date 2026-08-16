using AutoMapper;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;

namespace EscolaOnLine.Profiles
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<StudentCreateDto, RegisterDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Student"));

            CreateMap<Student, StudentReadDto>();
            CreateMap<Student, StudentReadSimplificadoDto>();

            CreateMap<StudentUpdateDto, Student>();

        }
    }
}
