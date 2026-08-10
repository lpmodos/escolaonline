using AutoMapper;
using EscolaOnLine.Dtos;
using EscolaOnLine.Models;

namespace EscolaOnLine.Profiles
{
    public class CourseProfile : Profile    
    {
        public CourseProfile()
        {
            CreateMap<CourseCreateDto, Course>();
            CreateMap<Course, CourseReadDto>();
            CreateMap<Course, CourseReadSimplificadoDto>();

            CreateMap<CourseUpdateDto, Course>()
                .ForAllMembers(options =>
                options.Condition((src, dest, srcMember) => srcMember != null ));
        }
    }
}
