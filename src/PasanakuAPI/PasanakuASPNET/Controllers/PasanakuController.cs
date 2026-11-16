using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasanakuAPI.Models;
using PasanakuAPI.Repositories;

namespace PasanakuAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PasanakuController : ControllerBase
    {
        private readonly PasanakuRepository _repository;

        public PasanakuController(PasanakuRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("grupos")]
        public async Task<IActionResult> ObtenerGrupos()
        {
            var grupos = await _repository.ObtenerGruposAsync();
            return Ok(grupos);
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            var usuarios = await _repository.ObtenerTodosLosUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpPost("grupos")]
        public async Task<IActionResult> CrearGrupo([FromBody] CrearGrupoRequest request)
        {
            var resultado = await _repository.CrearGrupoConMiembrosAsync(request);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje, idGrupo = resultado.IdGrupoCreado });
        }

        [HttpGet("grupo/{idGrupo}")]
        public async Task<IActionResult> ObtenerDashboard(int idGrupo)
        {
            var data = await _repository.ObtenerDashboardGrupoAsync(idGrupo);
            if (data == null)
                return NotFound(new { mensaje = "Pasanaku no encontrado." });

            return Ok(data);
        }

        [HttpPost("pagar")]
        public async Task<IActionResult> RegistrarPago([FromBody] RegistrarPagoRequest request)
        {
            var resultado = await _repository.RegistrarPagoCicloActualAsync(request.IdGrupo, request.IdUsuario);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPost("grupo/{idGrupo}/avanzar-turno")]
        public async Task<IActionResult> AvanzarTurno(int idGrupo)
        {
            var resultado = await _repository.AvanzarSiguienteTurnoAsync(idGrupo);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}