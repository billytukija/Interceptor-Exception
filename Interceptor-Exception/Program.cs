using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Interceptor_Exception
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }

    public class DetalhoErro
    {
        public int StatusCode { get; set; }
        public string Mensagem { get; set; }
    }

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _proximo;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="proximo"></param>
        public ExceptionMiddleware(RequestDelegate proximo)
        {
            _proximo = proximo;
        }

        public class LogExcecao
        {
            [Key]
            public long ExcecaoId { get; set; }
            [Required]
            public string Fonte { get; set; }
            [Required]
            public string Mensagem { get; set; }
            public string Excecao { get; set; }

            [Required]
            [Column(TypeName = "datetime")]
            public DateTime DateCadastro { get; set; }
        }

        public interface ILogExcecaoServico
        {
            void Create(LogExcecao logExcecao);
        }

        private static Task ManipulacaoExcecaoAsync(HttpContext contexto, Exception excecao)
        {
            contexto.Response.ContentType = "application/json";
            contexto.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return contexto.Response.WriteAsync(new DetalhoErro()
            {
                StatusCode = contexto.Response.StatusCode,
                Mensagem = "'Internal Server Error : ' opss.. error"
            }.ToString());
        }

        public async Task InvokeAsync(HttpContext httpContexto, ILogExcecaoServico logExcecaoServico)
        {
            try
            {
                await _proximo(httpContexto);
            }
            catch (Exception ex)
            {
                await ManipulacaoExcecaoAsync(httpContexto, ex);
            }
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();

        }
    }
}