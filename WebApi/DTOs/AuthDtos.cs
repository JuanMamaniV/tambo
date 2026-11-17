using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs
{
    // Servicio 12: Iniciar Sesión de Usuario
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public ClienteResponseDto? Cliente { get; set; }
        public string? Token { get; set; }
    }

    // Servicio 13: Registro de Nuevo Usuario
    public class RegistroClienteDto
    {
        [Required(ErrorMessage = "El tipo de documento es obligatorio (DNI, CE, PASAPORTE).")]
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de celular es obligatorio.")]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class ClienteResponseDto
    {
        public int ClienteId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    // Servicio 14: Listar Direcciones Guardadas del Usuario
    public class DireccionDto
    {
        public int DireccionId { get; set; }
        public int ClienteId { get; set; }
        public int? ZonaId { get; set; }
        public string? ZonaNombre { get; set; }
        public string Alias { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string DireccionTexto { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public string? DepartamentoInterior { get; set; }
        public string? Referencia { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public bool Principal { get; set; }
    }

    public class CrearDireccionDto
    {
        [Required]
        public string Alias { get; set; } = "Casa";

        [Required]
        public string Distrito { get; set; } = string.Empty;

        [Required]
        public string DireccionTexto { get; set; } = string.Empty;

        public string? Numero { get; set; }
        public string? DepartamentoInterior { get; set; }
        public string? Referencia { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public bool Principal { get; set; } = false;
    }
}
