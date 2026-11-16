namespace PasanakuAPI.Models
{
    public class LoginRequest
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegistrarUsuarioRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UsuarioSimpleDto
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }

    public class GrupoResumenDto
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoCuota { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class CrearGrupoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoCuota { get; set; }
        public string Frecuencia { get; set; } = "Semanal";
        public DateTime FechaInicio { get; set; }
        // Lista de IDs de usuarios en el orden exacto en que cobrarán (Turno 1, Turno 2, etc.)
        public List<int> IdsUsuariosOrdenados { get; set; } = new();
    }

    public class DashboardGrupoDto
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public decimal MontoCuota { get; set; }
        public decimal PozoTotal => MontoCuota * TotalMiembros;
        public decimal MontoRecaudadoCiclo { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string FechaInicio { get; set; } = string.Empty;
        public int CicloActual { get; set; }
        public int TotalMiembros { get; set; }
        public string EstadoGrupo { get; set; } = string.Empty;
        public MiembroEstadoDto? ReceptorActual { get; set; }
        public List<MiembroEstadoDto> Miembros { get; set; } = new();
    }

    public class MiembroEstadoDto
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int TurnoCobro { get; set; }
        public string EstadoCobro { get; set; } = string.Empty;
        public bool PagoCicloActual { get; set; }
        public string? FechaPagoCicloActual { get; set; }
    }

    public class RegistrarPagoRequest
    {
        public int IdGrupo { get; set; }
        public int IdUsuario { get; set; }
    }
}