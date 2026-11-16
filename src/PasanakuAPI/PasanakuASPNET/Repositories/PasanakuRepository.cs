using Microsoft.Data.SqlClient;
using PasanakuAPI.Models;

namespace PasanakuAPI.Repositories
{
    public class PasanakuRepository
    {
        private readonly string _connectionString;

        public PasanakuRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerConnection")
                ?? throw new InvalidOperationException("Cadena de conexión no configurada.");
        }

        // 1. Validar credenciales para Login
        public async Task<UsuarioSimpleDto?> ValidarCredencialesAsync(string correo, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT ID, Nombre + ' ' + Apellido AS NombreCompleto, Telefono, Correo
                FROM Usuarios
                WHERE Correo = @Correo AND PasswordHash = @Password;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Password", password);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UsuarioSimpleDto
                {
                    IdUsuario = reader.GetInt32(0),
                    NombreCompleto = reader.GetString(1),
                    Telefono = reader.GetString(2),
                    Correo = reader.GetString(3)
                };
            }
            return null;
        }

        // 2. Registrar nuevo usuario/participante
        public async Task<(bool Exito, string Mensaje, int IdNuevo)> RegistrarUsuarioAsync(RegistrarUsuarioRequest req)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                INSERT INTO Usuarios (Nombre, Apellido, Telefono, Correo, PasswordHash)
                OUTPUT INSERTED.ID
                VALUES (@Nombre, @Apellido, @Telefono, @Correo, @Password);";

            try
            {
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Nombre", req.Nombre.Trim());
                cmd.Parameters.AddWithValue("@Apellido", req.Apellido.Trim());
                cmd.Parameters.AddWithValue("@Telefono", req.Telefono.Trim());
                cmd.Parameters.AddWithValue("@Correo", req.Correo.Trim());
                cmd.Parameters.AddWithValue("@Password", req.Password);

                int nuevoId = (int)(await cmd.ExecuteScalarAsync())!;
                return (true, "Participante registrado correctamente.", nuevoId);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return (false, "Ya existe un usuario registrado con ese correo electrónico.", 0);
            }
        }

        // 3. Listar todos los usuarios (para asignarlos a un nuevo Pasanaku)
        public async Task<List<UsuarioSimpleDto>> ObtenerTodosLosUsuariosAsync()
        {
            var lista = new List<UsuarioSimpleDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT ID, Nombre + ' ' + Apellido, Telefono, Correo FROM Usuarios ORDER BY Nombre ASC;";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new UsuarioSimpleDto
                {
                    IdUsuario = reader.GetInt32(0),
                    NombreCompleto = reader.GetString(1),
                    Telefono = reader.GetString(2),
                    Correo = reader.GetString(3)
                });
            }
            return lista;
        }

        // 4. Listar todos los grupos de Pasanaku disponibles
        public async Task<List<GrupoResumenDto>> ObtenerGruposAsync()
        {
            var grupos = new List<GrupoResumenDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT ID, Nombre, MontoCuota, Frecuencia, Estado FROM Grupos ORDER BY ID DESC;";
            using var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                grupos.Add(new GrupoResumenDto
                {
                    IdGrupo = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    MontoCuota = reader.GetDecimal(2),
                    Frecuencia = reader.GetString(3),
                    Estado = reader.GetString(4)
                });
            }
            return grupos;
        }

        // 5. Crear un nuevo grupo de Pasanaku y sus turnos en una sola transacción
        public async Task<(bool Exito, string Mensaje, int IdGrupoCreado)> CrearGrupoConMiembrosAsync(CrearGrupoRequest req)
        {
            if (req.IdsUsuariosOrdenados == null || req.IdsUsuariosOrdenados.Count < 2)
                return (false, "Un Pasanaku necesita al menos 2 participantes asignados.", 0);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlGrupo = @"
                    INSERT INTO Grupos (Nombre, MontoCuota, Frecuencia, FechaInicio, CicloActual, Estado)
                    OUTPUT INSERTED.ID
                    VALUES (@Nombre, @MontoCuota, @Frecuencia, @FechaInicio, 1, 'Activo');";

                int nuevoIdGrupo;
                using (var cmdGrupo = new SqlCommand(sqlGrupo, conn, transaction))
                {
                    cmdGrupo.Parameters.AddWithValue("@Nombre", req.Nombre.Trim());
                    cmdGrupo.Parameters.AddWithValue("@MontoCuota", req.MontoCuota);
                    cmdGrupo.Parameters.AddWithValue("@Frecuencia", req.Frecuencia);
                    cmdGrupo.Parameters.AddWithValue("@FechaInicio", req.FechaInicio);

                    nuevoIdGrupo = (int)(await cmdGrupo.ExecuteScalarAsync())!;
                }

                string sqlMiembro = @"
                    INSERT INTO Miembros_Grupo (ID_Grupo, ID_Usuario, TurnoCobro, EstadoCobro)
                    VALUES (@IdGrupo, @IdUsuario, @TurnoCobro, 'Pendiente');";

                for (int i = 0; i < req.IdsUsuariosOrdenados.Count; i++)
                {
                    using var cmdMiembro = new SqlCommand(sqlMiembro, conn, transaction);
                    cmdMiembro.Parameters.AddWithValue("@IdGrupo", nuevoIdGrupo);
                    cmdMiembro.Parameters.AddWithValue("@IdUsuario", req.IdsUsuariosOrdenados[i]);
                    cmdMiembro.Parameters.AddWithValue("@TurnoCobro", i + 1);
                    await cmdMiembro.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return (true, "Nuevo Pasanaku creado exitosamente con todos sus turnos.", nuevoIdGrupo);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error al crear el Pasanaku: " + ex.Message, 0);
            }
        }

        // 6. Obtener el Dashboard completo de un grupo
        public async Task<DashboardGrupoDto?> ObtenerDashboardGrupoAsync(int idGrupo)
        {
            DashboardGrupoDto? dashboard = null;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sqlGrupo = @"
                SELECT ID, Nombre, MontoCuota, Frecuencia, FechaInicio, CicloActual, Estado
                FROM Grupos
                WHERE ID = @IdGrupo;";

            using (var cmdGrupo = new SqlCommand(sqlGrupo, conn))
            {
                cmdGrupo.Parameters.AddWithValue("@IdGrupo", idGrupo);
                using var reader = await cmdGrupo.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    dashboard = new DashboardGrupoDto
                    {
                        IdGrupo = reader.GetInt32(0),
                        NombreGrupo = reader.GetString(1),
                        MontoCuota = reader.GetDecimal(2),
                        Frecuencia = reader.GetString(3),
                        FechaInicio = reader.GetDateTime(4).ToString("dd/MM/yyyy"),
                        CicloActual = reader.GetInt32(5),
                        EstadoGrupo = reader.GetString(6)
                    };
                }
            }

            if (dashboard == null) return null;

            string sqlMiembros = @"
                SELECT 
                    u.ID,
                    u.Nombre + ' ' + u.Apellido AS NombreCompleto,
                    u.Telefono,
                    mg.TurnoCobro,
                    mg.EstadoCobro,
                    p.ID AS IdPago,
                    p.FechaPago
                FROM Miembros_Grupo mg
                INNER JOIN Usuarios u ON mg.ID_Usuario = u.ID
                LEFT JOIN Pagos p ON mg.ID_Grupo = p.ID_Grupo 
                                 AND mg.ID_Usuario = p.ID_Usuario 
                                 AND p.CicloCorrespondiente = @CicloActual
                WHERE mg.ID_Grupo = @IdGrupo
                ORDER BY mg.TurnoCobro ASC;";

            using (var cmdMiembros = new SqlCommand(sqlMiembros, conn))
            {
                cmdMiembros.Parameters.AddWithValue("@IdGrupo", idGrupo);
                cmdMiembros.Parameters.AddWithValue("@CicloActual", dashboard.CicloActual);

                using var reader = await cmdMiembros.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    bool yaPago = !reader.IsDBNull(5);
                    dashboard.Miembros.Add(new MiembroEstadoDto
                    {
                        IdUsuario = reader.GetInt32(0),
                        NombreCompleto = reader.GetString(1),
                        Telefono = reader.GetString(2),
                        TurnoCobro = reader.GetInt32(3),
                        EstadoCobro = reader.GetString(4),
                        PagoCicloActual = yaPago,
                        FechaPagoCicloActual = yaPago ? reader.GetDateTime(6).ToString("dd/MM/yyyy HH:mm") : null
                    });
                }
            }

            dashboard.TotalMiembros = dashboard.Miembros.Count;
            dashboard.MontoRecaudadoCiclo = dashboard.Miembros.Count(m => m.PagoCicloActual) * dashboard.MontoCuota;
            dashboard.ReceptorActual = dashboard.Miembros.FirstOrDefault(m => m.TurnoCobro == dashboard.CicloActual);

            return dashboard;
        }

        // 7. Registrar pago de cuota en el ciclo actual
        public async Task<(bool Exito, string Mensaje)> RegistrarPagoCicloActualAsync(int idGrupo, int idUsuario)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string sqlDatosGrupo = "SELECT MontoCuota, CicloActual, Estado FROM Grupos WHERE ID = @IdGrupo;";
            decimal montoCuota;
            int cicloActual;
            string estado;

            using (var cmd = new SqlCommand(sqlDatosGrupo, conn))
            {
                cmd.Parameters.AddWithValue("@IdGrupo", idGrupo);
                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return (false, "El grupo especificado no existe.");

                montoCuota = reader.GetDecimal(0);
                cicloActual = reader.GetInt32(1);
                estado = reader.GetString(2);
            }

            if (estado != "Activo")
                return (false, "El Pasanaku no se encuentra activo.");

            string sqlInsert = @"
                INSERT INTO Pagos (ID_Grupo, ID_Usuario, Monto, FechaPago, CicloCorrespondiente)
                VALUES (@IdGrupo, @IdUsuario, @Monto, GETDATE(), @Ciclo);";

            try
            {
                using var cmdInsert = new SqlCommand(sqlInsert, conn);
                cmdInsert.Parameters.AddWithValue("@IdGrupo", idGrupo);
                cmdInsert.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmdInsert.Parameters.AddWithValue("@Monto", montoCuota);
                cmdInsert.Parameters.AddWithValue("@Ciclo", cicloActual);

                await cmdInsert.ExecuteNonQueryAsync();
                return (true, "Pago registrado exitosamente.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                return (false, "Este participante ya registró su cuota en el turno actual.");
            }
        }

        // 8. Entregar pozo y avanzar al siguiente turno
        public async Task<(bool Exito, string Mensaje)> AvanzarSiguienteTurnoAsync(int idGrupo)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string sqlVerificacion = @"
                    SELECT 
                        g.CicloActual,
                        (SELECT COUNT(*) FROM Miembros_Grupo WHERE ID_Grupo = @IdGrupo) AS TotalMiembros,
                        (SELECT COUNT(*) FROM Pagos WHERE ID_Grupo = @IdGrupo AND CicloCorrespondiente = g.CicloActual) AS PagosRealizados
                    FROM Grupos g
                    WHERE g.ID = @IdGrupo;";

                int cicloActual, totalMiembros, pagosRealizados;

                using (var cmdVerif = new SqlCommand(sqlVerificacion, conn, transaction))
                {
                    cmdVerif.Parameters.AddWithValue("@IdGrupo", idGrupo);
                    using var reader = await cmdVerif.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                        return (false, "Grupo no encontrado.");

                    cicloActual = reader.GetInt32(0);
                    totalMiembros = reader.GetInt32(1);
                    pagosRealizados = reader.GetInt32(2);
                }

                if (pagosRealizados < totalMiembros)
                    return (false, $"Aún faltan {totalMiembros - pagosRealizados} pagos para cerrar este turno.");

                string sqlMarcarCobrado = @"
                    UPDATE Miembros_Grupo 
                    SET EstadoCobro = 'Cobrado' 
                    WHERE ID_Grupo = @IdGrupo AND TurnoCobro = @CicloActual;";

                using (var cmdCobrado = new SqlCommand(sqlMarcarCobrado, conn, transaction))
                {
                    cmdCobrado.Parameters.AddWithValue("@IdGrupo", idGrupo);
                    cmdCobrado.Parameters.AddWithValue("@CicloActual", cicloActual);
                    await cmdCobrado.ExecuteNonQueryAsync();
                }

                if (cicloActual >= totalMiembros)
                {
                    string sqlFinalizar = "UPDATE Grupos SET Estado = 'Finalizado' WHERE ID = @IdGrupo;";
                    using var cmdFin = new SqlCommand(sqlFinalizar, conn, transaction);
                    cmdFin.Parameters.AddWithValue("@IdGrupo", idGrupo);
                    await cmdFin.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    return (true, "¡Pozo entregado! Todos los turnos han concluido y el Pasanaku ha finalizado.");
                }
                else
                {
                    string sqlAvanzar = "UPDATE Grupos SET CicloActual = CicloActual + 1 WHERE ID = @IdGrupo;";
                    using var cmdAvanzar = new SqlCommand(sqlAvanzar, conn, transaction);
                    cmdAvanzar.Parameters.AddWithValue("@IdGrupo", idGrupo);
                    await cmdAvanzar.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    return (true, $"¡Pozo entregado! El Pasanaku avanzó al Turno #{cicloActual + 1}.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Error al procesar el cierre de turno: " + ex.Message);
            }
        }
    }
}