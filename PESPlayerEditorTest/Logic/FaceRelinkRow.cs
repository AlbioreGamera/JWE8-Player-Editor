using PlayerLibrary;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Fila de la grilla mostrada en <see cref="PESPlayerEditorTest.FaceRelinkWindow"/>:
    /// combina el FaceId/Código de cara preestablecida con el nombre del jugador real
    /// asociado (si existe) y la lista de jugadores de la base de datos actual que
    /// tienen ese FaceId asignado (bajo el mismo SkinColour).
    /// </summary>
    public class FaceRelinkRow
    {
        public int FaceId { get; set; }
        public string? FaceCode { get; set; }
        public string? FaceName { get; set; }
        public string? AssignedPlayers { get; set; }
    }
}