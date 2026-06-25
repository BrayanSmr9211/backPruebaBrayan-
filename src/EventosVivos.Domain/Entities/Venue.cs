namespace EventosVivos.Domain.Entities;

public class Venue
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Capacidad { get; set; }
    public string Ciudad { get; set; } = string.Empty;
}
