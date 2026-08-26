using EscolaOnLine.Services;
using Microsoft.AspNetCore.Mvc;

namespace EscolaOnLine
{
    public static class ServiceResultExtensions
    {
        public static IActionResult ToActionResult(this ServiceResult result)
        {
            if (result.Success)
            {
                return result.StatusCode switch
                {
                    StatusCodes.Status201Created => new StatusCodeResult(201),
                    StatusCodes.Status204NoContent => new NoContentResult(),
                    _ => new OkResult()
                };
            }

            return CreateProblemResult(result);
        }

        public static IActionResult ToActionResult<T>(this ServiceResult<T> result)
        {
            if (result.Success)
            {
                return result.StatusCode switch
                {
                    StatusCodes.Status201Created => new ObjectResult(result.Dados) { StatusCode = 201 },
                    StatusCodes.Status204NoContent => new NoContentResult(),
                    _ => new OkObjectResult(result.Dados)
                };
            }

            return CreateProblemResult(result);
        }

        private static IActionResult CreateProblemResult(ServiceResult result)
        {
            var problem = new ProblemDetails
            {
                Title = result.Title,
                Detail = result.Error,
                Status = result.StatusCode,
                Type = result.Type
            };

            // Adiciona erros de validação (422) se existirem
            if (result.Errors is not null)
            {
                problem.Extensions["errors"] = result.Errors;
            }

            return new ObjectResult(problem)
            {
                StatusCode = result.StatusCode
            };
        }
    }
}